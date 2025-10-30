using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SvendeApi.Data;
using SvendeApi.DTO;
using SvendeApi.Hubs;
using SvendeApi.Interface;
using SvendeApi.Models;

namespace SvendeApi.Services;

public class CommentService : ICommentService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHubContext<FeedHub> _hubContext;

    public CommentService(AppDbContext context, IMapper mapper, IHubContext<FeedHub> hubContext)
    {
        _context = context;
        _mapper = mapper;
        _hubContext = hubContext;
    }

    public async Task<CommentDTO> CreateAsync(Guid userId, CreateCommentDTO createCommentDTO)
    {
        var postExists = await _context.Posts.AnyAsync(p => p.PostId == createCommentDTO.PostId);
        if (!postExists)
            throw new KeyNotFoundException("Post not found");

        if (createCommentDTO.ParentCommentId.HasValue)
        {
            var parent = await _context.Comments
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CommentId == createCommentDTO.ParentCommentId.Value && !c.IsDeleted);
            if (parent == null)
                throw new KeyNotFoundException("Parent comment not found");
            if (parent.PostId != createCommentDTO.PostId)
                throw new InvalidOperationException("Parent comment must belong to the same post");
        }

        var entity = _mapper.Map<CommentModel>(createCommentDTO);
        entity.AuthorId = userId;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = entity.CreatedAt;
        _context.Comments.Add(entity);
        await _context.SaveChangesAsync();
        var dto = _mapper.Map<CommentDTO>(entity);
        await _hubContext.Clients.All.SendAsync("CommentCreated", dto);
        return dto;
    }

    public async Task<bool> DeleteAsync(Guid commentId, Guid requestedUserId)
    {
        var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);
        if (comment == null)
            return false;
        if (comment.AuthorId != requestedUserId)
            return false;
        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("CommentDeleted", commentId);
        return true;
    }

    public async Task<IReadOnlyList<CommentDTO>> GetAllAsync(int skip, int take)
    {
        return await _context.Comments.Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ProjectTo<CommentDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<CommentDTO> GetAsync(Guid commentId)
    {
        var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);
        if (comment == null)
            throw new KeyNotFoundException("Comment not found");
        return _mapper.Map<CommentDTO>(comment);
    }

    public async Task<IReadOnlyList<CommentDTO>> GetForPostAsync(Guid postId, int skip, int take)
    {
        return await _context.Comments
        .AsNoTracking()
        .Where(c => !c.IsDeleted && c.PostId == postId)
        .OrderByDescending(c => c.CreatedAt)
        .Skip(skip)
        .Take(take)
        .ProjectTo<CommentDTO>(_mapper.ConfigurationProvider)
        .ToListAsync();
    }

    public async Task<IReadOnlyList<CommentDTO>> GetForUserAsync(Guid userId, int skip, int take)
    {
        return await _context.Comments.Where(c => c.AuthorId == userId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ProjectTo<CommentDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<CommentDTO> UpdateAsync(Guid commentId, UpdateCommentDTO updateCommentDTO, Guid requestedUserId)
    {
        var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);
        if (comment == null)
            throw new KeyNotFoundException("Comment not found");
        if (comment.AuthorId != requestedUserId)
            throw new UnauthorizedAccessException("You can only update your own comments");
        if (!string.IsNullOrWhiteSpace(updateCommentDTO.Content))
            comment.Content = updateCommentDTO.Content;
        comment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return _mapper.Map<CommentDTO>(comment);
    }
}