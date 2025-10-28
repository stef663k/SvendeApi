using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SvendeApi.Data;
using SvendeApi.Interface;
using SvendeApi.Models;

namespace SvendeApi.Services;

public class RoleService : IRoleService
{
    private readonly AppDbContext _context;

    public RoleService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RoleModel> CreateAsync(RoleModel entity)
    {
        if (entity.RoleId == Guid.Empty)
            entity.RoleId = Guid.NewGuid();

        var name = entity.RoleName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required", nameof(entity.RoleName));

        var exists = await _context.Roles.AnyAsync(r => r.RoleName == name);
        if (exists)
            throw new InvalidOperationException("Role name already exists");

        entity.RoleName = name;
        _context.Roles.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<RoleModel> CreateIfNotExistsAsync(string roleName)
    {
        var name = roleName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required", nameof(roleName));

        var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == name);
        if (existingRole != null)
            return existingRole;

        var role = new RoleModel{RoleId = Guid.NewGuid(), RoleName = name};
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<RoleModel> DeleteAsync(RoleModel entity)
    {
        _context.Roles.Remove(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteByNameAsync(string roleName)
    {
        var name = roleName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == name);
        if (role == null)
            return false;

        var inUse = await _context.UserRoles.AnyAsync(ur => ur.RoleId == role.RoleId);
        if (inUse)
            return false;

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<RoleModel>> GetAllAsync()
    {
        return await _context.Roles.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<string>> GetAllNameAsync()
    {
        return await _context.Roles.Select(r => r.RoleName).AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<RoleModel>> GetAllOrderedAsync()
    {
        return await _context.Roles.OrderBy(r => r.RoleName).AsNoTracking().ToListAsync();
    }

    public async Task<RoleModel?> GetByIdAsync(Guid id)
    {
        return await _context.Roles.FindAsync(id);
    }

    public async Task<RoleModel> GetByNameAsync(string roleName)
    {
        var name = roleName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required", nameof(roleName));

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == name);
        if (role == null)
            throw new KeyNotFoundException("Role not found");
        return role;
    }

    public async Task<IEnumerable<string>> GetSuggestedRoleNamesAsync(string roleName)
    {
        var name = roleName?.Trim() ?? string.Empty;
        var query = _context.Roles.AsNoTracking().Select(r => r.RoleName);

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(n => EF.Functions.Like(n, name + "%"));
        }

        return await query.OrderBy(n => n).ToListAsync();
    }

    public async Task<bool> NameExistsAsync(string roleName)
    {
        var name = roleName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return false;
        return await _context.Roles.AnyAsync(r => r.RoleName == name);
    }

    public async Task<RoleModel> UpdateAsync(RoleModel entity)
    {
        var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == entity.RoleId);
        if (existingRole == null)
            throw new KeyNotFoundException("Role not found");

        var name = entity.RoleName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required", nameof(entity.RoleName));

        if (!string.Equals(existingRole.RoleName, name, StringComparison.Ordinal))
        {
            var exists = await _context.Roles.AnyAsync(r => r.RoleName == name);
            if (exists)
                throw new InvalidOperationException("Role name already exists");
        }

        existingRole.RoleName = name;
        await _context.SaveChangesAsync();
        return existingRole;
    }
}