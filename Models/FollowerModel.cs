using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvendeApi.Models;

public class FollowerModel
{
    public Guid FollowerUserId { get; set; }
    public UserModel FollowerUser { get; set; } = null!;
    public Guid FolloweeUserId { get; set; }
    public UserModel FolloweeUser { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}