namespace CusomMapOSM_Application.Models.DTOs.Features.Comments;

public record CreateCommentRequest
{
    public Guid? MapId { get; set; }
    public Guid? LayerId { get; set; }
    public required string Content { get; set; }
    public string? Position { get; set; }
}

public record UpdateCommentRequest
{
    public required string Content { get; set; }
    public string? Position { get; set; }
}

public record CommentDto
{
    public required int CommentId { get; set; }
    public Guid? MapId { get; set; }
    public Guid? LayerId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public required string Content { get; set; }
    public string? Position { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
}

