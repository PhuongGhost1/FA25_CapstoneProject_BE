using CusomMapOSM_Application.Interfaces.Features.QuickPolls;
using CusomMapOSM_Application.Interfaces.Services.Cache;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.QuickPolls;
using CusomMapOSM_Application.Models.DTOs.Features.Sessions.Events;
using CusomMapOSM_Application.Models.DTOs.Features.QuickPolls.Request;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CusomMapOSM_Infrastructure.Hubs;

public class SessionHub : Hub
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ISessionParticipantRepository _participantRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRedisCacheService _redisCacheService;
    private readonly IQuickPollService _quickPollService;
    private readonly ILogger<SessionHub> _logger;

    // Track connection to session mapping for cleanup
    private static readonly Dictionary<string, Guid> _connectionSessionMap = new();
    private static readonly object _lock = new();
    private static readonly TimeSpan MapStateCacheTtl = TimeSpan.FromHours(6);

    public SessionHub(
        ISessionRepository sessionRepository,
        ISessionParticipantRepository participantRepository,
        ICurrentUserService currentUserService,
        IRedisCacheService redisCacheService,
        IQuickPollService quickPollService,
        ILogger<SessionHub> logger)
    {
        _sessionRepository = sessionRepository;
        _participantRepository = participantRepository;
        _currentUserService = currentUserService;
        _redisCacheService = redisCacheService;
        _quickPollService = quickPollService;
        _logger = logger;
    }

    /// <summary>
    /// Join a session room to receive real-time updates
    /// </summary>
    public async Task JoinSession(Guid sessionId)
    {
        try
        {
            // Validate session exists
            var session = await _sessionRepository.GetSessionById(sessionId);
            if (session == null)
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Session not found" });
                return;
            }

            var connectionId = Context.ConnectionId;
            var groupName = GetSessionGroupName(sessionId);

            // Add to SignalR group
            await Groups.AddToGroupAsync(connectionId, groupName);

            // Track connection mapping
            lock (_lock)
            {
                _connectionSessionMap[connectionId] = sessionId;
            }

            _logger.LogInformation(
                "[SessionHub] Connection {ConnectionId} joined session {SessionId}",
                connectionId, sessionId);

            // Send initial state to caller
            var mapState = await _redisCacheService.Get<MapStateSyncEvent>(GetMapStateCacheKey(sessionId));

            await Clients.Caller.SendAsync("JoinedSession", new
            {
                SessionId = sessionId,
                SessionCode = session.SessionCode,
                Status = session.Status.ToString(),
                Message = $"Successfully joined session {session.SessionCode}",
                MapState = mapState
            });

            if (mapState != null)
            {
                await Clients.Caller.SendAsync("MapStateSync", mapState);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SessionHub] Error joining session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to join session" });
        }
    }

    /// <summary>
    /// Leave a session room
    /// </summary>
    public async Task LeaveSession(Guid sessionId)
    {
        try
        {
            var connectionId = Context.ConnectionId;
            var groupName = GetSessionGroupName(sessionId);

            await Groups.RemoveFromGroupAsync(connectionId, groupName);

            // Remove connection mapping
            lock (_lock)
            {
                _connectionSessionMap.Remove(connectionId);
            }

            _logger.LogInformation(
                "[SessionHub] Connection {ConnectionId} left session {SessionId}",
                connectionId, sessionId);

            await Clients.Caller.SendAsync("LeftSession", new
            {
                SessionId = sessionId,
                Message = "Successfully left session"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SessionHub] Error leaving session {SessionId}", sessionId);
        }
    }

    /// <summary>
    /// Teacher syncs map state (Teacher Focus feature)
    /// </summary>
    public async Task SyncMapState(Guid sessionId, MapStateSyncRequest request)
    {
        try
        {
            var userId = _currentUserService.GetUserId();
            var userName = _currentUserService.GetEmail() ?? "Teacher";

            // Validate user is the session host
            var session = await _sessionRepository.GetSessionById(sessionId);
            if (session == null || session.HostUserId != userId)
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Only host can sync map state" });
                return;
            }

            var syncEvent = new MapStateSyncEvent
            {
                SessionId = sessionId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                ZoomLevel = request.ZoomLevel,
                Bearing = request.Bearing,
                Pitch = request.Pitch,
                SyncedBy = userName,
                SyncedAt = DateTime.UtcNow
            };

            var cacheKey = GetMapStateCacheKey(sessionId);
            await _redisCacheService.Set(cacheKey, syncEvent, MapStateCacheTtl);

            // Broadcast to all participants except the caller
            await Clients.OthersInGroup(GetSessionGroupName(sessionId))
                .SendAsync("MapStateSync", syncEvent);

            _logger.LogInformation(
                "[SessionHub] Host {UserId} synced map state for session {SessionId}",
                userId, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SessionHub] Error syncing map state for session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to sync map state" });
        }
    }

    #region Quick Polls

    public async Task CreatePoll(CreateQuickPollRequest request)
    {
        var result = await _quickPollService.CreatePoll(request);
        await result.Match(
            async poll =>
            {
                await Clients.Group(GetSessionGroupName(request.SessionId))
                    .SendAsync("PollCreated", poll);
                if (poll.Status == PollStatusEnum.Active)
                {
                    await Clients.Group(GetSessionGroupName(request.SessionId))
                        .SendAsync("PollActivated", poll);
                }
            },
            async error =>
            {
                await Clients.Caller.SendAsync("Error", error.Description);
            });
    }

    public async Task SubmitVote(VoteRequest request)
    {
        var result = await _quickPollService.Vote(request);
        await result.Match(
            async _ =>
            {
                var resultsOption = await _quickPollService.GetPollResults(request.PollId);
                await resultsOption.Match(
                    async results =>
                    {
                        await Clients.Group(GetSessionGroupName(results.SessionId))
                            .SendAsync("PollResultsUpdated", results);
                    },
                    async _ => { });
            },
            async error =>
            {
                await Clients.Caller.SendAsync("Error", error.Description);
            });
    }

    public async Task ClosePoll(Guid pollId, Guid sessionId)
    {
        var result = await _quickPollService.ClosePoll(pollId);
        await result.Match(
            async _ =>
            {
                await Clients.Group(GetSessionGroupName(sessionId))
                    .SendAsync("PollClosed", pollId);
            },
            async error =>
            {
                await Clients.Caller.SendAsync("Error", error.Description);
            });
    }

    public async Task GetResults(Guid pollId)
    {
        var result = await _quickPollService.GetPollResults(pollId);
        await result.Match(
            async poll => await Clients.Caller.SendAsync("PollResults", poll),
            async error => await Clients.Caller.SendAsync("Error", error.Description));
    }

    public async Task GetActivePoll(Guid sessionId)
    {
        var result = await _quickPollService.GetActivePoll(sessionId);
        await result.Match(
            async poll => await Clients.Caller.SendAsync("PollResults", poll),
            async error => await Clients.Caller.SendAsync("Error", error.Description));
    }

    public async Task ActivatePoll(Guid pollId)
    {
        var result = await _quickPollService.ActivatePoll(pollId);
        await result.Match(
            async poll =>
            {
                await Clients.Group(GetSessionGroupName(poll.SessionId))
                    .SendAsync("PollActivated", poll);
            },
            async error => await Clients.Caller.SendAsync("Error", error.Description));
    }

    public async Task GetPollHistory(Guid sessionId)
    {
        var result = await _quickPollService.GetPollHistory(sessionId);
        await result.Match(
            async polls => await Clients.Caller.SendAsync("PollHistory", polls),
            async error => await Clients.Caller.SendAsync("Error", error.Description));
    }

    #endregion

    /// <summary>
    /// Auto-cleanup on disconnect
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        Guid? sessionId = null;

        try
        {
            // Get session from mapping
            lock (_lock)
            {
                if (_connectionSessionMap.TryGetValue(connectionId, out var mappedSessionId))
                {
                    sessionId = mappedSessionId;
                    _connectionSessionMap.Remove(connectionId);
                }
            }

            if (sessionId.HasValue)
            {
                await Groups.RemoveFromGroupAsync(connectionId, GetSessionGroupName(sessionId.Value));

                _logger.LogInformation(
                    "[SessionHub] Connection {ConnectionId} disconnected from session {SessionId}",
                    connectionId, sessionId.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[SessionHub] Error handling disconnection for connection {ConnectionId}",
                connectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    #region Server-to-Client Broadcast Methods

    /// <summary>
    /// Broadcast when a question is activated
    /// </summary>
    public async Task BroadcastQuestionActivated(Guid sessionId, QuestionActivatedEvent eventData)
    {
        try
        {
            await Clients.Group(GetSessionGroupName(sessionId))
                .SendAsync("QuestionActivated", eventData);

            _logger.LogInformation(
                "[SessionHub] Broadcast QuestionActivated for session {SessionId}, question {QuestionId}",
                sessionId, eventData.QuestionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SessionHub] Error broadcasting QuestionActivated");
        }
    }

    /// <summary>
    /// Broadcast when a response is submitted
    /// </summary>
    public async Task BroadcastResponseSubmitted(Guid sessionId, ResponseSubmittedEvent eventData)
    {
        try
        {
            await Clients.Group(GetSessionGroupName(sessionId))
                .SendAsync("ResponseSubmitted", eventData);

            _logger.LogInformation(
                "[SessionHub] Broadcast ResponseSubmitted for session {SessionId}, participant {ParticipantId}",
                sessionId, eventData.ParticipantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SessionHub] Error broadcasting ResponseSubmitted");
        }
    }

    /// <summary>
    /// Broadcast leaderboard update
    /// </summary>
    public async Task BroadcastLeaderboardUpdate(Guid sessionId, LeaderboardUpdateEvent eventData)
    {
        try
        {
            await Clients.Group(GetSessionGroupName(sessionId))
                .SendAsync("LeaderboardUpdate", eventData);

            _logger.LogInformation(
                "[SessionHub] Broadcast LeaderboardUpdate for session {SessionId}",
                sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SessionHub] Error broadcasting LeaderboardUpdate");
        }
    }

    /// <summary>
    /// Broadcast session status change
    /// </summary>
    public async Task BroadcastSessionStatusChanged(Guid sessionId, SessionStatusChangedEvent eventData)
    {
        try
        {
            await Clients.Group(GetSessionGroupName(sessionId))
                .SendAsync("SessionStatusChanged", eventData);

            _logger.LogInformation(
                "[SessionHub] Broadcast SessionStatusChanged for session {SessionId}, new status: {Status}",
                sessionId, eventData.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SessionHub] Error broadcasting SessionStatusChanged");
        }
    }

    /// <summary>
    /// Broadcast participant joined
    /// </summary>
    public async Task BroadcastParticipantJoined(Guid sessionId, ParticipantJoinedEvent eventData)
    {
        try
        {
            await Clients.Group(GetSessionGroupName(sessionId))
                .SendAsync("ParticipantJoined", eventData);

            _logger.LogInformation(
                "[SessionHub] Broadcast ParticipantJoined for session {SessionId}, participant {ParticipantId}",
                sessionId, eventData.SessionParticipantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SessionHub] Error broadcasting ParticipantJoined");
        }
    }

    /// <summary>
    /// Broadcast participant left
    /// </summary>
    public async Task BroadcastParticipantLeft(Guid sessionId, ParticipantLeftEvent eventData)
    {
        try
        {
            await Clients.Group(GetSessionGroupName(sessionId))
                .SendAsync("ParticipantLeft", eventData);

            _logger.LogInformation(
                "[SessionHub] Broadcast ParticipantLeft for session {SessionId}, participant {ParticipantId}",
                sessionId, eventData.SessionParticipantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SessionHub] Error broadcasting ParticipantLeft");
        }
    }

    /// <summary>
    /// Broadcast time extended
    /// </summary>
    public async Task BroadcastTimeExtended(Guid sessionId, TimeExtendedEvent eventData)
    {
        try
        {
            await Clients.Group(GetSessionGroupName(sessionId))
                .SendAsync("TimeExtended", eventData);

            _logger.LogInformation(
                "[SessionHub] Broadcast TimeExtended for session {SessionId}, question {SessionQuestionId}",
                sessionId, eventData.SessionQuestionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SessionHub] Error broadcasting TimeExtended");
        }
    }

    #endregion

    #region Private Helpers

    private static string GetSessionGroupName(Guid sessionId) => $"session:{sessionId}";
    private static string GetMapStateCacheKey(Guid sessionId) => $"session:{sessionId}:map-state";

    #endregion
}

public class MapStateSyncRequest
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int ZoomLevel { get; set; }
    public decimal? Bearing { get; set; }
    public decimal? Pitch { get; set; }
}
