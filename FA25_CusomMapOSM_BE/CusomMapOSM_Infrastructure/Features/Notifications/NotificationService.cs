using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Notifications;
using CusomMapOSM_Application.Models.DTOs.Features.Notifications;
using CusomMapOSM_Domain.Entities.Notifications;
using CusomMapOSM_Domain.Entities.Notifications.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Notifications;
using CusomMapOSM_Application.Interfaces.Services;
using CusomMapOSM_Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Optional;
using System.Text.Json;

namespace CusomMapOSM_Infrastructure.Features.Notifications;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(
        INotificationRepository notificationRepository,
        IHubContext<NotificationHub> hubContext)
    {
        _notificationRepository = notificationRepository;
        _hubContext = hubContext;
    }

    public async Task<Option<GetUserNotificationsResponse, Error>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        try
        {
            var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, page, pageSize, ct);
            var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId, ct);
            var totalCount = await _notificationRepository.GetTotalCountAsync(userId, ct);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var notificationDtos = notifications.Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                Type = n.Type ?? "",
                Message = n.Message ?? "",
                Status = n.Status ?? "",
                CreatedAt = n.CreatedAt,
                SentAt = n.SentAt,
                IsRead = n.IsRead,
                Metadata = n.Metadata
            }).ToList();

            return Option.Some<GetUserNotificationsResponse, Error>(new GetUserNotificationsResponse
            {
                Notifications = notificationDtos,
                TotalCount = totalCount,
                UnreadCount = unreadCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            });
        }
        catch (Exception ex)
        {
            return Option.None<GetUserNotificationsResponse, Error>(Error.Failure("Notification.GetFailed", $"Failed to get notifications: {ex.Message}"));
        }
    }

    public async Task<Option<MarkNotificationReadResponse, Error>> MarkNotificationAsReadAsync(int notificationId, CancellationToken ct = default)
    {
        try
        {
            // Get notification to get userId before marking as read
            var notification = await _notificationRepository.GetNotificationByIdAsync(notificationId, ct);
            if (notification == null)
            {
                return Option.None<MarkNotificationReadResponse, Error>(Error.NotFound("Notification.NotFound", "Notification not found"));
            }

            var result = await _notificationRepository.MarkAsReadAsync(notificationId, ct);
            if (!result)
            {
                return Option.None<MarkNotificationReadResponse, Error>(Error.NotFound("Notification.NotFound", "Notification not found"));
            }

            // Update unread count via SignalR
            try
            {
                var unreadCount = await _notificationRepository.GetUnreadCountAsync(notification.UserId, ct);
                await _hubContext.Clients.Group($"user_{notification.UserId}").SendAsync("UnreadCountUpdated", unreadCount, cancellationToken: ct);
            }
            catch
            {
                // Ignore SignalR errors for read status updates
            }

            return Option.Some<MarkNotificationReadResponse, Error>(new MarkNotificationReadResponse
            {
                Result = "Notification marked as read successfully"
            });
        }
        catch (Exception ex)
        {
            return Option.None<MarkNotificationReadResponse, Error>(Error.Failure("Notification.UpdateFailed", $"Failed to mark notification as read: {ex.Message}"));
        }
    }

    public async Task<Option<MarkAllNotificationsReadResponse, Error>> MarkAllNotificationsAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId, ct);
            var result = await _notificationRepository.MarkAllAsReadAsync(userId, ct);

            if (!result)
            {
                return Option.None<MarkAllNotificationsReadResponse, Error>(Error.Failure("Notification.UpdateFailed", "Failed to mark notifications as read"));
            }

            // Update unread count via SignalR (should be 0 now)
            try
            {
                await _hubContext.Clients.Group($"user_{userId}").SendAsync("UnreadCountUpdated", 0, cancellationToken: ct);
            }
            catch
            {
                // Ignore SignalR errors for read status updates
            }

            return Option.Some<MarkAllNotificationsReadResponse, Error>(new MarkAllNotificationsReadResponse
            {
                Result = "All notifications marked as read successfully",
                MarkedCount = unreadCount
            });
        }
        catch (Exception ex)
        {
            return Option.None<MarkAllNotificationsReadResponse, Error>(Error.Failure("Notification.UpdateFailed", $"Failed to mark all notifications as read: {ex.Message}"));
        }
    }

    public async Task<Option<int, Error>> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var count = await _notificationRepository.GetUnreadCountAsync(userId, ct);
            return Option.Some<int, Error>(count);
        }
        catch (Exception ex)
        {
            return Option.None<int, Error>(Error.Failure("Notification.GetCountFailed", $"Failed to get unread count: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> CreateNotificationAsync(Guid userId, string type, string message, string? metadata = null, CancellationToken ct = default)
    {
        try
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Message = message,
                Status = "pending",
                Metadata = metadata,
                CreatedAt = DateTime.UtcNow,
                SentAt = DateTime.UtcNow
            };

            var result = await _notificationRepository.CreateNotificationAsync(notification, ct);
            
            if (result && notification.NotificationId > 0)
            {
                // Push notification to user via SignalR (only if NotificationId was set by database)
                await PushNotificationToUserAsync(userId, notification, ct);
            }
            
            return Option.Some<bool, Error>(result);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("Notification.CreateFailed", $"Failed to create notification: {ex.Message}"));
        }
    }

    private async Task PushNotificationToUserAsync(Guid userId, Notification notification, CancellationToken ct = default)
    {
        try
        {
            var notificationDto = new NotificationDto
            {
                NotificationId = notification.NotificationId,
                Type = notification.Type ?? "",
                Message = notification.Message ?? "",
                Status = notification.Status ?? "",
                CreatedAt = notification.CreatedAt,
                SentAt = notification.SentAt,
                IsRead = notification.IsRead,
                Metadata = notification.Metadata
            };

            // Send notification to user's group
            await _hubContext.Clients.Group($"user_{userId}").SendAsync("NotificationReceived", notificationDto, cancellationToken: ct);
            
            // Also update unread count
            var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId, ct);
            await _hubContext.Clients.Group($"user_{userId}").SendAsync("UnreadCountUpdated", unreadCount, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            // Log error but don't fail the notification creation
            // SignalR push is best-effort; notification is already saved to DB
            Console.WriteLine($"Failed to push notification via SignalR: {ex.Message}");
        }
    }

    public async Task<Option<bool, Error>> CreateQuotaWarningNotificationAsync(Guid userId, string quotaType, int currentUsage, int limit, int percentageUsed, CancellationToken ct = default)
    {
        try
        {
            // Check if user already has a quota warning notification for this type in the last 24 hours
            var hasRecentNotification = await _notificationRepository.HasQuotaNotificationAsync(userId, quotaType, NotificationTypeEnum.QuotaWarning.ToString(), ct);
            if (hasRecentNotification)
            {
                return Option.Some<bool, Error>(true); // Already notified, don't spam
            }

            var metadata = JsonSerializer.Serialize(new
            {
                quotaType,
                currentUsage,
                limit,
                percentageUsed,
                timestamp = DateTime.UtcNow
            });

            var message = $"Your {quotaType} usage is at {percentageUsed}% ({currentUsage}/{limit}). Consider upgrading your plan if you need more resources.";

            var result = await CreateNotificationAsync(userId, NotificationTypeEnum.QuotaWarning.ToString(), message, metadata, ct);

            // Also send email notification
            if (result.HasValue)
            {
                // Get user email for email notification
                // This would need to be implemented with user service
                // await _emailNotificationService.SendQuotaWarningNotificationAsync(userEmail, userName, quotaType, currentUsage, limit, percentageUsed);
            }

            return result;
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("Notification.QuotaWarningFailed", $"Failed to create quota warning notification: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> CreateQuotaExceededNotificationAsync(Guid userId, string quotaType, int currentUsage, int limit, CancellationToken ct = default)
    {
        try
        {
            // Check if user already has a quota exceeded notification for this type in the last 24 hours
            var hasRecentNotification = await _notificationRepository.HasQuotaNotificationAsync(userId, quotaType, NotificationTypeEnum.QuotaExceeded.ToString(), ct);
            if (hasRecentNotification)
            {
                return Option.Some<bool, Error>(true); // Already notified, don't spam
            }

            var metadata = JsonSerializer.Serialize(new
            {
                quotaType,
                currentUsage,
                limit,
                timestamp = DateTime.UtcNow
            });

            var message = $"Your {quotaType} quota has been exceeded ({currentUsage}/{limit}). Please upgrade your plan to continue using this feature.";

            var result = await CreateNotificationAsync(userId, NotificationTypeEnum.QuotaExceeded.ToString(), message, metadata, ct);

            // Also send email notification
            if (result.HasValue)
            {
                // Get user email for email notification
                // await _emailNotificationService.SendQuotaExceededNotificationAsync(userEmail, userName, quotaType, currentUsage, limit);
            }

            return result;
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("Notification.QuotaExceededFailed", $"Failed to create quota exceeded notification: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> CreateTransactionCompletedNotificationAsync(Guid userId, decimal amount, string planName, CancellationToken ct = default)
    {
        try
        {
            var metadata = JsonSerializer.Serialize(new
            {
                amount,
                planName,
                timestamp = DateTime.UtcNow
            });

            var message = $"Your payment of ${amount:F2} for {planName} has been processed successfully. Your membership is now active.";

            var result = await CreateNotificationAsync(userId, NotificationTypeEnum.TransactionCompleted.ToString(), message, metadata, ct);

            // Also send email notification
            if (result.HasValue)
            {
                // Get user email for email notification
                // await _emailNotificationService.SendTransactionCompletedNotificationAsync(userEmail, userName, amount, planName);
            }

            return result;
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("Notification.TransactionFailed", $"Failed to create transaction completed notification: {ex.Message}"));
        }
    }
}
