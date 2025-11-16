using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CusomMapOSM_Infrastructure.Hubs;
public class MapCollaborationHub : Hub
{
    private readonly IMapSelectionService _selectionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MapCollaborationHub> _logger;
    public MapCollaborationHub(
        IMapSelectionService selectionService,
        ICurrentUserService currentUserService,
        ILogger<MapCollaborationHub> logger)
    {
        _selectionService = selectionService;
        _currentUserService = currentUserService;
        _logger = logger;
    }
    public async Task JoinMap(Guid mapId)
    {
        var userId = _currentUserService.GetUserId();
        var userName = _currentUserService.GetEmail() ?? "Unknown User";
        try
        {
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", new { Message = "User not authenticated" });
                return;
            }
            
            // Add to group first and wait for it to complete
            await Groups.AddToGroupAsync(Context.ConnectionId, GetMapGroupName(mapId));
            
            // Small delay to ensure group membership is fully registered
            await Task.Delay(50);
            
            var result = await _selectionService.UserJoinMap(mapId, userId.Value, Context.ConnectionId);
            await result.Match(
                async success =>
                {
                    // Get active users after adding the new user
                    var activeUsersResult = await _selectionService.GetActiveUsers(mapId);
                    var activeUsers = activeUsersResult.Match(
                        users => users,
                        error => new List<ActiveMapUserResponse>()
                    );
                    
                    // Get the new user's highlight color
                    var highlightColorResult = await _selectionService.GetUserHighlightColor(userId.Value);
                    var highlightColor = highlightColorResult.Match(
                        color => color,
                        error => "#FF5733" // Default color if not found
                    );
                    
                    // Broadcast UserJoined to all other members in the group
                    // OthersInGroup excludes the caller, so they won't receive their own join event
                    await Clients.OthersInGroup(GetMapGroupName(mapId))
                        .SendAsync("UserJoined", new
                        {
                            UserId = userId,
                            UserName = userName,
                            HighlightColor = highlightColor,
                            JoinedAt = DateTime.UtcNow
                        });
                    
                    // Send InitialState to the caller with all active users
                    await Clients.Caller.SendAsync("InitialState", new
                    {
                        ActiveUsers = activeUsers,
                        Message = $"Joined map {mapId}"
                    });
                    
                    _logger.LogInformation("User {UserId} joined map {MapId}", userId, mapId);
                },
                async error =>
                {
                    await Clients.Caller.SendAsync("Error", new { Message = error.Description });
                    _logger.LogError("Failed to join map {MapId}: {Error}", mapId, error.Description);
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining map {MapId} for user {UserId}", mapId, userId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to join map" });
        }
    }
    public async Task LeaveMap(Guid mapId)
    {
        var userId = _currentUserService.GetUserId();
        var connectionId = Context.ConnectionId;
        if (userId == null)
        {
            await Clients.Caller.SendAsync("Error", new { Message = "User not authenticated" });
            return;
        }
        try
        {
            await _selectionService.UserLeaveMap(mapId, userId.Value);
            await Groups.RemoveFromGroupAsync(connectionId, GetMapGroupName(mapId));
            
            // Remove connection mapping
            await _selectionService.RemoveConnectionMapping(connectionId);
            
            await Clients.OthersInGroup(GetMapGroupName(mapId))
                .SendAsync("UserLeft", new
                {
                    UserId = userId,
                    LeftAt = DateTime.UtcNow
                });
            _logger.LogInformation("User {UserId} left map {MapId}", userId, mapId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving map {MapId} for user {UserId}", mapId, userId);
        }
    }
    public async Task UpdateSelection(UpdateSelectionRequest request)
    {
        var userId = _currentUserService.GetUserId();
        if (userId == null)
        {
            await Clients.Caller.SendAsync("Error", new { Message = "User not authenticated" });
            return;
        }
        try
        {
            var result = await _selectionService.UpdateSelection(request.MapId, userId.Value, request);
            await result.Match(
                async selection =>
                {

                    await Clients.OthersInGroup(GetMapGroupName(request.MapId))
                        .SendAsync("SelectionUpdated", selection);
                    _logger.LogInformation(
                        "User {UserId} updated selection to {Type}:{ObjectId} on map {MapId}",
                        userId, request.SelectionType, request.SelectedObjectId, request.MapId);
                },
                async error =>
                {
                    await Clients.Caller.SendAsync("Error", new { Message = error.Description });
                    _logger.LogError(
                        "Failed to update selection for user {UserId} on map {MapId}: {Error}",
                        userId, request.MapId, error.Description);
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating selection for user {UserId}", userId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to update selection" });
        }
    }
    public async Task ClearSelection(ClearSelectionRequest request)
    {
        var userId = _currentUserService.GetUserId();
        if (userId == null)
        {
            await Clients.Caller.SendAsync("Error", new { Message = "User not authenticated" });
            return;
        }
        try
        {
            var result = await _selectionService.ClearSelection(request.MapId, userId.Value);
            await result.Match(
                async success =>
                {
                    await Clients.OthersInGroup(GetMapGroupName(request.MapId))
                        .SendAsync("SelectionCleared", new
                        {
                            UserId = userId,
                            MapId = request.MapId
                        });
                    _logger.LogInformation("User {UserId} cleared selection on map {MapId}", userId, request.MapId);
                },
                async error =>
                {
                    await Clients.Caller.SendAsync("Error", new { Message = error.Description });
                    _logger.LogError(
                        "Failed to clear selection for user {UserId} on map {MapId}: {Error}",
                        userId, request.MapId, error.Description);
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing selection for user {UserId}", userId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to clear selection" });
        }
    }
    public async Task SendHeartbeat(Guid mapId)
    {
        var userId = _currentUserService.GetUserId();
        if (userId == null)
        {
            return;
        }
        try
        {
            var selectionResult = await _selectionService.GetUserSelection(mapId, userId.Value);
            await selectionResult.Match(
                async selection =>
                {
                    await _selectionService.UpdateSelection(mapId, userId.Value, new UpdateSelectionRequest
                    {
                        MapId = mapId,
                        SelectionType = selection.SelectionType,
                        SelectedObjectId = selection.SelectedObjectId,
                        Latitude = selection.Latitude,
                        Longitude = selection.Longitude
                    });
                },
                error => Task.CompletedTask
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing heartbeat for user {UserId} on map {MapId}", userId, mapId);
        }
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _currentUserService.GetUserId();
        var connectionId = Context.ConnectionId;
        
        try
        {
            _logger.LogInformation("User {UserId} disconnected with connection {ConnectionId}", userId, connectionId);
            
            // Get mapId from connectionId
            var mapIdResult = await _selectionService.GetMapIdFromConnection(connectionId);
            await mapIdResult.Match(
                async mapId =>
                {
                    // Remove user from map
                    if (userId.HasValue)
                    {
                        await _selectionService.UserLeaveMap(mapId, userId.Value);
                        
                        // Remove from group
                        await Groups.RemoveFromGroupAsync(connectionId, GetMapGroupName(mapId));
                        
                        // Notify other users
                        await Clients.OthersInGroup(GetMapGroupName(mapId))
                            .SendAsync("UserLeft", new
                            {
                                UserId = userId,
                                LeftAt = DateTime.UtcNow
                            });
                        
                        _logger.LogInformation("User {UserId} removed from map {MapId} due to disconnect", userId, mapId);
                    }
                    
                    // Remove connection mapping
                    await _selectionService.RemoveConnectionMapping(connectionId);
                },
                async error =>
                {
                    // Connection mapping not found, just log
                    _logger.LogWarning("Connection {ConnectionId} not found in mapping, may have already been cleaned up", connectionId);
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling disconnection for user {UserId} with connection {ConnectionId}", userId, connectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
    #region Private Helpers
    private static string GetMapGroupName(Guid mapId) => $"map:{mapId}";
    #endregion
}