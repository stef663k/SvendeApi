using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SvendeApi.Data;
using SvendeApi.Interface;

namespace SvendeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableCors("Default")]
[Authorize]
public class FollowController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IFollowService _followService;
    private readonly IMapper _mapper;
    public FollowController(AppDbContext context, IFollowService followService, IMapper mapper)
    {
        _context = context;
        _followService = followService;
        _mapper = mapper;
    }

    [HttpPost("userId")]
    public async Task<IActionResult> FollowUser(Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        var followed = await _followService.FollowAsync(currentUserId, userId);
        return Ok(followed);
    }

    [HttpDelete("userId")]
    public async Task<IActionResult> UnfollowUser(Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        var unfollowed = await _followService.UnfollowAsync(currentUserId, userId);
        return Ok(unfollowed);
    }

    [HttpGet("userId/status")]
    public async Task<IActionResult> IsFollowing(Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        var isFollowing = await _followService.IsFollowingAsync(currentUserId, userId);
        return Ok(isFollowing);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
            ?? User.FindFirst("sub")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId)) return userId;
        throw new UnauthorizedAccessException("Invalid user ID in token");
    }
}