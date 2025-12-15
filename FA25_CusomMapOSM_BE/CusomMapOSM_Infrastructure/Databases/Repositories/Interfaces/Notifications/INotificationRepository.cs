using CusomMapOSM_Domain.Entities.Notifications;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Notifications;

public interface INotificationRepository
{
    Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<List<Notification>> GetUnreadNotificationsAsync(Guid userId, CancellationToken ct = default);
    Task<Notification?> GetNotificationByIdAsync(int notificationId, CancellationToken ct = default);
    Task<bool> MarkAsReadAsync(int notificationId, CancellationToken ct = default);
    Task<bool> MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
    Task<bool> CreateNotificationAsync(Notification notification, CancellationToken ct = default);
    Task<bool> UpdateNotificationAsync(Notification notification, CancellationToken ct = default);
    Task<bool> DeleteNotificationAsync(int notificationId, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    Task<int> GetTotalCountAsync(Guid userId, CancellationToken ct = default);
    Task<bool> HasQuotaNotificationAsync(Guid userId, string quotaType, string notificationType, CancellationToken ct = default);
}
