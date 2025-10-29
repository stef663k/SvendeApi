using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SvendeApi.Data;
using SvendeApi.Hubs;
using SvendeApi.Interface;
using SvendeApi.Models;

namespace SvendeApi.Services;

public class FollowService : IFollowService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHubContext<FeedHub> _hubContext;

    public FollowService(AppDbContext context, IMapper mapper, IHubContext<FeedHub> hubContext)
    {
        _context = context;
        _mapper = mapper;
        _hubContext = hubContext;
    }

    public async Task<bool> FollowAsync(Guid followerUserId, Guid followeeUserId)
    {
        if (followerUserId == followeeUserId)
            throw new InvalidOperationException("You cannot follow yourself");
        var exist = await _context.Followers.AnyAsync(f => f.FollowerUserId == followerUserId && f.FolloweeUserId == followeeUserId);
        if (exist)
            throw new InvalidOperationException("You are already following this user");
        var entity = new FollowerModel
        {
            FollowerUserId = followerUserId,
            FolloweeUserId = followeeUserId,
            CreatedAt = DateTime.UtcNow
        };
        _context.Followers.Add(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsFollowingAsync(Guid followerUserId, Guid followeeUserId)
    {
        return await _context.Followers.AnyAsync(f => f.FollowerUserId == followerUserId && f.FolloweeUserId == followeeUserId);
    }

    public async Task<bool> UnfollowAsync(Guid followerUserId, Guid followeeUserId)
    {
        var exist = await _context.Followers.FirstOrDefaultAsync(f => f.FollowerUserId == followerUserId && f.FolloweeUserId == followeeUserId);
        if (exist == null)
            return true;
        _context.Followers.Remove(exist);
        await _context.SaveChangesAsync();
        return true;
    }
}