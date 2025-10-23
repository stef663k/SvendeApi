using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SvendeApi.Models;

public class UserModel
{
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    [MinLength(8)]
    [Compare("Password")]
    public string Password { get; set; }
    public bool IsActive { get; set; }
    public Collection<UserRole>? UserRoles { get; set; }
    public Collection<PostModel>? Posts { get; set; }
    public Collection<CommentModel>? Comments { get; set; }
    public Collection<FollowerModel>? Followers { get; set; }
    public Collection<FollowerModel>? Followees { get; set; }
    public Collection<LikeModel>? Likes { get; set; }
}