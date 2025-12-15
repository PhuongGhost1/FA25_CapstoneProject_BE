using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Comments;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Comments;

public interface ICommentService
{
    Task<Option<CommentDto, Error>> CreateComment(CreateCommentRequest request);
    Task<Option<CommentDto, Error>> GetCommentById(int commentId);
    Task<Option<List<CommentDto>, Error>> GetCommentsByMapId(Guid mapId);
    Task<Option<List<CommentDto>, Error>> GetCommentsByLayerId(Guid layerId);
    Task<Option<CommentDto, Error>> UpdateComment(int commentId, UpdateCommentRequest request);
    Task<Option<bool, Error>> DeleteComment(int commentId);
}

