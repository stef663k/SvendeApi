using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SvendeApi.DTO;

public class LikeDTO
{
    public Guid LikeId { get; set; }
    public Guid PostId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateLikeDTO
{
    [Required]
    public Guid PostId { get; set; }
}