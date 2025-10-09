namespace CusomMapOSM_Application.Models.DTOs.Features.Notifications;

public record NotificationDto
{
    public required int NotificationId { get; set; }
    public required string Type { get; set; }
    public required string Message { get; set; }
    public required string Status { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public required bool IsRead { get; set; }
    public string? Metadata { get; set; }
}

public record GetUserNotificationsResponse
{
    public required List<NotificationDto> Notifications { get; set; }
    public required int TotalCount { get; set; }
    public required int UnreadCount { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
}

public record MarkNotificationReadResponse
{
    public required string Result { get; set; }
}

public record MarkAllNotificationsReadResponse
{
    public required string Result { get; set; }
    public required int MarkedCount { get; set; }
}
