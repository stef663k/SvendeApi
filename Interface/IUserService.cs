using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SvendeApi.DTO;

namespace SvendeApi.Interface;

public interface IUserService
{
    Task<UserDTO> GetByIdAsync(Guid id);
    Task<UserDTO> GetByEmailAsync(string email);
    Task<UserDTO> GetByUsernameAsync(string username);
    Task<UserDTO> GetByRolesByIdAsync(Guid id);
    Task<IEnumerable<UserDTO>> GetAllAsync();
    Task<IEnumerable<UserDTO>> GetByRoleAsync(string role);

    Task<UserDTO> CreateAsync(CreateUserDTO dto);
    Task<UserDTO> UpdateAsync(Guid id, UpdateUserDTO dto);
    Task<UserDTO> DeleteAsync(Guid id);

    Task AddRoleAsync(Guid userId, string roleName);
    Task AssignRoleAsync(Guid userId, IEnumerable<string> roleNames);
    Task RemoveRoleAsync(Guid userId, string roleName);

    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task ResetPasswordAsync(Guid userId, string newPassword);
    Task DeactivateAsync(Guid id);
    Task ReactivateAsync(Guid id);

    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
}