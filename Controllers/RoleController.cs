using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SvendeApi.Data;
using SvendeApi.DTO;
using SvendeApi.Interface;
using SvendeApi.Models;

namespace SvendeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableCors("Default")]
[Authorize(Roles = "Admin")]
public class RoleController : Controller
{
    private readonly AppDbContext _context;
    private readonly IRoleService _roleService;
    private readonly IMapper _mapper;
    public RoleController(AppDbContext context, IRoleService roleService, IMapper mapper)
    {
        _context = context;
        _roleService = roleService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            var roles = await _roleService.GetAllAsync();
            var rolesDto = _mapper.Map<IEnumerable<RoleDTO>>(roles);
            return Ok(rolesDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "An error occurred while fetching roles", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRole(Guid id)
    {
        try
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }
            var roleDto = _mapper.Map<RoleDTO>(role);
            return Ok(roleDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "An error occurred while fetching role", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole(CreateRoleDTO dto)
    {
        try
        {
            if (await _roleService.NameExistsAsync(dto.RoleName))
            {
                return BadRequest(new { message = "Role name already exists" });
            }

            var role = _mapper.Map<RoleModel>(dto);
            var createdRole = await _roleService.CreateAsync(dto);
            var roleDto = _mapper.Map<RoleDTO>(createdRole);

            return CreatedAtAction(nameof(GetRole), new { id = roleDto.RoleId }, roleDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "An error occurred while creating role", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(Guid id, UpdateRoleDTO dto)
    {
        try
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }
            if (await _roleService.NameExistsAsync(dto.RoleName) &&
            !string.Equals(role.RoleName, dto.RoleName, StringComparison.Ordinal))
            {
                return BadRequest(new { message = "Role name already exists" });
            }
            _mapper.Map(dto, role);
            role.RoleName = dto.RoleName.Trim();

            var updatedRole = await _roleService.UpdateAsync(id, dto);
            var roleDto = _mapper.Map<RoleDTO>(updatedRole);
            return CreatedAtAction(nameof(GetRole), new { id = roleDto.RoleId }, roleDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "An error occurred while updating role", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        try
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }
            var deletedRole = await _roleService.DeleteAsync(id);

            if (deletedRole == null)
            {
                return BadRequest(new { message = "Role is in use and cannot be deleted" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "An error occurred while deleting role", error = ex.Message });
        }
    }

    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestedRoleNames(string roleName)
    {
        try
        {
            var suggestion = new List<string> { "Admin", "User", "Manager" };
            return Ok(suggestion);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "An error occurred while fetching suggested role names", error = ex.Message });
        }
    }

}
