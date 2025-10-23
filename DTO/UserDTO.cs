using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvendeApi.DTO;

public class UserDTO
{
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class CreateUserDTO
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }
    [Required]
    [StringLength(50)]
    public string LastName { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    [MinLength(8)]
    public string Password { get; set; }
    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> Roles { get; set; } = new List<string>();
}

public class UpdateUserDTO
{
    [StringLength(50)]
    public string? FirstName { get; set; }
    [StringLength(50)]
    public string? LastName { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
}

public class LoginDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}

public class ChangePasswordDTO
{
    [Required]
    public string CurrentPassword { get; set; }
    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; }
    [Required]
    [Compare("NewPassword")]
    public string ConfirmNewPassword { get; set; }
}