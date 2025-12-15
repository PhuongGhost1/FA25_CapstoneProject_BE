using CusomMapOSM_Domain.Entities.Comments;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Comments;

public interface ICommentRepository
{
    Task<bool> CreateComment(Comment comment);
    Task<Comment?> GetCommentById(int commentId);
    Task<List<Comment>> GetCommentsByMapId(Guid mapId);
    Task<List<Comment>> GetCommentsByLayerId(Guid layerId);
    Task<bool> UpdateComment(Comment comment);
    Task<bool> DeleteComment(int commentId);
    Task<bool> CheckCommentExists(int commentId);
    Task<bool> CheckCommentBelongsToUser(int commentId, Guid userId);
}

