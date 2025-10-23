using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvendeApi.Models;

public class LikeModel
{
    public Guid LikeId { get; set; }
    public Guid PostId { get; set; }
    public PostModel Post { get; set; } = null!;
    public Guid UserId { get; set; }
    public UserModel User { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}