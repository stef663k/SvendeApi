using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SvendeApi.DTO;

namespace SvendeApi.Interface;

public interface ILikeService
{
    Task<LikeDTO> CreateAsync(Guid userId, CreateLikeDTO createLikeDTO);
    Task<IReadOnlyList<LikeDTO>> GetAllAsync(int skip, int take);
    Task<IReadOnlyList<LikeDTO>> GetForPostAsync(Guid postId, int skip, int take);
    Task<IReadOnlyList<LikeDTO>> GetForUserAsync(Guid userId, int skip, int take);
    Task<bool> DeleteAsync(Guid likeId, Guid requestedUserId);
    Task<bool> ExistsAsync(Guid userId, Guid postId);
    Task<int> GetLikeCountAsync(Guid postId);
}