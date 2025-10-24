using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SvendeApi.DTO;

namespace SvendeApi.Interface;

public interface ICommentService
{
    Task<CommentDTO> CreateAsync(Guid userId, CreateCommentDTO createCommentDTO);
    Task<IReadOnlyList<CommentDTO>> GetAllAsync(int skip, int take);
    Task<IReadOnlyList<CommentDTO>> GetForPostAsync(Guid postId, int skip, int take);
    Task<bool> DeleteAsync(Guid commentId, Guid requestedUserId);
}