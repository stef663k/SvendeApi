using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SvendeApi.Models;

namespace SvendeApi.Interface;

public interface IUserService : IGenericInterface<UserModel>
{
    Task<UserModel> GetByEmailAsync(string email);
    Task<UserModel> GetByUsernameAsync(string username);
    Task<UserModel> GetByRolesByIdAsync(Guid id);
    Task<IEnumerable<UserModel>> GetByRoleAsync(string role);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
    Task AssignRoleAsync(Guid userId, IEnumerable<string> roleNames);
    Task AddRoleAsync(Guid userId, string roleName);
    Task RemoveRoleAsync(Guid userId, string roleName);
    Task DeactivateAsync(Guid id);
    Task ReactivateAsync(Guid id);
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task ResetPasswordAsync(Guid userId, string newPassword);

}