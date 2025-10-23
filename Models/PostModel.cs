using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvendeApi.Models;

public class PostModel
{
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public UserModel Author { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public Guid ParentPostId { get; set; }
    public PostModel? ParentPost { get; set; }
}