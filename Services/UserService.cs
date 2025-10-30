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
using SvendeApi.Utilities;

namespace SvendeApi.Services;

public class UserService : IUserService
{
	private readonly AppDbContext _context;
	private readonly IMapper _mapper;

	public UserService(AppDbContext context, IMapper mapper)
	{
		_context = context;
		_mapper = mapper;
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

		if (!user.UserRoles.Any(ur => ur.RoleId == role.RoleId))
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

	public async Task<UserDTO> CreateAsync(CreateUserDTO dto)
	{
		var user = _mapper.Map<UserModel>(dto);
		if (user.UserId == Guid.Empty)
			user.UserId = Guid.NewGuid();

		user.Email = user.Email?.Trim()!;
		user.Password = PasswordHasher.HashPassword(user.Password);

		_context.Users.Add(user);
		await _context.SaveChangesAsync();
		return _mapper.Map<UserDTO>(user);
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

	public async Task<UserDTO> DeleteAsync(Guid id)
	{
		var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
		if (user == null)
			throw new KeyNotFoundException("User not found");
		_context.Users.Remove(user);
		await _context.SaveChangesAsync();
		return _mapper.Map<UserDTO>(user);
	}

	public async Task<bool> EmailExistsAsync(string email)
	{
		var emailAddress = email.Trim();
		if (string.IsNullOrWhiteSpace(emailAddress))
			return false;
		return await _context.Users.AnyAsync(u => u.Email == emailAddress);
	}

	public async Task<IEnumerable<UserDTO>> GetAllAsync()
	{
		var users = await _context.Users.AsNoTracking().ToListAsync();
		return _mapper.Map<IEnumerable<UserDTO>>(users);
	}

	public async Task<UserDTO> GetByEmailAsync(string email)
	{
		var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
		if (user == null)
			throw new Exception("User not found");
		return _mapper.Map<UserDTO>(user);
	}

	public async Task<UserDTO> GetByIdAsync(Guid id)
	{
		var user = await _context.Users.FindAsync(id);
		if (user == null)
			throw new KeyNotFoundException("User not found");
		return _mapper.Map<UserDTO>(user);
	}

	public async Task<IEnumerable<UserDTO>> GetByRoleAsync(string role)
	{
		var name = role.Trim();
		if (string.IsNullOrWhiteSpace(name))
			return Enumerable.Empty<UserDTO>();

		var users = await _context.Users
			.Include(u => u.UserRoles!)
			.ThenInclude(ur => ur.Role)
			.Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == name))
			.AsNoTracking()
			.ToListAsync();
		return _mapper.Map<IEnumerable<UserDTO>>(users);
	}

	public async Task<UserDTO> GetByRolesByIdAsync(Guid id)
	{
		var user = await _context.Users
			.Include(u => u.UserRoles!)
			.ThenInclude(ur => ur.Role)
			.FirstOrDefaultAsync(u => u.UserId == id);
		if (user == null)
			throw new Exception("User not found");
		return _mapper.Map<UserDTO>(user);
	}

	public async Task<UserDTO> GetByUsernameAsync(string username)
	{
		var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == username);
		if (user == null)
			throw new KeyNotFoundException("User not found");
		return _mapper.Map<UserDTO>(user);
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

	public async Task<UserDTO> UpdateAsync(Guid id, UpdateUserDTO dto)
	{
		var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
		if (user == null)
			throw new KeyNotFoundException("User not found");

		// Only update provided fields
		if (!string.IsNullOrWhiteSpace(dto.FirstName)) user.FirstName = dto.FirstName;
		if (!string.IsNullOrWhiteSpace(dto.LastName)) user.LastName = dto.LastName;
		if (!string.IsNullOrWhiteSpace(dto.Email)) user.Email = dto.Email.Trim();

		await _context.SaveChangesAsync();
		return _mapper.Map<UserDTO>(user);
	}

	public async Task<bool> UsernameExistsAsync(string username)
	{
		var emailAddress = username.Trim();
		if (string.IsNullOrWhiteSpace(emailAddress))
			return false;
		return await _context.Users.AnyAsync(u => u.Email == emailAddress);
	}
	public async Task SetProcessingRestrictedAsync(Guid userId, bool restricted)
	{
		var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
		if (user == null)
			throw new KeyNotFoundException("User not found");
		user.ProcessingRestricted = restricted;
		await _context.SaveChangesAsync();
	}
	public async Task ForgetUserAsync(Guid userId)
	{
		var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
		if (user == null)
			throw new KeyNotFoundException("User not found");
		user.Password = null;
		await _context.SaveChangesAsync();
	}
}