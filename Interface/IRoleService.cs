using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SvendeApi.DTO;

namespace SvendeApi.Interface;

public interface IRoleService
{
    Task<RoleDTO> GetByIdAsync(Guid id);
    Task<RoleDTO> GetByNameAsync(string roleName);
    Task<IEnumerable<RoleDTO>> GetAllAsync();
    Task<IEnumerable<RoleDTO>> GetAllOrderedAsync();
    Task<IEnumerable<string>> GetAllNameAsync();
    Task<IEnumerable<string>> GetSuggestedRoleNamesAsync(string roleName);

    Task<RoleDTO> CreateAsync(CreateRoleDTO dto);
    Task<RoleDTO> CreateIfNotExistsAsync(string roleName);
    Task<RoleDTO> UpdateAsync(Guid id, UpdateRoleDTO dto);
    Task<RoleDTO> DeleteAsync(Guid id);
    Task<bool> DeleteByNameAsync(string roleName);

    Task<bool> NameExistsAsync(string roleName);
}