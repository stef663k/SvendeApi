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

public class PostService : IPostService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHubContext<FeedHub> _hubContext;

    public PostService(AppDbContext context, IMapper mapper, IHubContext<FeedHub> hubContext)
    {
        _context = context;
        _mapper = mapper;
        _hubContext = hubContext;
    }

    public async Task<PostDTO> CreateAsync(Guid userId, CreatePostDTO createPostDTO)
    {
        var entity = _mapper.Map<PostModel>(createPostDTO);
        entity.AuthorId = userId;
        entity.CreatedAt = DateTime.UtcNow;

        _context.Posts.Add(entity);
        await _context.SaveChangesAsync();
        var dto = _mapper.Map<PostDTO>(entity);
        await _hubContext.Clients.All.SendAsync("PostCreated", dto);
        return dto;
    }

    public async Task<bool> DeleteAsync(Guid postId, Guid requestedUserId)
    {
        var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
        if (post == null)
            return false;
        if (post.AuthorId != requestedUserId)
            return false;

        post.IsDeleted = true;
        await _context.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("PostDeleted", postId);
        return true;
    }

    public async Task<IReadOnlyList<PostDTO>> GetAllAsync(int skip, int take)
    {
        return await _context.Posts.Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ProjectTo<PostDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<PostDTO> GetAsync(Guid postId)
    {
        var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
        return post == null ? null : _mapper.Map<PostDTO>(post);
    }

    public async Task<IReadOnlyList<PostDTO>> GetUserFeedAsync(Guid userId, int skip, int take)
    {
        var followeeIds = await _context.Followers.Where(f => f.FollowerUserId == userId).Select(f => f.FolloweeUserId).ToListAsync();

        return await _context.Posts.AsNoTracking()
        .Where(p => !p.IsDeleted && followeeIds
        .Contains(p.AuthorId))
        .OrderByDescending(p => p.CreatedAt)
        .Skip(skip).Take(take)
        .ProjectTo<PostDTO>(_mapper.ConfigurationProvider)
        .ToListAsync();
    }

    public async Task<IReadOnlyList<PostDTO>> GetUserPostsAsync(Guid userId, int skip, int take)
    {
        return await _context.Posts.AsNoTracking()
            .Where(p => !p.IsDeleted && p.AuthorId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip).Take(take)
            .ProjectTo<PostDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<bool> LikeAsync(Guid postId, Guid userId)
    {
        var exist = await _context.Likes.AnyAsync(l => l.PostId == postId && l.UserId == userId);
        if (exist)
            return true;
        var like = new LikeModel
        {
            LikeId = Guid.NewGuid(),
            PostId = postId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        _context.Likes.Add(like);
        await _context.SaveChangesAsync();
        await _hubContext.Clients.Group($"post:{postId}").SendAsync("PostLiked", new { postId, userId });
        return true;
    }

    public async Task<bool> UnlikeAsync(Guid postId, Guid userId)
    {
        var like = await _context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
        if (like == null)
            return true;
        _context.Likes.Remove(like);
        await _context.SaveChangesAsync();
        await _hubContext.Clients.Group($"post:{postId}").SendAsync("PostUnliked", new { postId, userId });
        return true;
    }

    public async Task<PostDTO> UpdateAsync(Guid postId, UpdatePostDTO updatePostDTO, Guid requestedUserId)
    {
        var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
        if (post == null)
            throw new KeyNotFoundException("Post not found");
        
        if (post.AuthorId != requestedUserId)
            throw new UnauthorizedAccessException("You can only update your own posts");

        if (!string.IsNullOrWhiteSpace(updatePostDTO.Content))
        {
            post.Content = updatePostDTO.Content;
        }

        post.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        var dto = _mapper.Map<PostDTO>(post);
        await _hubContext.Clients.All.SendAsync("PostUpdated", dto);
        return dto;
    }
}