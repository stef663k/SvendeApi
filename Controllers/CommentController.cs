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
public class CommentController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ICommentService _commentService;
    private readonly IMapper _mapper;
    public CommentController(AppDbContext context, ICommentService commentService, IMapper mapper)
    {
        _context = context;
        _commentService = commentService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetComments(Guid? postId, int skip = 0, int take = 10)
    {
        if (postId == null)
        {
            var allComments = await _commentService.GetAllAsync(skip, take);
            return Ok(allComments);
        }
        else
        {
            var comments = await _commentService.GetForPostAsync(postId.Value, skip, take);
            return Ok(comments);
        }

    }

    [HttpPost("{commentId}")]
    [Consumes("application/json")]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentDTO dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var comment = await _commentService.CreateAsync(userId, dto);
            return Ok(comment);
        }
        catch (Exception ex)
        {
            return Problem(title: "Create Comment Error", detail: ex.Message, statusCode: 500);
        }
    }

    [HttpDelete("{commentId}")]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        var userId = GetCurrentUserId();
        var deleted = await _commentService.DeleteAsync(commentId, userId);
        if (!deleted)
            return NotFound(new { message = "Comment not found" });
        return NoContent();
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
