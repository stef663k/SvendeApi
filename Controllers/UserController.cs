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
[Authorize]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly IMapper _mapper;
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;
    private readonly ILikeService _likeService;
    public UserController(AppDbContext context, IUserService userService, IRoleService roleService, IMapper mapper, IPostService postService, ICommentService commentService, ILikeService likeService)
    {
        _context = context;
        _userService = userService;
        _roleService = roleService;
        _mapper = mapper;
        _postService = postService;
        _commentService = commentService;
        _likeService = likeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllAsync();
            var usersDto = _mapper.Map<IEnumerable<UserDTO>>(users);
            return Ok(usersDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "An error occured while fetching users", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (id != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid("You can only view your own profile unless you're an admin");
            }

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var userDto = _mapper.Map<UserDTO>(user);
            return Ok(userDto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "An error occurred while fetching user", error = ex.Message });
        }
    }

    [HttpGet("{id}/public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicUser(Guid id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }
            var userDto = _mapper.Map<UserDTO>(user);
            return Ok(userDto);
        }
        catch(Exception ex)
        {
            return BadRequest(new { message = "An error occurred while fetching user", error = ex.Message });
        }
    }
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser(CreateUserDTO dto)
    {
        try
        {
            if (await _userService.EmailExistsAsync(dto.Email))
            {
                return BadRequest(new { message = "Email already exists" });
            }

            var user = _mapper.Map<UserModel>(dto);
            var createdUser = await _userService.CreateAsync(dto);

            if (dto.Roles?.Any() == true)
            {
                await _userService.AssignRoleAsync(createdUser.UserId, dto.Roles);
                createdUser = await _userService.GetByIdAsync(createdUser.UserId);
            }

            var userDto = _mapper.Map<UserDTO>(createdUser);
            return CreatedAtAction(nameof(GetUser), new { id = userDto.UserId }, userDto);
        }
        catch(Exception ex)
        {
            return BadRequest(new { message = "An error occurred while creating user", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserDTO dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (id != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid("You can only update your own profile unless you're an admin");
            }

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }
            _mapper.Map(dto, user);

            var updatedUser = await _userService.UpdateAsync(id, _mapper.Map<UpdateUserDTO>(dto));
            if (updatedUser != null && User.IsInRole("Admin"))
            {
                await _userService.AssignRoleAsync(updatedUser.UserId, dto.Roles);
                updatedUser = await _userService.GetByIdAsync(updatedUser.UserId);
            }
            var userDto = _mapper.Map<UserDTO>(updatedUser);
            return Ok(userDto);
        }
        catch(KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
        catch(Exception ex)
        {
            return BadRequest(new { message = "An error occurred while updating user", error = ex.Message });
        }
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser([FromBody] Guid id)
    {
        try
        {
            var currentUserId = await _userService.GetByIdAsync(id);
            if (currentUserId == null)
            {
                return NotFound(new { message = "User not found" });
            }
            await _userService.DeleteAsync(id);
            return NoContent();
        }
        catch(Exception ex)
        {
            return BadRequest(new { message = "An error occurred while deleting user", error = ex.Message });
        }
    }

    [HttpPost("{id}/roles")]
    public async Task<IActionResult> AssignRoles(Guid id, List<string> roles)
    {
        try
        {
            await _userService.AssignRoleAsync(id, roles);
            return NoContent();
        }
        catch(KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
        catch(Exception ex)
        {
            return BadRequest(new { message = "An error occurred while assigning roles", error = ex.Message });
        }
    }

    [HttpPost("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(Guid id, ChangePasswordDTO dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (id != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid("You can only change your own password unless you're an admin");
            }
            await _userService.ChangePasswordAsync(id, dto.CurrentPassword, dto.NewPassword);
            return NoContent();
        }
        catch(KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("You are not authorized to change this password");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "An error occurred while changing password", error = ex.Message });
        }
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        try
        {
            await _userService.DeactivateAsync(id);
            return NoContent();
        }
        catch(KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
        catch(Exception ex)
        {
            return BadRequest(new { message = "An error occurred while deactivating user", error = ex.Message });
        }
    }

    [HttpPost("{id}/reactivate")]
    public async Task<IActionResult> ReactivateUser(Guid id)
    {
        try
        {
            await _userService.ReactivateAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "An error occurred while reactivating user", error = ex.Message });
        }
    }

    [HttpGet("{id}/data-export")]
    public async Task<IActionResult> ExportUserData(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (id != currentUserId && !User.IsInRole("Admin"))
                return Forbid("You can only access your own data unless you're an admin");

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var posts = await _postService.GetUserPostsAsync(id, 0, int.MaxValue);
            var comments = await _commentService.GetForUserAsync(id, 0, int.MaxValue);
            var likes = await _likeService.GetForUserAsync(id, 0, int.MaxValue);

            var export = new UserDataExport
            {
                User = _mapper.Map<UserDTO>(user),
                Posts = posts.ToList(),
                Comments = comments.ToList(),
                Likes = likes.ToList()
            };
            return Ok(export);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "An error occurred during data export", error = ex.Message });
        }
    }

    [HttpPost("{id}/restrict-processing")]
    public async Task<IActionResult> RestrictProcessing(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (id != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid("You can only restrict processing on your own account unless you're an admin");
            }
            await _userService.SetProcessingRestrictedAsync(id, true);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "An error occurred while restricting processing", error = ex.Message });
        }
    }

    [HttpPost("{id}/unrestrict-processing")]
    public async Task<IActionResult> UnrestrictProcessing(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (id != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid("You can only unrestrict processing on your own account unless you're an admin");
            }
            await _userService.SetProcessingRestrictedAsync(id, false);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "An error occurred while unrestricting processing", error = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User ID not found in token");
    }
}
