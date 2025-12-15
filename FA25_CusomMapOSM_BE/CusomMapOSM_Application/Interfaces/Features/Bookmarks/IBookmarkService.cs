using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Bookmarks;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Bookmarks;

public interface IBookmarkService
{
    Task<Option<BookmarkDto, Error>> CreateBookmark(CreateBookmarkRequest request);
    Task<Option<BookmarkDto, Error>> GetBookmarkById(int bookmarkId);
    Task<Option<List<BookmarkDto>, Error>> GetMyBookmarks();
    Task<Option<List<BookmarkDto>, Error>> GetBookmarksByMapId(Guid mapId);
    Task<Option<BookmarkDto, Error>> UpdateBookmark(int bookmarkId, UpdateBookmarkRequest request);
    Task<Option<bool, Error>> DeleteBookmark(int bookmarkId);
}

