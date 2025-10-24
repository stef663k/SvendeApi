using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SvendeApi.DTO;

namespace SvendeApi.Interface;

public interface IAuthService
{
    Task<AuthResponseDTO> LoginAsync(LoginDTO loginDTO, string? ipAddress = null);
    Task<AuthResponseDTO> RefreshTokenAsync(string refreshToken, string? ipAddress = null);
    Task LogoutAsync(string refreshToken, string? ipAddress = null);
    Task RevokeAllTokensAsync(Guid userId, string? reason = null, string? ipAddress = null);
}