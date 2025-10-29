using Microsoft.AspNetCore.SignalR;
using CusomMapOSM_Application.Interfaces.Services.StoryMaps;
using CusomMapOSM_Application.Models.StoryMaps;
using System.Collections.Concurrent;

namespace CusomMapOSM_API.Hubs;

public class StoryHub : Hub
{
    private readonly IStoryBroadcastService _broadcastService;
    
    private static readonly ConcurrentDictionary<string, int> SessionViewerCounts = new();

    public StoryHub(IStoryBroadcastService broadcastService)
    {
        _broadcastService = broadcastService;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        foreach (var sessionCode in SessionViewerCounts.Keys.ToList())
        {
            SessionViewerCounts.AddOrUpdate(sessionCode, 0, (key, count) => Math.Max(0, count - 1));
            await Clients.Group($"host_{sessionCode}").SendAsync("ViewerCountChanged", 
                SessionViewerCounts.GetValueOrDefault(sessionCode, 0));
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinAsViewer(string sessionCode)
    {
        if (!_broadcastService.TryGetSession(sessionCode, out var sessionInfo))
        {
            await Clients.Caller.SendAsync("SessionNotFound");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"viewers_{sessionCode}");
        
        var newCount = SessionViewerCounts.AddOrUpdate(sessionCode, 1, (key, count) => count + 1);
        
        var currentState = _broadcastService.GetState(sessionCode);
        if (currentState != null)
        {
            await Clients.Caller.SendAsync("StoryStepChanged", currentState);
        }
        
        await Clients.Group($"host_{sessionCode}").SendAsync("ViewerCountChanged", newCount);
        
        await Clients.Caller.SendAsync("JoinedSession", new
        {
            SessionCode = sessionCode,
            MapId = sessionInfo.MapId,
            StoryMapId = sessionInfo.StoryMapId,
            ViewerCount = newCount
        });
    }

    public async Task JoinAsHost(string sessionCode)
    {
        if (!_broadcastService.TryGetSession(sessionCode, out var sessionInfo))
        {
            await Clients.Caller.SendAsync("SessionNotFound");
            return;
        }

        _broadcastService.BindHost(sessionCode, Context.ConnectionId);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, $"host_{sessionCode}");
        
        var viewerCount = SessionViewerCounts.GetValueOrDefault(sessionCode, 0);
        await Clients.Caller.SendAsync("HostJoined", new
        {
            SessionCode = sessionCode,
            ViewerCount = viewerCount,
            CurrentState = _broadcastService.GetState(sessionCode)
        });
    }

    public async Task BroadcastStep(string sessionCode, StoryBroadcastState stepState)
    {
        if (!_broadcastService.IsHost(sessionCode, Context.ConnectionId))
        {
            await Clients.Caller.SendAsync("Unauthorized", "Only host can broadcast");
            return;
        }

        if (!_broadcastService.TryGetSession(sessionCode, out _))
        {
            await Clients.Caller.SendAsync("SessionNotFound");
            return;
        }

        _broadcastService.UpdateState(sessionCode, stepState);

        await Clients.Group($"viewers_{sessionCode}").SendAsync("StoryStepChanged", stepState);
        
        await Clients.Caller.SendAsync("StepBroadcasted", stepState);
    }

    public async Task GetViewerCount(string sessionCode)
    {
        if (!_broadcastService.IsHost(sessionCode, Context.ConnectionId))
        {
            await Clients.Caller.SendAsync("Unauthorized", "Only host can get viewer count");
            return;
        }

        var count = SessionViewerCounts.GetValueOrDefault(sessionCode, 0);
        await Clients.Caller.SendAsync("ViewerCount", count);
    }

    public async Task EndSession(string sessionCode)
    {
        if (!_broadcastService.IsHost(sessionCode, Context.ConnectionId))
        {
            await Clients.Caller.SendAsync("Unauthorized", "Only host can end session");
            return;
        }

        await Clients.Group($"viewers_{sessionCode}").SendAsync("SessionEnded");
        
        _broadcastService.EndSession(sessionCode);
        SessionViewerCounts.TryRemove(sessionCode, out _);

        await Clients.Caller.SendAsync("SessionEnded");
    }
}
