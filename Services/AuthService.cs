using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SvendeApi.Data;
using SvendeApi.DTO;
using SvendeApi.Interface;
using SvendeApi.Models;
using SvendeApi.Utilities;

namespace SvendeApi.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly IHostEnvironment _env;
    private readonly IConfiguration _config;
    public AuthService(AppDbContext context, IMapper mapper, ILogger<AuthService> logger, IHostEnvironment env, IConfiguration config)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _env = env;
        _config = config;
    }

    public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDTO, string? ipAddress = null)
    {
        var email = loginDTO.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            throw new UnauthorizedAccessException("Invalid email address");

        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles!)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            _logger.LogWarning("Invalid email address or password: {Email}", email);
            throw new UnauthorizedAccessException(_env.IsDevelopment() ? "User not found" : "Invalid credentials");
        }
        if (!PasswordHasher.VerifyPassword(loginDTO.Password, user.Password))
        {
            _logger.LogWarning("Login failed: password mismatch for user {UserId}", user.UserId);
            throw new UnauthorizedAccessException(_env.IsDevelopment() ? "Password mismatch" : "Invalid credentials");
        }
        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: user is not active: {UserId}", user.UserId);
            throw new UnauthorizedAccessException("User is deactivated");
        }

        var (token, expiresAt) = GenerateAccessTokenForUser(user);
        var (refreshToken, _) = await GenerateRefreshTokenForUserAsync(user, ipAddress);

        var response = _mapper.Map<AuthResponseDTO>(user);
        response.Token = token;
        response.RefreshToken = refreshToken;
        response.ExpiresAt = expiresAt;
        return response;
    }

    public async Task LogoutAsync(string refreshToken, string? ipAddress = null)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        if (token == null)
        {
            _logger.LogWarning("Logout: refresh token not found (prefix={Prefix})",
                string.IsNullOrEmpty(refreshToken) ? "" : refreshToken.Substring(0, Math.Min(8, refreshToken.Length)));
            return;
        }
        if (token.RevokedAt != null)
        {
            _logger.LogWarning("Logout: refresh token already revoked at {RevokedAt}", token.RevokedAt);
            return;
        }
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        _logger.LogInformation("Logout: token revoked for user {UserId} at {RevokedAt}", token.UserId, token.RevokedAt);
        await _context.SaveChangesAsync();
    }

    public async Task<AuthResponseDTO> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
    {
        var token = await _context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        if (token.RevokedAt != null)
            throw new UnauthorizedAccessException("Refresh token revoked");

        if (token.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired");

        var user = token.User;
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid user");

        var newToken = await GenerateRefreshTokenForUserAsync(user, ipAddress, token.Token);
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReplacedByToken = newToken.token;

        await _context.SaveChangesAsync();

        var (jwt, expiresAt) = GenerateAccessTokenForUser(user);
        var response = _mapper.Map<AuthResponseDTO>(user);
        response.Token = jwt;
        response.RefreshToken = newToken.token;
        response.ExpiresAt = expiresAt;
        return response;
    }
    public async Task RevokeAllTokensAsync(Guid userId, string? reason = null, string? ipAddress = null)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync();
        var now = DateTime.UtcNow;
        foreach (var token in tokens)
        {
            token.RevokedAt = now;
            token.RevokedByIp = ipAddress;
            token.RevocationReason = reason;
        }
        await _context.SaveChangesAsync();
    }


    private (string token, DateTime expiresAt) GenerateAccessTokenForUser(UserModel user)
    {
        var key = _config["Jwt:Key"];
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];
        var minutes = int.TryParse(_config["Jwt:AccessTokenExpiresInMinutes"], out var m) ? m : 30;

        if (string.IsNullOrEmpty(key))
            throw new InvalidOperationException("Jwt:Key is not configured");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Email)
        };
        var roleNames = user.UserRoles?
            .Select(ur => ur.Role?.RoleName)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            ?? Enumerable.Empty<string?>();
        foreach (var role in roleNames)
        {
            claims.Add(new Claim(ClaimTypes.Role, role!));
        }

        var expires = DateTime.UtcNow.AddMinutes(minutes);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );
        var tokenHandler = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenHandler, expires);
    }
    private async Task<(string token, DateTime expiresAt)> GenerateRefreshTokenForUserAsync(UserModel user, string? ipAddress, string? replacedToken = null)
    {
        var days = int.TryParse(_config["Jwt:RefreshTokenDays"], out var d) ? d : 7;
        var newToken = new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = user.UserId,
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(days),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            ReplacedByToken = null,
            RevokedAt = null,
        };

        _context.RefreshTokens.Add(newToken);
        await _context.SaveChangesAsync();
        return (newToken.Token, newToken.ExpiresAt);
    }

    private static string GenerateSecureToken()
    {
        Span<byte> buffer = stackalloc byte[64];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToBase64String(buffer).TrimEnd('=');
    }
}

