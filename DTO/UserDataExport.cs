using System;
using System.Collections.Generic;

namespace SvendeApi.DTO;

public class UserDataExport
{
    public UserDTO User { get; set; }
    public List<PostDTO> Posts { get; set; } = new();
    public List<CommentDTO> Comments { get; set; } = new();
    public List<LikeDTO> Likes { get; set; } = new();
}
