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
public class LikeController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILikeService _likeService;
    private readonly IMapper _mapper;
    public LikeController(AppDbContext context, ILikeService likeService, IMapper mapper)
    {
        _context = context;
        _likeService = likeService;
        _mapper = mapper;
    }

    [HttpPost("postId")]
    public async Task<IActionResult> LikePost(Guid postId)
    {
        var userId = GetCurrentUserId();
        var like = await _likeService.CreateAsync(userId, new CreateLikeDTO { PostId = postId });
        return Ok(like);
    }

    [HttpDelete("likeId")]
    public async Task<IActionResult> UnlikePost(Guid likeId)
    {
        var userId = GetCurrentUserId();
        var unliked = await _likeService.DeleteAsync(likeId, userId);
        return Ok(unliked);
    }

    [HttpGet("{postId}/status")]
    public async Task<IActionResult> GetLikeStatus(Guid postId)
    {
        var userId = GetCurrentUserId();
        var liked = await _likeService.ExistsAsync(userId, postId);
        return Ok(liked);
    }

    [HttpGet("postId /count")]
    public async Task<IActionResult> GetLikeCount(Guid postId)
    {
        var count = await _likeService.GetLikeCountAsync(postId);
        return Ok(count);
    }

    [HttpGet("user/list")]
    public async Task<IActionResult> GetUserLikes(Guid postId, int skip = 0, int take = 10)
    {
        var likes = await _likeService.GetForPostAsync(postId, skip, take);
        return Ok(likes);
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
