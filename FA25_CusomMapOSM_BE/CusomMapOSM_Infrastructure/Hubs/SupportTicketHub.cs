using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace CusomMapOSM_Infrastructure.Hubs;

public class SupportTicketHub : Hub
{
    private readonly ILogger<SupportTicketHub> _logger;

    public SupportTicketHub(ILogger<SupportTicketHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var userId = GetUserId();
            var role = GetUserRole();

            if (userId.HasValue)
            {
                var userGroup = $"user_{userId.Value}";
                await Groups.AddToGroupAsync(Context.ConnectionId, userGroup);
                _logger.LogInformation("[SupportTicketHub] User {UserId} connected. ConnectionId: {ConnectionId}", 
                    userId.Value, Context.ConnectionId);

                if (role == "Admin" || role == "SystemAdmin")
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
                    _logger.LogInformation("[SupportTicketHub] Admin {UserId} added to admin group. ConnectionId: {ConnectionId}", 
                        userId.Value, Context.ConnectionId);
                }
            }
            else
            {
                _logger.LogWarning("[SupportTicketHub] Connection without user ID. ConnectionId: {ConnectionId}", 
                    Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SupportTicketHub] Error in OnConnectedAsync. ConnectionId: {ConnectionId}", 
                Context.ConnectionId);
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userId = GetUserId();
            if (userId.HasValue)
            {
                var userGroup = $"user_{userId.Value}";
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userGroup);
                _logger.LogInformation("[SupportTicketHub] User {UserId} disconnected. ConnectionId: {ConnectionId}", 
                    userId.Value, Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SupportTicketHub] Error in OnDisconnectedAsync. ConnectionId: {ConnectionId}", 
                Context.ConnectionId);
        }
    }

    public async Task JoinTicketRoom(int ticketId)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                _logger.LogWarning("[SupportTicketHub] JoinTicketRoom called without user ID. TicketId: {TicketId}", 
                    ticketId);
                return;
            }

            var roomName = $"ticket_{ticketId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
            _logger.LogInformation("[SupportTicketHub] User {UserId} joined ticket room {TicketId}. ConnectionId: {ConnectionId}", 
                userId.Value, ticketId, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SupportTicketHub] Error joining ticket room. TicketId: {TicketId}, ConnectionId: {ConnectionId}", 
                ticketId, Context.ConnectionId);
        }
    }

    public async Task LeaveTicketRoom(int ticketId)
    {
        try
        {
            var userId = GetUserId();
            var roomName = $"ticket_{ticketId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
            _logger.LogInformation("[SupportTicketHub] User {UserId} left ticket room {TicketId}. ConnectionId: {ConnectionId}", 
                userId, ticketId, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SupportTicketHub] Error leaving ticket room. TicketId: {TicketId}, ConnectionId: {ConnectionId}", 
                ticketId, Context.ConnectionId);
        }
    }

    private Guid? GetUserId()
    {
        if (Context.User == null)
        {
            return null;
        }

        var userIdClaim = Context.User.FindFirst(ClaimTypes.NameIdentifier) 
            ?? Context.User.FindFirst("userId");

        if (userIdClaim == null)
        {
            return null;
        }

        if (Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }

    private string? GetUserRole()
    {
        if (Context.User == null)
        {
            return null;
        }

        var roleClaim = Context.User.FindFirst(ClaimTypes.Role) 
            ?? Context.User.FindFirst("role");

        return roleClaim?.Value;
    }
}

