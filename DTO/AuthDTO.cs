using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SvendeApi.DTO;

public class AuthResponseDTO
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public UserDTO User { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}

public class RefreshRequestDTO
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}

public class LogoutRequestDTO
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}
