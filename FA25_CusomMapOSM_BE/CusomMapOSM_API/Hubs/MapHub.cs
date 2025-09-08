using Microsoft.AspNetCore.SignalR;
using CusomMapOSM_Infrastructure.Services;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using CusomMapOSM_Application.Interfaces;
using CusomMapOSM_Infrastructure.Features.Collaboration;

namespace CusomMapOSM_API.Hubs;

public class MapHub : Hub
{
    private readonly CollaborativeMapService _collaborativeService;
    private static readonly ConcurrentDictionary<string, HashSet<string>> MapUsers = new();
    private static readonly ConcurrentDictionary<string, List<MapEditOperation>> OperationHistory = new();
    private const int MAX_HISTORY_PER_MAP = 1000;

    public static int GetTotalMaps() => MapUsers.Count;
    public static int GetTotalUsers() => MapUsers.Values.Sum(users => users.Count);

    public MapHub(CollaborativeMapService collaborativeService)
    {
        _collaborativeService = collaborativeService;
    }

    // Dictionary to store connected users and their current map ID
    private static Dictionary<string, string> ConnectedUsers = new();

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        if (ConnectedUsers.ContainsKey(connectionId))
        {
            var mapId = ConnectedUsers[connectionId];
            await Groups.RemoveFromGroupAsync(connectionId, mapId);
            ConnectedUsers.Remove(connectionId);
            await Clients.Group(mapId).SendAsync("UserDisconnected", connectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Join a specific map editing session
    public async Task JoinMap(string mapId)
    {
        var connectionId = Context.ConnectionId;


        // Remove from previous map if any
        if (ConnectedUsers.ContainsKey(connectionId))
        {
            var oldMapId = ConnectedUsers[connectionId];
            await Groups.RemoveFromGroupAsync(connectionId, oldMapId);
            ConnectedUsers.Remove(connectionId);

            // Remove from old map's user list
            if (MapUsers.TryGetValue(oldMapId, out var oldUsers))
            {
                oldUsers.Remove(connectionId);
                await Clients.Group(oldMapId).SendAsync("UserLeft", connectionId);

                // Clean up empty maps
                if (oldUsers.Count == 0)
                {
                    MapUsers.TryRemove(oldMapId, out _);
                    OperationHistory.TryRemove(oldMapId, out _);
                }
            }
        }

        // Add to new map
        await Groups.AddToGroupAsync(connectionId, mapId);
        ConnectedUsers[connectionId] = mapId;

        // Add to map's user list
        var users = MapUsers.GetOrAdd(mapId, _ => new HashSet<string>());


        users.Add(connectionId);

        // Send current users list to the new user
        await Clients.Caller.SendAsync("UserList", users);

        // Notify others
        await Clients.Group(mapId).SendAsync("UserJoined", connectionId);

        // Send recent history
        if (OperationHistory.TryGetValue(mapId, out var history))
        {
            await Clients.Caller.SendAsync("InitialHistory", history);
        }
    }

    // Broadcast map changes to all users in the same map session
    public async Task UpdateMap(string mapId, MapEditOperation operation)
    {
        // Validate operation
        var (isValid, error) = await _collaborativeService.ValidateOperation(operation, mapId);
        if (!isValid)
        {
            await Clients.Caller.SendAsync("OperationRejected", error);
            return;
        }

        // Transform operation if needed
        var concurrentOps = OperationHistory.GetOrAdd(mapId, _ => new List<MapEditOperation>())
            .Where(op => op.Timestamp > operation.Timestamp.AddSeconds(-5))
            .ToList();

        var transformedOp = await _collaborativeService.TransformOperation(operation, concurrentOps);

        // Add to history
        var history = OperationHistory.GetOrAdd(mapId, _ => new List<MapEditOperation>());
        history.Add(transformedOp);

        // Trim history if too long
        if (history.Count > MAX_HISTORY_PER_MAP)
        {
            history.RemoveRange(0, history.Count - MAX_HISTORY_PER_MAP);
        }

        // Increment version
        await _collaborativeService.IncrementVersion(mapId);

        // Broadcast to all users
        await Clients.Group(mapId).SendAsync("MapUpdated", transformedOp);
    }

    // Undo last operation
    public async Task UndoOperation(string mapId)
    {
        if (!OperationHistory.TryGetValue(mapId, out var history) || !history.Any())
        {
            return;
        }

        var lastOp = history.LastOrDefault(op => op.UserId == Context.ConnectionId && !op.IsReverted);
        if (lastOp == null)
        {
            return;
        }

        // Mark operation as reverted
        lastOp.IsReverted = true;

        // Create reverse operation
        var undoOp = new MapEditOperation
        {
            Type = $"undo_{lastOp.Type}",
            Data = lastOp.Data,
            UserId = Context.ConnectionId,
            Timestamp = DateTime.UtcNow
        };

        // Add undo operation to history
        history.Add(undoOp);

        // Broadcast undo
        await Clients.Group(mapId).SendAsync("MapUpdated", undoOp);
    }

    // Request to lock an object for editing
    public async Task LockObject(string mapId, string objectId)
    {
        var success = await _collaborativeService.TryLockObject(mapId, objectId, Context.ConnectionId);
        if (success)
        {
            await Clients.Group(mapId).SendAsync("ObjectLocked", objectId, Context.ConnectionId);
        }
    }

    // Release lock on an object
    public async Task ReleaseLock(string mapId, string objectId)
    {
        await _collaborativeService.ReleaseLock(mapId, objectId);
        await Clients.Group(mapId).SendAsync("ObjectUnlocked", objectId);
    }

    // Broadcast cursor position to show where other users are working
    public async Task UpdateCursor(string mapId, double lat, double lng)
    {
        await Clients.OthersInGroup(mapId).SendAsync("CursorMoved", Context.ConnectionId, lat, lng);
    }
}