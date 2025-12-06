using CusomMapOSM_Domain.Entities.Bookmarks;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Bookmarks;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Bookmarks;

public class BookmarkRepository : IBookmarkRepository
{
    private readonly CustomMapOSMDbContext _context;

    public BookmarkRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateBookmark(Bookmark bookmark)
    {
        _context.Bookmarks.Add(bookmark);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Bookmark?> GetBookmarkById(int bookmarkId)
    {
        return await _context.Bookmarks
            .Include(b => b.User)
            .Include(b => b.Map)
            .FirstOrDefaultAsync(b => b.BookmarkId == bookmarkId);
    }

    public async Task<List<Bookmark>> GetBookmarksByUserId(Guid userId)
    {
        return await _context.Bookmarks
            .Include(b => b.Map)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Bookmark>> GetBookmarksByMapId(Guid mapId)
    {
        return await _context.Bookmarks
            .Include(b => b.User)
            .Where(b => b.MapId == mapId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<Bookmark?> GetBookmarkByUserAndMap(Guid userId, Guid mapId)
    {
        return await _context.Bookmarks
            .Include(b => b.Map)
            .FirstOrDefaultAsync(b => b.UserId == userId && b.MapId == mapId);
    }

    public async Task<bool> UpdateBookmark(Bookmark bookmark)
    {
        _context.Bookmarks.Update(bookmark);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteBookmark(int bookmarkId)
    {
        var bookmark = await _context.Bookmarks.FindAsync(bookmarkId);
        if (bookmark == null)
            return false;

        _context.Bookmarks.Remove(bookmark);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> CheckBookmarkExists(int bookmarkId)
    {
        return await _context.Bookmarks.AnyAsync(b => b.BookmarkId == bookmarkId);
    }

    public async Task<bool> CheckBookmarkBelongsToUser(int bookmarkId, Guid userId)
    {
        return await _context.Bookmarks.AnyAsync(b => b.BookmarkId == bookmarkId && b.UserId == userId);
    }
}

