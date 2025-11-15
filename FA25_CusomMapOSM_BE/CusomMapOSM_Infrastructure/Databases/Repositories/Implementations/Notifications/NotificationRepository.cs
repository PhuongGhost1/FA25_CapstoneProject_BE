using CusomMapOSM_Domain.Entities.Notifications;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Notifications;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Notifications;

public class NotificationRepository : INotificationRepository
{
    private readonly CustomMapOSMDbContext _context;

    public NotificationRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<List<Notification>> GetUnreadNotificationsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Notification?> GetNotificationByIdAsync(int notificationId, CancellationToken ct = default)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId, ct);
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, CancellationToken ct = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId, ct);

        if (notification == null) return false;

        notification.IsRead = true;
        _context.Notifications.Update(notification);
        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        _context.Notifications.UpdateRange(notifications);
        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> CreateNotificationAsync(Notification notification, CancellationToken ct = default)
    {
        _context.Notifications.Add(notification);
        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> UpdateNotificationAsync(Notification notification, CancellationToken ct = default)
    {
        _context.Notifications.Update(notification);
        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteNotificationAsync(int notificationId, CancellationToken ct = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId, ct);

        if (notification == null) return false;

        _context.Notifications.Remove(notification);
        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);
    }

    public async Task<int> GetTotalCountAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId, ct);
    }

    public async Task<bool> HasQuotaNotificationAsync(Guid userId, string quotaType, string notificationType, CancellationToken ct = default)
    {
        // Check if user already has a quota notification of this type in the last 24 hours
        var yesterday = DateTime.UtcNow.AddDays(-1);

        return await _context.Notifications
            .AnyAsync(n => n.UserId == userId
                          && n.Type == notificationType
                          && n.CreatedAt >= yesterday
                          && n.Metadata != null
                          && n.Metadata.Contains($"\"quotaType\":\"{quotaType}\""), ct);
    }
}
