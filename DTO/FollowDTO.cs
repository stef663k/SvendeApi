using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SvendeApi.DTO;

public class FollowDTO
{
    public Guid FollowerUserId { get; set; }
    public Guid FolloweeUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateFollowDTO
{
    [Required]
    public Guid FolloweeUserId { get; set; }
}