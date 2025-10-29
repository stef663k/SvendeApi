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

public class LikeService : ILikeService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHubContext<FeedHub> _hubContext;

    public LikeService(AppDbContext context, IMapper mapper, IHubContext<FeedHub> hubContext)
    {
        _context = context;
        _mapper = mapper;
        _hubContext = hubContext;
    }

    public async Task<LikeDTO> CreateAsync(Guid userId, CreateLikeDTO createLikeDTO)
    {
        var postExists = await _context.Posts.AnyAsync(p => p.PostId == createLikeDTO.PostId);
        if (!postExists)
            throw new KeyNotFoundException("Post not found");

        var exist = await _context.Likes.AnyAsync(l => l.UserId == userId && l.PostId == createLikeDTO.PostId);
        if (exist)
            throw new InvalidOperationException("You already liked this post");
        var entity = new LikeModel
        {
            LikeId = Guid.NewGuid(),
            UserId = userId,
            PostId = createLikeDTO.PostId,
            CreatedAt = DateTime.UtcNow
        };
        _context.Likes.Add(entity);
        await _context.SaveChangesAsync();
        return _mapper.Map<LikeDTO>(entity);
        
    }

    public async Task<bool> DeleteAsync(Guid likeId, Guid requestedUserId)
    {
        var like = await _context.Likes.FirstOrDefaultAsync(l => l.LikeId == likeId && l.UserId == requestedUserId);
        if (like == null)
            return false;
        _context.Likes.Remove(like);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid postId)
    {
        return await _context.Likes
        .AnyAsync(l => l.UserId == userId && l.PostId == postId);
    }

    public async Task<IReadOnlyList<LikeDTO>> GetAllAsync(int skip, int take)
    {
        return await _context.Likes
        .OrderByDescending(l => l.CreatedAt)
        .Skip(skip)
        .Take(take)
        .ProjectTo<LikeDTO>(_mapper.ConfigurationProvider)
        .ToListAsync();
    }

    public async Task<IReadOnlyList<LikeDTO>> GetForPostAsync(Guid postId, int skip, int take)
    {
        return await _context.Likes
        .Where(l => l.PostId == postId)
        .OrderByDescending(l => l.CreatedAt)
        .Skip(skip)
        .Take(take)
        .ProjectTo<LikeDTO>(_mapper.ConfigurationProvider)
        .ToListAsync();
    }

    public async Task<IReadOnlyList<LikeDTO>> GetForUserAsync(Guid userId, int skip, int take)
    {
        return await _context.Likes
        .Where(l => l.UserId == userId)
        .OrderByDescending(l => l.CreatedAt)
        .Skip(skip)
        .Take(take)
        .ProjectTo<LikeDTO>(_mapper.ConfigurationProvider)
        .ToListAsync();
    }
}