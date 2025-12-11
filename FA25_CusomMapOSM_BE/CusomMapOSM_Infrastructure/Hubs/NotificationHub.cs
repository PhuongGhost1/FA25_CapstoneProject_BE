using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace CusomMapOSM_Infrastructure.Hubs;

public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        if (Context.User != null)
        {
            var claims = Context.User.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
        }
        
        var userId = GetUserId();
        var role = GetUserRole();
        
        if (userId.HasValue)
        {
            try
            {
                var groupName = $"user_{userId.Value}";
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                _logger.LogInformation("[NotificationHub] User {UserId} connected. ConnectionId: {ConnectionId}", 
                    userId.Value, Context.ConnectionId);

                if (role == "Admin" || role == "SystemAdmin" || role == "admin" || role == "systemadmin")
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
                    _logger.LogInformation("[NotificationHub] Admin {UserId} added to admin group. ConnectionId: {ConnectionId}", 
                        userId.Value, Context.ConnectionId);
                }
                
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[NotificationHub] ❌ Error adding connection to group. ConnectionId: {ConnectionId}, UserId: {UserId}", 
                    Context.ConnectionId, userId.Value);
                Context.Abort();
            }
        }
        else
        {
            _logger.LogWarning("[NotificationHub] ⚠️ No user ID found, aborting connection. ConnectionId: {ConnectionId}", 
                Context.ConnectionId);
            Context.Abort();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var role = GetUserRole();
        
        if (userId.HasValue)
        {
            try
            {
                var groupName = $"user_{userId.Value}";
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                
                if (role == "Admin" || role == "SystemAdmin" || role == "admin" || role == "systemadmin")
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admin");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[NotificationHub] ❌ Error removing connection from group. ConnectionId: {ConnectionId}, UserId: {UserId}", 
                    Context.ConnectionId, userId.Value);
            }
        }
        
        await base.OnDisconnectedAsync(exception);
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

