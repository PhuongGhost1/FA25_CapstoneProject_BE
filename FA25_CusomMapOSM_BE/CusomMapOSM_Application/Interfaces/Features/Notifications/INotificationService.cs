using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Notifications;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Notifications;

public interface INotificationService
{
    // User notification management
    Task<Option<GetUserNotificationsResponse, Error>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<Option<MarkNotificationReadResponse, Error>> MarkNotificationAsReadAsync(int notificationId, CancellationToken ct = default);
    Task<Option<MarkAllNotificationsReadResponse, Error>> MarkAllNotificationsAsReadAsync(Guid userId, CancellationToken ct = default);
    Task<Option<int, Error>> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);

    // Notification creation (internal use)
    Task<Option<bool, Error>> CreateNotificationAsync(Guid userId, string type, string message, string? metadata = null, CancellationToken ct = default);
    Task<Option<bool, Error>> CreateQuotaWarningNotificationAsync(Guid userId, string quotaType, int currentUsage, int limit, int percentageUsed, CancellationToken ct = default);
    Task<Option<bool, Error>> CreateQuotaExceededNotificationAsync(Guid userId, string quotaType, int currentUsage, int limit, CancellationToken ct = default);
    Task<Option<bool, Error>> CreateTransactionCompletedNotificationAsync(Guid userId, decimal amount, string planName, CancellationToken ct = default);
    Task<Option<bool, Error>> CreateTransactionPendingNotificationAsync(
        Guid userId,
        Guid transactionId,
        string planName,
        string? planSummary,
        string? paymentUrl,
        CancellationToken ct = default);
}
