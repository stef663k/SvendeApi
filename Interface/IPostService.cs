using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SvendeApi.DTO;
using SvendeApi.Models;

namespace SvendeApi.Interface;

public interface IPostService
{
    Task<PostDTO> CreateAsync(Guid userId, CreatePostDTO createPostDTO);
    Task<PostDTO> GetAsync(Guid postId);
    Task<PostDTO> UpdateAsync(Guid postId, UpdatePostDTO updatePostDTO, Guid requestedUserId);
    Task<IReadOnlyList<PostDTO>> GetAllAsync(int skip, int take);
    Task<IReadOnlyList<PostDTO>> GetUserPostsAsync(Guid userId, int skip, int take);
    Task<IReadOnlyList<PostDTO>> GetUserFeedAsync(Guid userId, int skip, int take);
    Task<bool> DeleteAsync(Guid postId, Guid requestedUserId);
    Task<bool> LikeAsync(Guid postId, Guid userId);
    Task<bool> UnlikeAsync(Guid postId, Guid userId);
}