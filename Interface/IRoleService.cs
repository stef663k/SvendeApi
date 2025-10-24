using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SvendeApi.Models;

namespace SvendeApi.Interface;

public interface IRoleService : IGenericInterface<RoleModel>
{
    Task<RoleModel> GetByNameAsync(string roleName);
    Task<bool> NameExistsAsync(string roleName);
    Task<IEnumerable<RoleModel>> GetAllOrderedAsync();
    Task<IEnumerable<string>> GetAllNameAsync();
    Task<RoleModel> CreateIfNotExistsAsync(string roleName);
    Task<bool> DeleteByNameAsync(string roleName);
    Task<IEnumerable<string>> GetSuggestedRoleNamesAsync(string roleName);
}