using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvendeApi.Models;

public class CommentModel
{
    public Guid CommentId { get; set; }
    public Guid PostId { get; set; }
    public PostModel? Post { get; set; }
    public Guid AuthorId { get; set; }
    public UserModel? Author { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public Guid ParentCommentId { get; set; }
    public CommentModel? ParentComment { get; set; }
}