using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SvendeApi.Data;
using SvendeApi.Interface;
using SvendeApi.Models;
using SvendeApi.Utilities;

namespace SvendeApi.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddRoleAsync(Guid userId, string roleName)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles!)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            throw new Exception("User not found");

        var name = roleName?.Trim();
        if (string.IsNullOrEmpty(name))
            return;

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == name);
        if (role == null)
            throw new Exception("Role not found");
 
        if (!user.UserRoles.Any(ur=> ur.RoleId == role.RoleId))
        {
            user.UserRoles.Add(new UserRole { RoleId = role.RoleId, UserId = userId });
            await _context.SaveChangesAsync();
        }
    }

    public async Task AssignRoleAsync(Guid userId, IEnumerable<string> roleNames)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles!)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
            throw new Exception("User not found");
 
        var names = (roleNames ?? Enumerable.Empty<string>())
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .Distinct();
 
        var roles = await _context.Roles.Where(r => names.Contains(r.RoleName))
            .ToListAsync();
            
        user.UserRoles.Clear();
        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole { RoleId = role.RoleId, UserId = userId });
        }
        await _context.SaveChangesAsync();
    }
 
    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
            throw new Exception("User not found");
 
        if (!PasswordHasher.VerifyPassword(currentPassword, user.Password))
            throw new UnauthorizedAccessException("Invalid current password");
 
        user.Password = PasswordHasher.HashPassword(newPassword);
        await _context.SaveChangesAsync();
    }

    public async Task<UserModel> CreateAsync(UserModel entity)
    {
        if (entity.UserId == Guid.Empty)
            entity.UserId = Guid.NewGuid();
 
        if (!string.IsNullOrWhiteSpace(entity.Email))
            entity.Email = entity.Email.Trim();
 
        if (!string.IsNullOrWhiteSpace(entity.Password))
        {
            if (!PasswordHasher.IsWellFormedHash(entity.Password))
                entity.Password = PasswordHasher.HashPassword(entity.Password);
        }
 
        _context.Users.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
 
    }

    public async Task DeactivateAsync(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null)
            throw new Exception("User not found");
        if (!user.IsActive)
            return;
        user.IsActive = false;
        await _context.SaveChangesAsync();
    }

    public async Task<UserModel> DeleteAsync(UserModel entity)
    {
        _context.Users.Remove(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var emailAddress = email.Trim();
        if (string.IsNullOrWhiteSpace(emailAddress))
            return false;
        return await _context.Users.AnyAsync(u => u.Email == emailAddress);
    }

    public async Task<IEnumerable<UserModel>> GetAllAsync()
    {
        return await _context.Users.AsNoTracking().ToListAsync() ?? Enumerable.Empty<UserModel>();
    }

    public async Task<UserModel> GetByEmailAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            throw new Exception("User not found");
        return user;
    }

    public async Task<UserModel?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
 
    }

    public async Task<IEnumerable<UserModel>> GetByRoleAsync(string role)
    {
        var name = role.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return Enumerable.Empty<UserModel>();
 
        return await _context.Users
            .Include(u => u.UserRoles!)
            .ThenInclude(ur => ur.Role)
            .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == name))
            .AsNoTracking()
            .ToListAsync() ?? Enumerable.Empty<UserModel>();
    }

    public async Task<UserModel> GetByRolesByIdAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles!)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null)
            throw new Exception("User not found");
        return user;
    }

    public async Task<UserModel> GetByUsernameAsync(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == username);
        if (user == null)
            throw new KeyNotFoundException("User not found");
        return user;
    }

    public async Task ReactivateAsync(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null)
            throw new KeyNotFoundException("User not found");
        if (user.IsActive)
            return;
        user.IsActive = true;
        await _context.SaveChangesAsync();
    }

    public async Task RemoveRoleAsync(Guid userId, string roleName)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles!)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");
 
        var name = roleName.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return;
 
        var roleRemove = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == name);
        if (roleRemove != null)
        {
            var userRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == roleRemove.RoleId);
            if (userRole != null)
            {
                user.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task ResetPasswordAsync(Guid userId, string newPassword)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        user.Password = PasswordHasher.HashPassword(newPassword);
        await _context.SaveChangesAsync();
    }

    public async Task<UserModel> UpdateAsync(UserModel entity)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == entity.UserId);
        if (existingUser == null)
            throw new KeyNotFoundException("User not found");

        existingUser.FirstName = entity.FirstName;
        existingUser.LastName = entity.LastName;
        existingUser.Email = entity.Email;
        await _context.SaveChangesAsync();
        return existingUser;
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        var emailAddress = username.Trim();
        if (string.IsNullOrWhiteSpace(emailAddress))
            return false;
        return await _context.Users.AnyAsync(u => u.Email == emailAddress);
    }
}