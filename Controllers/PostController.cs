using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SvendeApi.Data;
using SvendeApi.DTO;
using SvendeApi.Hubs;
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
    private readonly IHubContext<FeedHub> _hubContext;

    public PostController(AppDbContext context, IPostService postService, IMapper mapper, IHubContext<FeedHub> hubContext)
    {
        _context = context;
        _postService = postService;
        _mapper = mapper;
        _hubContext = hubContext;
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
    [HttpPost]
    public async Task<IActionResult> CreatedPostNoSolid([FromBody] dynamic data)
    {
        try
        {
            // Forretningslogik direkte i controller
            var userId = GetCurrentUserId();

            // Manuel mapping af data til PostModel
            var post = new PostModel
            {
                PostId = Guid.NewGuid(),
                AuthorId = userId,
                Content = data.content.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                ParentPostId = data.parentPostId?.ToString() != null ? Guid.Parse(data.parentPostId.ToString()) : null
            };

            if (string.IsNullOrEmpty(post.Content) || post.Content.Length > 1000)
                return BadRequest("Invalid content length");

            if (ContainsOnlyEmoji(post.Content))
                return BadRequest("Content cannot only contain emojis");

            // Direkte database operationer i controller
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // SignalR gentagelse i alle controllers
            await _hubContext.Clients.All.SendAsync("PostCreated", new
            {
                postId = post.PostId,
                content = post.Content,
            });

            return Ok(new
            {
                postId = post.PostId,
                content = post.Content,
                createdAt = post.CreatedAt,
                updatedAt = post.UpdatedAt,
                isDeleted = post.IsDeleted,
                parentPostId = post.ParentPostId,
            });

        }
        catch (Exception ex){
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
    private bool ContainsOnlyEmoji(string content)
    {
        var trimmedContent = content.Trim();
        if (string.IsNullOrEmpty(trimmedContent))
            return false;
        
        return trimmedContent.All(c =>
        {
            return char.IsWhiteSpace(c) ||
                   (c >= 0x1000 && (
                       (c >= 0x1F600 && c <= 0x1F64F) || // Emoticons
                       (c >= 0x1F300 && c <= 0x1F5FF) || // Symbols & Pictographs
                       (c >= 0x1F680 && c <= 0x1F6FF) || // Transport & Map Symbols
                       (c >= 0x1F700 && c <= 0x1F77F) || // Alchemical Symbols
                       (c >= 0x1F780 && c <= 0x1F7FF) || // Geometric Shapes Extended
                       (c >= 0x1F800 && c <= 0x1F8FF) || // Supplemental Arrows-C
                       (c >= 0x1F900 && c <= 0x1F9FF) || // Supplemental Symbols and Pictographs
                       (c >= 0x1FA00 && c <= 0x1FA6F) || // Chess Symbols
                       (c >= 0x1FA70 && c <= 0x1FAFF) || // Symbols and Pictographs Extended-A
                       (c >= 0x2600 && c <= 0x26FF) || // Miscellaneous Symbols
                       (c >= 0x2700 && c <= 0x27BF) || // Dingbats
                       (c >= 0xFE00 && c <= 0xFE0F) || // Variation Selectors
                       (c >= 0x1F000 && c <= 0x1F02F) || // Mahjong Tiles
                       (c >= 0x1F0A0 && c <= 0x1F0FF) || // Playing Cards
                       (c >= 0x1F100 && c <= 0x1F1FF) || // Regional Indicator Symbols
                       (c >= 0x1F200 && c <= 0x1F2FF)    // Supplemental Symbols and Pictographs
                   ));
        });
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
        var isAdmin = User.IsInRole("Admin");

        var post = await _postService.GetAsync(postId);
        if (post == null)
            return NotFound(new { message = "Post not found" });

        if (!isAdmin && post.AuthorId != userId)
            return Forbid();

        var deleted = await _postService.DeleteAsync(postId, userId);
        if (!deleted)
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
