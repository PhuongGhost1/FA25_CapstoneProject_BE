using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CusomMapOSM_Infrastructure.Hubs;
public class MapCollaborationHub : Hub
{
    private readonly IMapSelectionService _selectionService;
    private readonly ILogger<MapCollaborationHub> _logger;
    private readonly IUserService _userService;
    public MapCollaborationHub(
        IMapSelectionService selectionService,
        ILogger<MapCollaborationHub> logger,
        IUserService userService)
    {
        _selectionService = selectionService;
        _logger = logger;
        _userService = userService;
    }
    public async Task JoinMap(Guid mapId, Guid userId)
    {
        var userResult = await _userService.GetUserByIdAsync(userId);
        var user = userResult.Match(u => u, error => throw new Exception($"User not found: {error.Description}"));
        try
        {
            // Add to group first and wait for it to complete
            await Groups.AddToGroupAsync(Context.ConnectionId, GetMapGroupName(mapId));

            // Small delay to ensure group membership is fully registered
            await Task.Delay(50);

            var result = await _selectionService.UserJoinMap(mapId, userId, Context.ConnectionId);
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
                    var highlightColorResult = await _selectionService.GetUserHighlightColor(userId);
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
                            UserName = user.FullName,
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
    public async Task LeaveMap(Guid mapId, Guid userId)
    {
        _logger.LogWarning("LeaveMap called by user {UserId} for map {MapId}", userId, mapId);
        var connectionId = Context.ConnectionId;
        try
        {
            await _selectionService.UserLeaveMap(mapId, userId);
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
    public async Task UpdateSelection(UpdateSelectionRequest request, Guid userId)
    {
        _logger.LogWarning("UpdateSelection called by user {UserId} for map {MapId}", userId, request.MapId);
        try
        {
            var result = await _selectionService.UpdateSelection(request.MapId, userId, request);
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
    public async Task ClearSelection(ClearSelectionRequest request, Guid userId)
    {
        _logger.LogWarning("ClearSelection called by user {UserId} for map {MapId}", userId, request.MapId);
        try
        {
            var result = await _selectionService.ClearSelection(request.MapId, userId);
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
    public async Task SendHeartbeat(Guid mapId, Guid userId)
    {
        _logger.LogWarning("SendHeartbeat called by user {UserId} for map {MapId}", userId, mapId);
        try
        {
            var selectionResult = await _selectionService.GetUserSelection(mapId, userId);
            await selectionResult.Match(
                async selection =>
                {
                    await _selectionService.UpdateSelection(mapId, userId, new UpdateSelectionRequest
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
        var userIdClaim = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = null;
        if (Guid.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        _logger.LogWarning("OnDisconnectedAsync called by user {UserId} with connection {ConnectionId}", userId, Context.ConnectionId);
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