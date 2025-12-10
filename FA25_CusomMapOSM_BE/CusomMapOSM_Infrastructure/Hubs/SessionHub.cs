using CusomMapOSM_Application.Interfaces.Features.QuickPolls;
using CusomMapOSM_Application.Interfaces.Services.Cache;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.QuickPolls;
using CusomMapOSM_Application.Models.DTOs.Features.Sessions.Events;
using CusomMapOSM_Application.Models.DTOs.Features.QuickPolls.Request;
using CusomMapOSM_Application.Interfaces.Features.Sessions;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
namespace CusomMapOSM_Infrastructure.Hubs;

public class SessionHub : Hub
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ISessionParticipantRepository _participantRepository;
    private readonly ISessionQuestionRepository _sessionQuestionRepository;
    private readonly ISessionService _sessionService;
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
        ISessionQuestionRepository sessionQuestionRepository,
        ISessionService sessionService,
        ICurrentUserService currentUserService,
        IRedisCacheService redisCacheService,
        IQuickPollService quickPollService,
        ILogger<SessionHub> logger)
    {
        _sessionRepository = sessionRepository;
        _participantRepository = participantRepository;
        _sessionQuestionRepository = sessionQuestionRepository;
        _sessionService = sessionService;
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

            // Check if session has ended - block joining completed/cancelled sessions
            if (session.Status == CusomMapOSM_Domain.Entities.Sessions.Enums.SessionStatusEnum.COMPLETED ||
                session.Status == CusomMapOSM_Domain.Entities.Sessions.Enums.SessionStatusEnum.CANCELLED)
            {
                await Clients.Caller.SendAsync("SessionEnded", new
                {
                    SessionId = sessionId,
                    Status = session.Status.ToString(),
                    Message = "This session has ended and is no longer accessible",
                    EndedAt = session.EndTime ?? DateTime.UtcNow
                });
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
            var segmentState = await _redisCacheService.Get<SegmentSyncEvent>(GetSegmentStateCacheKey(sessionId));
            var mapLockState = await _redisCacheService.Get<MapLockStateSyncEvent>(GetMapLockStateCacheKey(sessionId));

            await Clients.Caller.SendAsync("JoinedSession", new
            {
                SessionId = sessionId,
                SessionCode = session.SessionCode,
                Status = session.Status.ToString(),
                Message = $"Successfully joined session {session.SessionCode}",
                MapState = mapState,
                SegmentState = segmentState,
                MapLockState = mapLockState
            });

            if (mapState != null)
            {
                await Clients.Caller.SendAsync("MapStateSync", mapState);
            }
            
            if (segmentState != null)
            {
                await Clients.Caller.SendAsync("SegmentSync", segmentState);
            }

            if (mapLockState != null)
            {
                await Clients.Caller.SendAsync("MapLockStateSync", mapLockState);
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

    /// <summary>
    /// Teacher syncs current segment (for storymap playback)
    /// </summary>
    public async Task SyncSegment(Guid sessionId, SegmentSyncRequest request)
    {
        try
        {
            var userId = _currentUserService.GetUserId();

            // Validate user is the session host
            var session = await _sessionRepository.GetSessionById(sessionId);
            if (session == null || session.HostUserId != userId)
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Only host can sync segment" });
                return;
            }

            var syncEvent = new SegmentSyncEvent
            {
                SessionId = sessionId,
                SegmentIndex = request.SegmentIndex,
                SegmentId = request.SegmentId,
                SegmentName = request.SegmentName,
                IsPlaying = request.IsPlaying,
                SyncedAt = DateTime.UtcNow
            };

            // Cache current segment state
            var cacheKey = GetSegmentStateCacheKey(sessionId);
            await _redisCacheService.Set(cacheKey, syncEvent, MapStateCacheTtl);

            // Broadcast to all participants except the caller
            await Clients.OthersInGroup(GetSessionGroupName(sessionId))
                .SendAsync("SegmentSync", syncEvent);

            _logger.LogInformation(
                "[SessionHub] Host synced segment {SegmentIndex} for session {SessionId}",
                request.SegmentIndex, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SessionHub] Error syncing segment for session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to sync segment" });
        }
    }

    /// <summary>
    /// Teacher syncs map lock state (for locking/unlocking student map interaction)
    /// </summary>
    public async Task SyncMapLockState(Guid sessionId, MapLockStateSyncRequest request)
    {
        try
        {
            var userId = _currentUserService.GetUserId();

            // Validate user is the session host
            var session = await _sessionRepository.GetSessionById(sessionId);
            if (session == null || session.HostUserId != userId)
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Only host can sync map lock state" });
                return;
            }

            var syncEvent = new MapLockStateSyncEvent
            {
                SessionId = sessionId,
                IsLocked = request.IsLocked,
                SyncedAt = DateTime.UtcNow
            };

            // Cache current map lock state
            var cacheKey = GetMapLockStateCacheKey(sessionId);
            await _redisCacheService.Set(cacheKey, syncEvent, MapStateCacheTtl);

            // Broadcast to all participants except the caller
            await Clients.OthersInGroup(GetSessionGroupName(sessionId))
                .SendAsync("MapLockStateSync", syncEvent);

            _logger.LogInformation(
                "[SessionHub] Host synced map lock state {IsLocked} for session {SessionId}",
                request.IsLocked, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SessionHub] Error syncing map lock state for session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to sync map lock state" });
        }
    }

    /// <summary>
    /// Teacher syncs map layer (base map provider)
    /// </summary>
    public async Task SyncMapLayer(Guid sessionId, string layerKey)
    {
        try
        {
            var userId = _currentUserService.GetUserId();

            // Validate user is the session host
            var session = await _sessionRepository.GetSessionById(sessionId);
            if (session == null || session.HostUserId != userId)
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Only host can sync map layer" });
                return;
            }

            var syncEvent = new MapLayerSyncEvent
            {
                SessionId = sessionId,
                LayerKey = layerKey,
                SyncedAt = DateTime.UtcNow
            };

            // Cache current map layer
            var cacheKey = GetMapLayerCacheKey(sessionId);
            await _redisCacheService.Set(cacheKey, syncEvent, MapStateCacheTtl);

            // Broadcast to all participants except the caller
            await Clients.OthersInGroup(GetSessionGroupName(sessionId))
                .SendAsync("MapLayerSync", syncEvent);

            _logger.LogInformation(
                "[SessionHub] Host synced map layer {LayerKey} for session {SessionId}",
                layerKey, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SessionHub] Error syncing map layer for session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to sync map layer" });
        }
    }

    /// <summary>
    /// Teacher broadcasts a question to all students
    /// </summary>
    public async Task BroadcastQuestionToStudents(Guid sessionId, QuestionBroadcastRequest request)
    {
        try
        {
            var userId = _currentUserService.GetUserId();

            // Validate user is the session host
            var session = await _sessionRepository.GetSessionById(sessionId);
            if (session == null || session.HostUserId != userId)
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Only host can broadcast question" });
                return;
            }

            var resolvedSessionQuestionId =
                await _sessionService.ResolveAndActivateSessionQuestion(sessionId, request.QuestionId);

            var broadcastEvent = new QuestionBroadcastEvent
            {
                SessionQuestionId = resolvedSessionQuestionId ?? request.SessionQuestionId,
                SessionId = sessionId,
                QuestionId = request.QuestionId,
                QuestionText = request.QuestionText,
                QuestionType = request.QuestionType,
                QuestionImageUrl = request.QuestionImageUrl,
                Options = request.Options,
                Points = request.Points,
                TimeLimit = request.TimeLimit,
                BroadcastedAt = DateTime.UtcNow
            };

            // Broadcast to all participants
            await Clients.Group(GetSessionGroupName(sessionId))
                .SendAsync("QuestionBroadcast", broadcastEvent);

            _logger.LogInformation(
                "[SessionHub] Host broadcasted question {QuestionId} for session {SessionId}",
                request.QuestionId, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SessionHub] Error broadcasting question for session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to broadcast question" });
        }
    }

    /// <summary>
    /// Teacher shows question results to all students
    /// </summary>
    public async Task ShowQuestionResults(Guid sessionId, QuestionResultsRequest request)
    {
        try
        {
            var userId = _currentUserService.GetUserId();

            // Validate user is the session host
            var session = await _sessionRepository.GetSessionById(sessionId);
            if (session == null || session.HostUserId != userId)
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Only host can show results" });
                return;
            }

            var resultsEvent = new QuestionResultsEvent
            {
                SessionId = sessionId,
                QuestionId = request.QuestionId,
                Results = request.Results,
                CorrectAnswer = request.CorrectAnswer,
                ShowedAt = DateTime.UtcNow
            };

            // Broadcast to all participants
            await Clients.Group(GetSessionGroupName(sessionId))
                .SendAsync("QuestionResults", resultsEvent);

            _logger.LogInformation(
                "[SessionHub] Host showed results for question {QuestionId} in session {SessionId}",
                request.QuestionId, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SessionHub] Error showing results for session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to show results" });
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

    public async Task BroadcastQuestionResponsesUpdate(Guid sessionId, QuestionResponsesUpdateEvent eventData)
    {
        try
        {
            await Clients.Group(GetSessionGroupName(sessionId))
                .SendAsync("QuestionResponsesUpdate", eventData);

            _logger.LogInformation(
                "[SessionHub] Broadcast QuestionResponsesUpdate for session {SessionId}, question {SessionQuestionId}, total responses: {TotalResponses}",
                sessionId, eventData.SessionQuestionId, eventData.TotalResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SessionHub] Error broadcasting QuestionResponsesUpdate");
        }
    }

    #endregion

    #region Private Helpers

    private static string GetSessionGroupName(Guid sessionId) => $"session:{sessionId}";
    private static string GetMapStateCacheKey(Guid sessionId) => $"session:{sessionId}:map-state";
    private static string GetSegmentStateCacheKey(Guid sessionId) => $"session:{sessionId}:segment-state";
    private static string GetMapLockStateCacheKey(Guid sessionId) => $"session:{sessionId}:map-lock-state";
    private static string GetMapLayerCacheKey(Guid sessionId) => $"session:{sessionId}:map-layer";

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

public class SegmentSyncRequest
{
    public int SegmentIndex { get; set; }
    public string? SegmentId { get; set; }
    public string? SegmentName { get; set; }
    public bool IsPlaying { get; set; }
}

public class SegmentSyncEvent
{
    public Guid SessionId { get; set; }
    public int SegmentIndex { get; set; }
    public string? SegmentId { get; set; }
    public string? SegmentName { get; set; }
    public bool IsPlaying { get; set; }
    public DateTime SyncedAt { get; set; }
}

public class MapLayerSyncEvent
{
    public Guid SessionId { get; set; }
    public string LayerKey { get; set; } = string.Empty;
    public DateTime SyncedAt { get; set; }
}

public class QuestionBroadcastRequest
{
    public Guid SessionQuestionId { get; set; }
    public string QuestionId { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public string? QuestionImageUrl { get; set; }
    public List<QuestionOptionData>? Options { get; set; }
    public int Points { get; set; }
    public int TimeLimit { get; set; }
}

public class QuestionOptionData
{
    public string Id { get; set; } = string.Empty;
    public string OptionText { get; set; } = string.Empty;
    public string? OptionImageUrl { get; set; }
    public int DisplayOrder { get; set; }
}

public class QuestionBroadcastEvent
{
    public Guid SessionQuestionId { get; set; }
    public Guid SessionId { get; set; }
    public string QuestionId { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public string? QuestionImageUrl { get; set; }
    public List<QuestionOptionData>? Options { get; set; }
    public int Points { get; set; }
    public int TimeLimit { get; set; }
    public DateTime BroadcastedAt { get; set; }
}

public class QuestionResultsRequest
{
    public string QuestionId { get; set; } = string.Empty;
    public List<StudentResultData>? Results { get; set; }
    public string? CorrectAnswer { get; set; }
}

public class StudentResultData
{
    public string ParticipantId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Answer { get; set; }
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }
}

public class QuestionResultsEvent
{
    public Guid SessionId { get; set; }
    public string QuestionId { get; set; } = string.Empty;
    public List<StudentResultData>? Results { get; set; }
    public string? CorrectAnswer { get; set; }
    public DateTime ShowedAt { get; set; }
}

public class MapLockStateSyncRequest
{
    public bool IsLocked { get; set; }
}

public class MapLockStateSyncEvent
{
    public Guid SessionId { get; set; }
    public bool IsLocked { get; set; }
    public DateTime SyncedAt { get; set; }
}
