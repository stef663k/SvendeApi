using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using SvendeApi.Data;
using SvendeApi.Interface;
using SvendeApi.Models;
using SvendeApi.DTO;

namespace SvendeApi.Services;

public class RoleService : IRoleService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public RoleService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<RoleDTO> CreateAsync(CreateRoleDTO dto)
    {
        var entity = _mapper.Map<RoleModel>(dto);
        if (entity.RoleId == Guid.Empty)
            entity.RoleId = Guid.NewGuid();
        var name = entity.RoleName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required", nameof(dto.RoleName));
        var exists = await _context.Roles.AnyAsync(r => r.RoleName == name);
        if (exists)
            throw new InvalidOperationException("Role name already exists");
        entity.RoleName = name;
        _context.Roles.Add(entity);
        await _context.SaveChangesAsync();
        return _mapper.Map<RoleDTO>(entity);
    }

    public async Task<RoleDTO> CreateIfNotExistsAsync(string roleName)
    {
        var name = roleName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required", nameof(roleName));
        var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == name);
        if (existingRole != null)
            return _mapper.Map<RoleDTO>(existingRole);
        var role = new RoleModel{RoleId = Guid.NewGuid(), RoleName = name};
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return _mapper.Map<RoleDTO>(role);
    }

    public async Task<RoleDTO> DeleteAsync(Guid id)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == id);
        if (role == null)
            throw new KeyNotFoundException("Role not found");
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
        return _mapper.Map<RoleDTO>(role);
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

    public async Task<IEnumerable<RoleDTO>> GetAllAsync()
    {
        var roles = await _context.Roles.AsNoTracking().ToListAsync();
        return _mapper.Map<IEnumerable<RoleDTO>>(roles);
    }

    public async Task<IEnumerable<string>> GetAllNameAsync()
    {
        return await _context.Roles.Select(r => r.RoleName).AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<RoleDTO>> GetAllOrderedAsync()
    {
        var roles = await _context.Roles.OrderBy(r => r.RoleName).AsNoTracking().ToListAsync();
        return _mapper.Map<IEnumerable<RoleDTO>>(roles);
    }

    public async Task<RoleDTO> GetByIdAsync(Guid id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
            throw new KeyNotFoundException("Role not found");
        return _mapper.Map<RoleDTO>(role);
    }

    public async Task<RoleDTO> GetByNameAsync(string roleName)
    {
        var name = roleName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required", nameof(roleName));
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == name);
        if (role == null)
            throw new KeyNotFoundException("Role not found");
        return _mapper.Map<RoleDTO>(role);
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

    public async Task<RoleDTO> UpdateAsync(Guid id, UpdateRoleDTO dto)
    {
        var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == id);
        if (existingRole == null)
            throw new KeyNotFoundException("Role not found");
        var name = dto.RoleName?.Trim();
        if (!string.IsNullOrWhiteSpace(name) && !string.Equals(existingRole.RoleName, name, StringComparison.Ordinal))
        {
            var exists = await _context.Roles.AnyAsync(r => r.RoleName == name);
            if (exists)
                throw new InvalidOperationException("Role name already exists");
            existingRole.RoleName = name;
        }
        await _context.SaveChangesAsync();
        return _mapper.Map<RoleDTO>(existingRole);
    }
}