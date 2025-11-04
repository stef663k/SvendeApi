using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SvendeApi.Data;
using SvendeApi.DTO;
using SvendeApi.Interface;

namespace SvendeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableCors("Default")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;
    public AuthController(AppDbContext context, IAuthService authService, IMapper mapper)
    {
        _context = context;
        _authService = authService;
        _mapper = mapper;
    }

    [HttpGet("session")]
    public async Task<IActionResult> GetSession()
    {
        try
        {
            var ipAddress = GetCLientIpAddress();
            if (!Request.Cookies.TryGetValue("refreshToken", out var token) || string.IsNullOrWhiteSpace(token))
            {
                return Unauthorized(new { message = "No refresh token found" });
            }

            var result = await _authService.RefreshTokenAsync(token, ipAddress);
            var cookieOptions = BuildRefreshCookieOptions(result.ExpiresAt);
            Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while refreshing session", error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDTO loginDTO)
    {
        try
        {
            var ipAddress = GetCLientIpAddress();
            var result = await _authService.LoginAsync(loginDTO, ipAddress);
            var cookieOptions = BuildRefreshCookieOptions(result.ExpiresAt);
            Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while logging in", error = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequestDTO refreshRequestDTO)
    {
        try
        {
            var ipAddress = GetCLientIpAddress();
            var token = refreshRequestDTO?.RefreshToken;
            if (string.IsNullOrWhiteSpace(token))
            {
                Request.Cookies.TryGetValue("refreshToken", out token);
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                return Unauthorized(new { message = "No refresh token found" });
            }
            var result = await _authService.RefreshTokenAsync(token, ipAddress);
            var cookieOptions = BuildRefreshCookieOptions(result.ExpiresAt);
            Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while refreshing session", error = ex.Message });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequestDTO logoutRequestDTO)
    {
        try
        {
            var ipAddress = GetCLientIpAddress();
            var token = logoutRequestDTO?.RefreshToken;
            if (string.IsNullOrWhiteSpace(token))
            {
                Request.Cookies.TryGetValue("refreshToken", out token);
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                return Unauthorized(new { message = "No refresh token found" });
            }
            await _authService.LogoutAsync(token!, ipAddress);
            var cookieOptions = BuildRefreshCookieOptions(DateTime.UtcNow);
            Response.Cookies.Delete("refreshToken", cookieOptions);
            return Ok(new { message = "Logout successful" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while logging out", error = ex.Message });
        }
    }

    private string? GetCLientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
            ?? User.FindFirst("sub")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId)) return userId;
        throw new UnauthorizedAccessException("Invalid user ID in token");
    }

    private static CookieOptions BuildRefreshCookieOptions(DateTime expiresAtUtc)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = expiresAtUtc,
            Path = "/"
        };
    }
}