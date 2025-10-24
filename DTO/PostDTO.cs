using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SvendeApi.DTO;

public class PostDTO
{
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public Guid ParentPostId { get; set; }
    public PostDTO? ParentPost { get; set; }
}

public class CreatePostDTO
{
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; }
    public Guid? ParentPostId { get; set; }
}

public class UpdatePostDTO
{
    [MaxLength(1000)]
    public string? Content { get; set; }
}