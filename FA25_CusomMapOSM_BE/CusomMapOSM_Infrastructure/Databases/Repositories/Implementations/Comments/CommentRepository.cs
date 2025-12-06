using CusomMapOSM_Domain.Entities.Comments;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Comments;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Comments;

public class CommentRepository : ICommentRepository
{
    private readonly CustomMapOSMDbContext _context;

    public CommentRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateComment(Comment comment)
    {
        _context.Comments.Add(comment);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Comment?> GetCommentById(int commentId)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Map)
            .Include(c => c.Layer)
            .FirstOrDefaultAsync(c => c.CommentId == commentId);
    }

    public async Task<List<Comment>> GetCommentsByMapId(Guid mapId)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Where(c => c.MapId == mapId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Comment>> GetCommentsByLayerId(Guid layerId)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Where(c => c.LayerId == layerId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> UpdateComment(Comment comment)
    {
        comment.UpdatedAt = DateTime.UtcNow;
        _context.Comments.Update(comment);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteComment(int commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
            return false;

        _context.Comments.Remove(comment);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> CheckCommentExists(int commentId)
    {
        return await _context.Comments.AnyAsync(c => c.CommentId == commentId);
    }

    public async Task<bool> CheckCommentBelongsToUser(int commentId, Guid userId)
    {
        return await _context.Comments.AnyAsync(c => c.CommentId == commentId && c.UserId == userId);
    }
}

