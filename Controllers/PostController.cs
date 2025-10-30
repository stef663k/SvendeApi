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
public class PostController : Controller
{
    private readonly AppDbContext _context;
    private readonly IPostService _postService;
    private readonly IMapper _mapper;

    public PostController(AppDbContext context, IPostService postService, IMapper mapper)
    {
        _context = context;
        _postService = postService;
        _mapper = mapper;
    }

    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostDTO dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var post = await _postService.CreateAsync(userId, dto);
            return Ok(post);
        }
        catch (Exception ex)
        {
            return Problem(title: "Create Post Error", detail: ex.Message, statusCode: 500);
        }
    }

    [HttpGet("postId")]
    public async Task<IActionResult> GetPost(Guid postId)
    {
        var post = await _postService.GetAsync(postId);
        if (post == null)
            return NotFound(new { message = "Post not found" });
        return Ok(post);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPosts(int skip = 0, int take = 10)
    {
        var posts = await _postService.GetAllAsync(skip, take);
        return Ok(posts);
    }

    [HttpGet("feed")]
    public async Task<IActionResult> GetUserFeed(int skip = 0, int take = 10)
    {
        var userId = TryGetCurrentUserId();
        if (userId == null)
        {
            return Ok(Array.Empty<PostDTO>());
        }
        var posts = await _postService.GetUserFeedAsync(userId.Value, skip, take);
        return Ok(posts);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserPosts(Guid userId, int skip = 0, int take = 10)
    {
        var posts = await _postService.GetUserPostsAsync(userId, skip, take);
        return Ok(posts);
    }

    [HttpDelete("{postId}")]
    public async Task<IActionResult> DeletePost(Guid postId)
    {
        var userId = GetCurrentUserId();
        var deleted = await _postService.DeleteAsync(postId, userId);
        if (deleted)
            return NotFound(new { message = "Post not found" });
        return NoContent();
    }

    [HttpPut("{postId}/like")]
    public async Task<IActionResult> LikePost(Guid postId)
    {
        var userId = GetCurrentUserId();
        var liked = await _postService.LikeAsync(postId, userId);
        return Ok(liked);
    }

    [HttpPut("{postId}/unlike")]
    public async Task<IActionResult> UnlikePost(Guid postId)
    {
        var userId = GetCurrentUserId();
        var unliked = await _postService.UnlikeAsync(postId, userId);
        return Ok(unliked);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
            ?? User.FindFirst("sub")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        throw new UnauthorizedAccessException("Invalid user ID in token");
    }

    private Guid? TryGetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
            ?? User.FindFirst("sub")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId)) return userId;
        return null;
    }

}
