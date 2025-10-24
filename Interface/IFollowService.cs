using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvendeApi.Interface;

public interface IFollowService
{
    Task<bool> FollowAsync(Guid followerUserId, Guid followeeUserId);
    Task<bool> UnfollowAsync(Guid followerUserId, Guid followeeUserId);
    Task<bool> IsFollowingAsync(Guid followerUserId, Guid followeeUserId);
}