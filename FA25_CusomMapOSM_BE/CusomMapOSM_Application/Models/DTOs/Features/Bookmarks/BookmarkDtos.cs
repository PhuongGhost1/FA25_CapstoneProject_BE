namespace CusomMapOSM_Application.Models.DTOs.Features.Bookmarks;

public record CreateBookmarkRequest
{
    public required Guid MapId { get; set; }
    public string? Name { get; set; }
    public string? ViewState { get; set; }
}

public record UpdateBookmarkRequest
{
    public string? Name { get; set; }
    public string? ViewState { get; set; }
}

public record BookmarkDto
{
    public required int BookmarkId { get; set; }
    public required Guid MapId { get; set; }
    public string? MapName { get; set; }
    public required Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? Name { get; set; }
    public string? ViewState { get; set; }
    public required DateTime CreatedAt { get; set; }
}

