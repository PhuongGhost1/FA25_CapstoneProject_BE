using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Bookmarks;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Bookmarks;
using CusomMapOSM_Domain.Entities.Bookmarks;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Bookmarks;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using Optional;
using Optional.Unsafe;

namespace CusomMapOSM_Infrastructure.Features.Bookmarks;

public class BookmarkService : IBookmarkService
{
    private readonly IBookmarkRepository _bookmarkRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapRepository _mapRepository;

    public BookmarkService(IBookmarkRepository bookmarkRepository, ICurrentUserService currentUserService, IMapRepository mapRepository)
    {
        _bookmarkRepository = bookmarkRepository;
        _currentUserService = currentUserService;
        _mapRepository = mapRepository;
    }

    public async Task<Option<BookmarkDto, Error>> CreateBookmark(CreateBookmarkRequest request)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserId();
            if (!currentUserId.HasValue)
            {
                return Option.None<BookmarkDto, Error>(Error.Unauthorized("User.NotAuthenticated", "User must be authenticated"));
            }

            // Validate that the map exists
            var map = await _mapRepository.GetMapById(request.MapId);
            if (map == null || !map.IsActive)
            {
                return Option.None<BookmarkDto, Error>(Error.NotFound("Map.NotFound", "Map not found or is not active"));
            }

            // Check if bookmark already exists for this user and map
            var existingBookmark = await _bookmarkRepository.GetBookmarkByUserAndMap(currentUserId.Value, request.MapId);
            if (existingBookmark != null)
            {
                return Option.None<BookmarkDto, Error>(Error.Conflict("Bookmark.AlreadyExists", "Bookmark already exists for this map"));
            }

            var bookmark = new Bookmark
            {
                MapId = request.MapId,
                UserId = currentUserId.Value,
                Name = request.Name ?? string.Empty,
                ViewState = request.ViewState ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _bookmarkRepository.CreateBookmark(bookmark);
            if (!created)
            {
                return Option.None<BookmarkDto, Error>(Error.Failure("Bookmark.CreateFailed", "Failed to create bookmark"));
            }

            var createdBookmark = await _bookmarkRepository.GetBookmarkById(bookmark.BookmarkId);
            if (createdBookmark == null)
            {
                return Option.None<BookmarkDto, Error>(Error.Failure("Bookmark.NotFound", "Created bookmark not found"));
            }

            return Option.Some<BookmarkDto, Error>(MapToDto(createdBookmark));
        }
        catch (Exception ex)
        {
            return Option.None<BookmarkDto, Error>(Error.Failure("Bookmark.CreateFailed", $"Failed to create bookmark: {ex.Message}"));
        }
    }

    public async Task<Option<BookmarkDto, Error>> GetBookmarkById(int bookmarkId)
    {
        try
        {
            var bookmark = await _bookmarkRepository.GetBookmarkById(bookmarkId);
            if (bookmark == null)
            {
                return Option.None<BookmarkDto, Error>(Error.NotFound("Bookmark.NotFound", "Bookmark not found"));
            }

            return Option.Some<BookmarkDto, Error>(MapToDto(bookmark));
        }
        catch (Exception ex)
        {
            return Option.None<BookmarkDto, Error>(Error.Failure("Bookmark.GetFailed", $"Failed to get bookmark: {ex.Message}"));
        }
    }

    public async Task<Option<List<BookmarkDto>, Error>> GetMyBookmarks()
    {
        try
        {
            var currentUserId = _currentUserService.GetUserId();
            if (!currentUserId.HasValue)
            {
                return Option.None<List<BookmarkDto>, Error>(Error.Unauthorized("User.NotAuthenticated", "User must be authenticated"));
            }

            var bookmarks = await _bookmarkRepository.GetBookmarksByUserId(currentUserId.Value);
            return Option.Some<List<BookmarkDto>, Error>(bookmarks.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            return Option.None<List<BookmarkDto>, Error>(Error.Failure("Bookmark.GetFailed", $"Failed to get bookmarks: {ex.Message}"));
        }
    }

    public async Task<Option<List<BookmarkDto>, Error>> GetBookmarksByMapId(Guid mapId)
    {
        try
        {
            var bookmarks = await _bookmarkRepository.GetBookmarksByMapId(mapId);
            return Option.Some<List<BookmarkDto>, Error>(bookmarks.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            return Option.None<List<BookmarkDto>, Error>(Error.Failure("Bookmark.GetFailed", $"Failed to get bookmarks: {ex.Message}"));
        }
    }

    public async Task<Option<BookmarkDto, Error>> UpdateBookmark(int bookmarkId, UpdateBookmarkRequest request)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserId();
            if (!currentUserId.HasValue)
            {
                return Option.None<BookmarkDto, Error>(Error.Unauthorized("User.NotAuthenticated", "User must be authenticated"));
            }

            var bookmark = await _bookmarkRepository.GetBookmarkById(bookmarkId);
            if (bookmark == null)
            {
                return Option.None<BookmarkDto, Error>(Error.NotFound("Bookmark.NotFound", "Bookmark not found"));
            }

            var belongsToUser = await _bookmarkRepository.CheckBookmarkBelongsToUser(bookmarkId, currentUserId.Value);
            if (!belongsToUser)
            {
                return Option.None<BookmarkDto, Error>(Error.Forbidden("Bookmark.NotAuthorized", "You can only update your own bookmarks"));
            }

            if (request.Name != null)
            {
                bookmark.Name = request.Name;
            }

            if (request.ViewState != null)
            {
                bookmark.ViewState = request.ViewState;
            }

            var updated = await _bookmarkRepository.UpdateBookmark(bookmark);
            if (!updated)
            {
                return Option.None<BookmarkDto, Error>(Error.Failure("Bookmark.UpdateFailed", "Failed to update bookmark"));
            }

            var updatedBookmark = await _bookmarkRepository.GetBookmarkById(bookmarkId);
            if (updatedBookmark == null)
            {
                return Option.None<BookmarkDto, Error>(Error.Failure("Bookmark.NotFound", "Updated bookmark not found"));
            }

            return Option.Some<BookmarkDto, Error>(MapToDto(updatedBookmark));
        }
        catch (Exception ex)
        {
            return Option.None<BookmarkDto, Error>(Error.Failure("Bookmark.UpdateFailed", $"Failed to update bookmark: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> DeleteBookmark(int bookmarkId)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserId();
            if (!currentUserId.HasValue)
            {
                return Option.None<bool, Error>(Error.Unauthorized("User.NotAuthenticated", "User must be authenticated"));
            }

            var bookmark = await _bookmarkRepository.GetBookmarkById(bookmarkId);
            if (bookmark == null)
            {
                return Option.None<bool, Error>(Error.NotFound("Bookmark.NotFound", "Bookmark not found"));
            }

            var belongsToUser = await _bookmarkRepository.CheckBookmarkBelongsToUser(bookmarkId, currentUserId.Value);
            if (!belongsToUser)
            {
                return Option.None<bool, Error>(Error.Forbidden("Bookmark.NotAuthorized", "You can only delete your own bookmarks"));
            }

            var deleted = await _bookmarkRepository.DeleteBookmark(bookmarkId);
            if (!deleted)
            {
                return Option.None<bool, Error>(Error.Failure("Bookmark.DeleteFailed", "Failed to delete bookmark"));
            }

            return Option.Some<bool, Error>(true);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("Bookmark.DeleteFailed", $"Failed to delete bookmark: {ex.Message}"));
        }
    }

    private static BookmarkDto MapToDto(Bookmark bookmark)
    {
        return new BookmarkDto
        {
            BookmarkId = bookmark.BookmarkId,
            MapId = bookmark.MapId,
            MapName = bookmark.Map?.MapName,
            UserId = bookmark.UserId,
            UserName = bookmark.User?.FullName,
            Name = bookmark.Name,
            ViewState = bookmark.ViewState,
            CreatedAt = bookmark.CreatedAt
        };
    }
}

