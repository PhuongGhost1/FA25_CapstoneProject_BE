using System.Security.Claims;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Notifications;
using CusomMapOSM_Application.Models.DTOs.Features.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Notifications;

public class NotificationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/notifications")
            .WithTags("Notifications")
            .WithDescription("User notification management endpoints")
            .RequireAuthorization();

        // Get user notifications with pagination
        group.MapGet("/", async (
                ClaimsPrincipal user,
                [FromServices] INotificationService notificationService,
                CancellationToken ct,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 20) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var result = await notificationService.GetUserNotificationsAsync(userId, page, pageSize, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetUserNotifications")
            .WithDescription("Get user notifications with pagination")
            .Produces<GetUserNotificationsResponse>(200)
            .ProducesProblem(401)
            .ProducesProblem(500);

        // Get unread count
        group.MapGet("/unread-count", async (
                ClaimsPrincipal user,
                [FromServices] INotificationService notificationService,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var result = await notificationService.GetUnreadCountAsync(userId, ct);
                return result.Match(
                    success => Results.Ok(new { unreadCount = success }),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetUnreadCount")
            .WithDescription("Get unread notification count")
            .Produces<object>(200)
            .ProducesProblem(401)
            .ProducesProblem(500);

        // Mark notification as read
        group.MapPut("/{notificationId:int}/read", async (
                [FromRoute] int notificationId,
                [FromServices] INotificationService notificationService,
                CancellationToken ct) =>
            {
                var result = await notificationService.MarkNotificationAsReadAsync(notificationId, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("MarkNotificationAsRead")
            .WithDescription("Mark a specific notification as read")
            .Produces<MarkNotificationReadResponse>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // Mark all notifications as read
        group.MapPut("/mark-all-read", async (
                ClaimsPrincipal user,
                [FromServices] INotificationService notificationService,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var result = await notificationService.MarkAllNotificationsAsReadAsync(userId, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("MarkAllNotificationsAsRead")
            .WithDescription("Mark all user notifications as read")
            .Produces<MarkAllNotificationsReadResponse>(200)
            .ProducesProblem(401)
            .ProducesProblem(500);
    }
}
