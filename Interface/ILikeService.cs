using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SvendeApi.DTO;

namespace SvendeApi.Interface;

public interface ILikeService
{
    Task<CommentDTO> CreateAsync(Guid userId, CreateCommentDTO createCommentDTO);
    Task<IReadOnlyList<CommentDTO>> GetAlælAsync(int skip, int take);
    Task<IReadOnlyList<CommentDTO>> GetForPostAsync(Guid postId, int skip, int take);
    Task<bool> DeleteAsync(Guid likeId, Guid requestedUserId);
}