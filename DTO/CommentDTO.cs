using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SvendeApi.DTO;

public class CommentDTO
{
    public Guid CommentId { get; set; }
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? ParentCommentId { get; set; }
    public CommentDTO? ParentComment { get; set; }
}

public class CreateCommentDTO
{
    [Required]
    public Guid PostId { get; set; }
    public Guid? ParentCommentId { get; set; }
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; }
}

public class UpdateCommentDTO
{
    [MaxLength(1000)]
    public string? Content { get; set; }
}