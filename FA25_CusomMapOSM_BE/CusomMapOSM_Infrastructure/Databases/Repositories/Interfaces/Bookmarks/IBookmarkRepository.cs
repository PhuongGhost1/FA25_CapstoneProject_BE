using CusomMapOSM_Domain.Entities.Bookmarks;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Bookmarks;

public interface IBookmarkRepository
{
    Task<bool> CreateBookmark(Bookmark bookmark);
    Task<Bookmark?> GetBookmarkById(int bookmarkId);
    Task<List<Bookmark>> GetBookmarksByUserId(Guid userId);
    Task<List<Bookmark>> GetBookmarksByMapId(Guid mapId);
    Task<Bookmark?> GetBookmarkByUserAndMap(Guid userId, Guid mapId);
    Task<bool> UpdateBookmark(Bookmark bookmark);
    Task<bool> DeleteBookmark(int bookmarkId);
    Task<bool> CheckBookmarkExists(int bookmarkId);
    Task<bool> CheckBookmarkBelongsToUser(int bookmarkId, Guid userId);
}

