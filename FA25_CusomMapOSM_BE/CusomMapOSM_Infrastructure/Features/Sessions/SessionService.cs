using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Sessions;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Sessions.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Sessions.Response;
using CusomMapOSM_Application.Models.DTOs.Features.Sessions.Events;
using CusomMapOSM_Domain.Entities.QuestionBanks;
using CusomMapOSM_Domain.Entities.QuestionBanks.Enums;
using CusomMapOSM_Domain.Entities.Sessions;
using CusomMapOSM_Domain.Entities.Sessions.Enums;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.QuestionBanks;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using CusomMapOSM_Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Optional;
using ResponseLeaderboardEntry = CusomMapOSM_Application.Models.DTOs.Features.Sessions.Response.LeaderboardEntry;
using EventLeaderboardEntry = CusomMapOSM_Application.Models.DTOs.Features.Sessions.Events.LeaderboardEntry;

namespace CusomMapOSM_Infrastructure.Features.Sessions;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ISessionParticipantRepository _participantRepository;
    private readonly ISessionQuestionRepository _sessionQuestionRepository;
    private readonly IStudentResponseRepository _responseRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly ISessionQuestionBankRepository _sessionQuestionBankRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHubContext<SessionHub> _sessionHubContext;
    private readonly IQuestionOptionRepository _questionOptionRepository;
    private readonly IMapRepository _mapRepository;
    private readonly IQuestionBankRepository _questionBankRepository;

    public SessionService(
        ISessionRepository sessionRepository,
        ISessionParticipantRepository participantRepository,
        ISessionQuestionRepository sessionQuestionRepository,
        IStudentResponseRepository responseRepository,
        IQuestionRepository questionRepository,
        IQuestionOptionRepository questionOptionRepository,
        ISessionQuestionBankRepository sessionQuestionBankRepository,
        ICurrentUserService currentUserService,
        IHubContext<SessionHub> sessionHubContext,
        IMapRepository mapRepository,
        IQuestionBankRepository questionBankRepository)
    {
        _sessionRepository = sessionRepository;
        _participantRepository = participantRepository;
        _sessionQuestionRepository = sessionQuestionRepository;
        _responseRepository = responseRepository;
        _questionRepository = questionRepository;
        _questionOptionRepository = questionOptionRepository;
        _sessionQuestionBankRepository = sessionQuestionBankRepository;
        _currentUserService = currentUserService;
        _sessionHubContext = sessionHubContext;
        _mapRepository = mapRepository;
        _questionBankRepository = questionBankRepository;
    }

    public async Task<Option<CreateSessionResponse, Error>> CreateSession(CreateSessionRequest request)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<CreateSessionResponse, Error>(
                Error.Unauthorized("Session.Unauthorized", "User not authenticated"));
        }

        var map = await _mapRepository.GetMapById(request.MapId);
        if (map == null || !map.IsActive)
        {
            return Option.None<CreateSessionResponse, Error>(
                Error.NotFound("Map.NotFound", "Map not found or has been deleted"));
        }

        // Check if map is published as storymap (can create sessions)
        if (!map.IsStoryMap || map.Status != MapStatusEnum.Published)
        {
            return Option.None<CreateSessionResponse, Error>(
                Error.ValidationError("Map.NotStoryMap", 
                    "Chỉ có thể tạo session từ bản đồ đã được publish thành storymap. Vui lòng publish bản đồ với loại storymap để có thể tạo session."));
        }

        // Validate all question banks exist and are active
        foreach (var questionBankId in request.QuestionBankId)
        {
            var questionBank = await _questionBankRepository.GetQuestionBankById(questionBankId);
            if (questionBank == null || !questionBank.IsActive)
            {
                return Option.None<CreateSessionResponse, Error>(
                    Error.NotFound("QuestionBank.NotFound", 
                        $"Question bank {questionBankId} not found or has been deleted"));
            }
        }

        // Generate unique session code
        var sessionCode = await _sessionRepository.GenerateUniqueSessionCode();

        // Create session
        var session = new Session
        {
            SessionId = Guid.NewGuid(),
            MapId = request.MapId,
            HostUserId = currentUserId.Value,
            SessionCode = sessionCode,
            SessionName = request.SessionName,
            Description = request.Description,
            SessionType = request.SessionType,
            Status = SessionStatusEnum.WAITING,
            MaxParticipants = request.MaxParticipants,
            AllowLateJoin = request.AllowLateJoin,
            ShowLeaderboard = request.ShowLeaderboard,
            ShowCorrectAnswers = request.ShowCorrectAnswers,
            ShuffleQuestions = request.ShuffleQuestions,
            ShuffleOptions = request.ShuffleOptions,
            EnableHints = request.EnableHints,
            PointsForSpeed = request.PointsForSpeed,
            ScheduledStartTime = request.ScheduledStartTime,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _sessionRepository.CreateSession(session);
        if (!created)
        {
            return Option.None<CreateSessionResponse, Error>(
                Error.Failure("Session.CreateFailed", "Failed to create session"));
        }

        foreach (var questionBankId in request.QuestionBankId)
        {
            var sessionQuestionBank = new SessionQuestionBank
            {
                SessionQuestionBankId = Guid.NewGuid(),
                SessionId = session.SessionId,
                QuestionBankId = questionBankId,
                AttachedAt = DateTime.UtcNow
            };
            await _sessionQuestionBankRepository.AddQuestionBank(sessionQuestionBank);
        }

        int globalQueueOrder = 0;
        foreach (var questionBankId in request.QuestionBankId)
        {
            var questions = await _questionRepository.GetQuestionsByQuestionBankId(questionBankId);
            if (request.ShuffleQuestions)
            {
                questions = questions.OrderBy(_ => Guid.NewGuid()).ToList();
            }

            foreach (var question in questions)
            {
                globalQueueOrder++;
                var sessionQuestion = new SessionQuestion
                {
                    SessionQuestionId = Guid.NewGuid(),
                    SessionId = session.SessionId,
                    QuestionId = question.QuestionId,
                    QueueOrder = globalQueueOrder,
                    Status = SessionQuestionStatusEnum.QUEUED,
                    CreatedAt = DateTime.UtcNow
                };
                await _sessionQuestionRepository.CreateSessionQuestion(sessionQuestion);
            }
        }

        return Option.Some<CreateSessionResponse, Error>(new CreateSessionResponse
        {
            SessionId = session.SessionId,
            SessionCode = session.SessionCode, 
            SessionName = session.SessionName, 
            Message = "Session created successfully", 
            CreatedAt = session.CreatedAt
        });
    }
    public async Task<Option<GetSessionResponse, Error>> GetSessionById(Guid sessionId)
    {
        var session = await _sessionRepository.GetSessionById(sessionId);
        if (session == null)
        {
            return Option.None<GetSessionResponse, Error>(
                Error.NotFound("Session.NotFound", "Session not found"));
        }

        // Get all question banks attached to this session
        var sessionQuestionBanks = await _sessionQuestionBankRepository.GetQuestionBanks(session.SessionId);
        var questionBanks = sessionQuestionBanks
            .Where(sqb => sqb.QuestionBank != null)
            .Select(sqb => new QuestionBankInfo
            {
                QuestionBankId = sqb.QuestionBank!.QuestionBankId,
                QuestionBankName = sqb.QuestionBank.BankName,
                Description = sqb.QuestionBank.Description,
                Category = sqb.QuestionBank.Category,
                TotalQuestions = sqb.QuestionBank.TotalQuestions,
                AttachedAt = sqb.AttachedAt
            })
            .ToList();

        return Option.Some<GetSessionResponse, Error>(new GetSessionResponse
        {
            SessionId = session.SessionId,
            SessionCode = session.SessionCode,
            SessionName = session.SessionName,
            Description = session.Description,
            SessionType = session.SessionType,
            Status = session.Status,
            MapId = session.MapId,
            MapName = session.Map?.MapName ?? string.Empty,
            QuestionBanks = questionBanks,
            HostUserId = session.HostUserId,
            HostUserName = session.HostUser?.FullName ?? string.Empty,
            MaxParticipants = session.MaxParticipants,
            TotalParticipants = session.TotalParticipants,
            TotalResponses = session.TotalResponses,
            AllowLateJoin = session.AllowLateJoin,
            ShowLeaderboard = session.ShowLeaderboard,
            ShowCorrectAnswers = session.ShowCorrectAnswers,
            ScheduledStartTime = session.ScheduledStartTime,
            ActualStartTime = session.ActualStartTime,
            EndTime = session.EndTime,
            CreatedAt = session.CreatedAt
        });
    }

    public async Task<Option<GetSessionResponse, Error>> GetSessionByCode(string sessionCode)
    {
        var session = await _sessionRepository.GetSessionByCode(sessionCode);
        if (session == null)
        {
            return Option.None<GetSessionResponse, Error>(
                Error.NotFound("Session.NotFound", "Session not found"));
        }

        return await GetSessionById(session.SessionId);
    }

    public async Task<Option<List<GetSessionResponse>, Error>> GetMySessionsAsHost(Guid? organizationId = null,
        string? sortBy = null, string? order = null)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<List<GetSessionResponse>, Error>(
                Error.Unauthorized("Session.Unauthorized", "User not authenticated"));
        }

        List<Session> sessions;

        // If organizationId provided, use repository method to ensure Workspace is included and filtering happens in the DB
        if (organizationId.HasValue)
        {
            sessions = await _sessionRepository.GetSessionsByHostUserIdAndOrganizationId(currentUserId.Value, organizationId.Value);
        }
        else
        {
            sessions = await _sessionRepository.GetSessionsByHostUserId(currentUserId.Value);
        }

        // Apply sorting
        sessions = ApplySorting(sessions, sortBy, order);

        var response = await MapSessionsToResponses(sessions);
        return Option.Some<List<GetSessionResponse>, Error>(response);
    }

    public async Task<Option<List<GetSessionResponse>, Error>> GetAllSessionsByOrganization(Guid organizationId,
        string? sortBy = null, string? order = null, Guid? hostId = null, string? status = null, bool? hasQuestionBanks = null)
    {
        var sessions = await _sessionRepository.GetSessionsByOrganizationId(organizationId);

        // Filter by host if provided
        if (hostId.HasValue)
        {
            sessions = sessions.Where(s => s.HostUserId == hostId.Value).ToList();
        }

        // Filter by status if provided
        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<SessionStatusEnum>(status, true, out var statusEnum))
            {
                sessions = sessions.Where(s => s.Status == statusEnum).ToList();
            }
        }

        // Filter by question banks existence if provided
        if (hasQuestionBanks.HasValue)
        {
            var sessionIds = sessions.Select(s => s.SessionId).ToList();
            var sessionsWithQB = await _sessionQuestionBankRepository.GetSessionsWithQuestionBanks(sessionIds);
            
            if (hasQuestionBanks.Value)
            {
                sessions = sessions.Where(s => sessionsWithQB.Contains(s.SessionId)).ToList();
            }
            else
            {
                sessions = sessions.Where(s => !sessionsWithQB.Contains(s.SessionId)).ToList();
            }
        }

        // Apply sorting
        sessions = ApplySorting(sessions, sortBy, order);

        var response = await MapSessionsToResponses(sessions);
        return Option.Some<List<GetSessionResponse>, Error>(response);
    }

    private List<Session> ApplySorting(List<Session> sessions, string? sortBy, string? order)
    {
        var isDescending = order?.ToLower() == "desc";

        return (sortBy?.ToLower()) switch
        {
            "name" => isDescending
                ? sessions.OrderByDescending(s => s.SessionName).ToList()
                : sessions.OrderBy(s => s.SessionName).ToList(),
            "status" => isDescending
                ? sessions.OrderByDescending(s => s.Status).ToList()
                : sessions.OrderBy(s => s.Status).ToList(),
            "createdat" or null => isDescending
                ? sessions.OrderByDescending(s => s.CreatedAt).ToList()
                : sessions.OrderBy(s => s.CreatedAt).ToList(),
            _ => sessions
        };
    }

    private async Task<List<GetSessionResponse>> MapSessionsToResponses(List<Session> sessions)
    {
        var response = new List<GetSessionResponse>();
        foreach (var s in sessions)
        {
            var sessionQuestionBanks = await _sessionQuestionBankRepository.GetQuestionBanks(s.SessionId);
            var questionBanks = sessionQuestionBanks
                .Where(sqb => sqb.QuestionBank != null)
                .Select(sqb => new QuestionBankInfo
                {
                    QuestionBankId = sqb.QuestionBank!.QuestionBankId,
                    QuestionBankName = sqb.QuestionBank.BankName,
                    Description = sqb.QuestionBank.Description,
                    Category = sqb.QuestionBank.Category,
                    TotalQuestions = sqb.QuestionBank.TotalQuestions,
                    AttachedAt = sqb.AttachedAt
                })
                .ToList();

            response.Add(new GetSessionResponse
            {
                SessionId = s.SessionId,
                SessionCode = s.SessionCode,
                SessionName = s.SessionName,
                Description = s.Description,
                SessionType = s.SessionType,
                Status = s.Status,
                MapId = s.MapId,
                MapName = s.Map?.MapName ?? string.Empty,
                QuestionBanks = questionBanks,
                HostUserId = s.HostUserId,
                HostUserName = s.HostUser?.FullName ?? string.Empty,
                TotalParticipants = s.TotalParticipants,
                TotalResponses = s.TotalResponses,
                CreatedAt = s.CreatedAt
            });
        }

        return response;
    }

    public async Task<Option<bool, Error>> DeleteSession(Guid sessionId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<bool, Error>(
                Error.Unauthorized("Session.Unauthorized", "User not authenticated"));
        }

        var isHost = await _sessionRepository.CheckUserIsHost(sessionId, currentUserId.Value);
        if (!isHost)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("Session.NotHost", "Only the host can delete the session"));
        }

        var deleted = await _sessionRepository.DeleteSession(sessionId);
        return deleted
            ? Option.Some<bool, Error>(true)
            : Option.None<bool, Error>(Error.Failure("Session.DeleteFailed", "Failed to delete session"));
    }

    public async Task<Option<bool, Error>> StartSession(Guid sessionId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<bool, Error>(
                Error.Unauthorized("Session.Unauthorized", "User not authenticated"));
        }

        var isHost = await _sessionRepository.CheckUserIsHost(sessionId, currentUserId.Value);
        if (!isHost)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("Session.NotHost", "Only the host can start the session"));
        }

        var started = await _sessionRepository.StartSession(sessionId);
        if (!started)
        {
            return Option.None<bool, Error>(Error.Failure("Session.StartFailed", "Failed to start session"));
        }

        // Broadcast session status change
        await _sessionHubContext.Clients.Group($"session:{sessionId}")
            .SendAsync("SessionStatusChanged", new SessionStatusChangedEvent
            {
                SessionId = sessionId,
                Status = SessionStatusEnum.IN_PROGRESS.ToString(),
                Message = "Session has started",
                ChangedAt = DateTime.UtcNow
            });

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<bool, Error>> PauseSession(Guid sessionId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<bool, Error>(
                Error.Unauthorized("Session.Unauthorized", "User not authenticated"));
        }

        var isHost = await _sessionRepository.CheckUserIsHost(sessionId, currentUserId.Value);
        if (!isHost)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("Session.NotHost", "Only the host can pause the session"));
        }

        var paused = await _sessionRepository.PauseSession(sessionId);
        if (!paused)
        {
            return Option.None<bool, Error>(Error.Failure("Session.PauseFailed", "Failed to pause session"));
        }

        // Broadcast session status change
        await _sessionHubContext.Clients.Group($"session:{sessionId}")
            .SendAsync("SessionStatusChanged", new SessionStatusChangedEvent
            {
                SessionId = sessionId,
                Status = SessionStatusEnum.PAUSED.ToString(),
                Message = "Session has been paused",
                ChangedAt = DateTime.UtcNow
            });

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<bool, Error>> ResumeSession(Guid sessionId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<bool, Error>(
                Error.Unauthorized("Session.Unauthorized", "User not authenticated"));
        }

        var isHost = await _sessionRepository.CheckUserIsHost(sessionId, currentUserId.Value);
        if (!isHost)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("Session.NotHost", "Only the host can resume the session"));
        }

        var resumed = await _sessionRepository.ResumeSession(sessionId);
        if (!resumed)
        {
            return Option.None<bool, Error>(Error.Failure("Session.ResumeFailed", "Failed to resume session"));
        }

        // Broadcast session status change
        await _sessionHubContext.Clients.Group($"session:{sessionId}")
            .SendAsync("SessionStatusChanged", new SessionStatusChangedEvent
            {
                SessionId = sessionId,
                Status = SessionStatusEnum.IN_PROGRESS.ToString(),
                Message = "Session has resumed",
                ChangedAt = DateTime.UtcNow
            });

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<bool, Error>> EndSession(Guid sessionId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<bool, Error>(
                Error.Unauthorized("Session.Unauthorized", "User not authenticated"));
        }

        var isHost = await _sessionRepository.CheckUserIsHost(sessionId, currentUserId.Value);
        if (!isHost)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("Session.NotHost", "Only the host can end the session"));
        }

        var ended = await _sessionRepository.EndSession(sessionId);
        if (!ended)
        {
            return Option.None<bool, Error>(
                Error.Failure("Session.EndFailed", "Failed to end session"));
        }

        // Mark all participants as left
        await _participantRepository.MarkAllParticipantsAsLeft(sessionId);

        // Broadcast session status change
        await _sessionHubContext.Clients.Group($"session:{sessionId}")
            .SendAsync("SessionStatusChanged", new SessionStatusChangedEvent
            {
                SessionId = sessionId,
                Status = SessionStatusEnum.COMPLETED.ToString(),
                Message = "Session has ended",
                ChangedAt = DateTime.UtcNow
            });

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<JoinSessionResponse, Error>> JoinSession(JoinSessionRequest request)
    {
        var session = await _sessionRepository.GetSessionByCode(request.SessionCode);
        if (session == null)
        {
            return Option.None<JoinSessionResponse, Error>(
                Error.NotFound("Session.NotFound", "Session not found with this code"));
        }

        // Check if session allows joining
        if (session.Status != SessionStatusEnum.WAITING && session.Status != SessionStatusEnum.IN_PROGRESS)
        {
            return Option.None<JoinSessionResponse, Error>(
                Error.ValidationError("Session.NotJoinable", "Session is not accepting participants"));
        }

        if (!session.AllowLateJoin && session.Status == SessionStatusEnum.IN_PROGRESS)
        {
            return Option.None<JoinSessionResponse, Error>(
                Error.ValidationError("Session.LateJoinDisabled", "Late join is not allowed for this session"));
        }

        // Check max participants - get fresh count to avoid race conditions
        if (session.MaxParticipants > 0)
        {
            var currentParticipantCount = await _participantRepository.GetActiveParticipantCount(session.SessionId);
            if (currentParticipantCount >= session.MaxParticipants)
            {
                return Option.None<JoinSessionResponse, Error>(
                    Error.ValidationError("Session.Full", $"Phiên đã đạt số lượng người tham gia tối đa ({session.MaxParticipants} người)"));
            }
        }

        var currentUserId = _currentUserService.GetUserId();

        string participantKey;
        if (currentUserId != null)
        {
            participantKey = GenerateParticipantKeyForUser(currentUserId.Value, session.SessionId);
            var existingParticipant =
                await _participantRepository.GetParticipantBySessionAndParticipantKey(session.SessionId, participantKey);
            if (existingParticipant != null)
            {
                // Allow rejoin - reactivate participant if inactive and return existing info
                if (!existingParticipant.IsActive)
                {
                    existingParticipant.IsActive = true;
                    existingParticipant.JoinedAt = DateTime.UtcNow;
                    await _participantRepository.UpdateParticipant(existingParticipant);
                    await _sessionRepository.UpdateParticipantCount(session.SessionId);
                }
                
                // Return existing participant info for rejoin
                return Option.Some<JoinSessionResponse, Error>(new JoinSessionResponse
                {
                    SessionParticipantId = existingParticipant.SessionParticipantId,
                    SessionId = session.SessionId,
                    SessionName = session.SessionName,
                    DisplayName = existingParticipant.DisplayName,
                    Message = "Rejoined session successfully",
                    JoinedAt = existingParticipant.JoinedAt
                });
            }
        }
        else
        {
            participantKey = GenerateGuestParticipantKey();
        }

        var participant = new SessionParticipant
        {
            SessionParticipantId = Guid.NewGuid(),
            SessionId = session.SessionId,
            DisplayName = request.DisplayName,
            ParticipantKey = participantKey,
            IsGuest = currentUserId == null,
            DeviceInfo = request.DeviceInfo,
            JoinedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _participantRepository.CreateParticipant(participant);
        if (!created)
        {
            return Option.None<JoinSessionResponse, Error>(
                Error.Failure("Session.JoinFailed", "Failed to join session"));
        }

        // Update session participant count
        await _sessionRepository.UpdateParticipantCount(session.SessionId);

        // Get updated participant count
        var updatedSession = await _sessionRepository.GetSessionById(session.SessionId);
        var totalParticipants = updatedSession?.TotalParticipants ?? 1;

        // Broadcast participant joined event
        await _sessionHubContext.Clients.Group($"session:{session.SessionId}")
            .SendAsync("ParticipantJoined", new ParticipantJoinedEvent
            {
                SessionParticipantId = participant.SessionParticipantId,
                DisplayName = participant.DisplayName,
                IsGuest = participant.IsGuest,
                TotalParticipants = totalParticipants,
                JoinedAt = participant.JoinedAt
            });

        return Option.Some<JoinSessionResponse, Error>(new JoinSessionResponse
        {
            SessionParticipantId = participant.SessionParticipantId,
            SessionId = session.SessionId,
            SessionName = session.SessionName,
            DisplayName = participant.DisplayName,
            Message = "Joined session successfully",
            JoinedAt = participant.JoinedAt
        });
    }

    public async Task<Option<bool, Error>> LeaveSession(Guid sessionParticipantId)
    {
        var participant = await _participantRepository.GetParticipantById(sessionParticipantId);
        if (participant == null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Participant.NotFound", "Participant not found"));
        }

        var left = await _participantRepository.MarkParticipantAsLeft(sessionParticipantId);
        if (!left)
        {
            return Option.None<bool, Error>(
                Error.Failure("Session.LeaveFailed", "Failed to leave session"));
        }

        // Update session participant count
        await _sessionRepository.UpdateParticipantCount(participant.SessionId);

        // Get updated participant count
        var updatedSession = await _sessionRepository.GetSessionById(participant.SessionId);
        var totalParticipants = updatedSession?.TotalParticipants ?? 0;

        // Broadcast participant left event
        await _sessionHubContext.Clients.Group($"session:{participant.SessionId}")
            .SendAsync("ParticipantLeft", new ParticipantLeftEvent
            {
                SessionParticipantId = participant.SessionParticipantId,
                DisplayName = participant.DisplayName,
                TotalParticipants = totalParticipants,
                LeftAt = DateTime.UtcNow
            });

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<LeaderboardResponse, Error>> GetLeaderboard(Guid sessionId, int limit = 10)
    {
        var session = await _sessionRepository.GetSessionById(sessionId);
        if (session == null)
        {
            return Option.None<LeaderboardResponse, Error>(
                Error.NotFound("Session.NotFound", "Session not found"));
        }

        var participants = await _participantRepository.GetLeaderboard(sessionId, limit);
        var currentUserId = _currentUserService.GetUserId();

        // Find current user's participant in this session if authenticated
        string? currentUserParticipantKey = null;
        if (currentUserId != null)
        {
            // Generate the same key used when joining
            currentUserParticipantKey = GenerateParticipantKeyForUser(currentUserId.Value, sessionId);
        }

        var leaderboard = participants.Select((p, index) => new ResponseLeaderboardEntry
        {
            Rank = index + 1,
            SessionParticipantId = p.SessionParticipantId,
            DisplayName = p.DisplayName,
            TotalScore = p.TotalScore,
            TotalCorrect = p.TotalCorrect,
            TotalAnswered = p.TotalAnswered,
            AverageResponseTime = p.AverageResponseTime,
            // Mark as current user if ParticipantKey matches
            IsCurrentUser = currentUserParticipantKey != null && p.ParticipantKey == currentUserParticipantKey
        }).ToList();

        return Option.Some<LeaderboardResponse, Error>(new LeaderboardResponse
        {
            SessionId = sessionId,
            SessionName = session.SessionName,
            Leaderboard = leaderboard,
            UpdatedAt = DateTime.UtcNow
        });
    }

    public async Task<Option<bool, Error>> ActivateNextQuestion(Guid sessionId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<bool, Error>(
                Error.Unauthorized("Session.Unauthorized", "User not authenticated"));
        }

        var isHost = await _sessionRepository.CheckUserIsHost(sessionId, currentUserId.Value);
        if (!isHost)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("Session.NotHost", "Only the host can control questions"));
        }

        // Complete current active question if exists
        var activeQuestion = await _sessionQuestionRepository.GetActiveQuestion(sessionId);
        if (activeQuestion != null)
        {
            await _sessionQuestionRepository.CompleteQuestion(activeQuestion.SessionQuestionId);
        }

        // Get next queued question
        var nextQuestion = await _sessionQuestionRepository.GetNextQueuedQuestion(sessionId);
        if (nextQuestion == null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Session.NoMoreQuestions", "No more questions in queue"));
        }

        // Activate next question
        var activated = await _sessionQuestionRepository.ActivateQuestion(nextQuestion.SessionQuestionId);
        if (!activated)
        {
            return Option.None<bool, Error>(Error.Failure("Session.ActivateFailed", "Failed to activate question"));
        }

        // Get full question details for broadcasting
        var activatedQuestion = await _sessionQuestionRepository.GetSessionQuestionById(nextQuestion.SessionQuestionId);
        if (activatedQuestion?.Question != null)
        {
            var question = activatedQuestion.Question;
            var optionEntities = await _questionOptionRepository.GetQuestionOptionsByQuestionId(question.QuestionId);
            var totalQuestions = await _sessionQuestionRepository.GetTotalQuestionsInSession(sessionId);

            // Broadcast question activated event
            await _sessionHubContext.Clients.Group($"session:{sessionId}")
                .SendAsync("QuestionActivated", new QuestionActivatedEvent
                {
                    SessionQuestionId = activatedQuestion.SessionQuestionId,
                    QuestionId = question.QuestionId,
                    QuestionText = question.QuestionText,
                    QuestionType = question.QuestionType.ToString(),
                    Points = activatedQuestion.PointsOverride ?? question.Points,
                    TimeLimit = activatedQuestion.TimeLimitOverride ?? question.TimeLimit,
                    QuestionNumber = activatedQuestion.QueueOrder,
                    TotalQuestions = totalQuestions,
                    Options = optionEntities.Select(o => new QuestionOptionDto
                    {
                        QuestionOptionId = o.QuestionOptionId,
                        OptionText = o.OptionText
                    }).ToList(),
                    ActivatedAt = DateTime.UtcNow
                });
        }

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<bool, Error>> SkipCurrentQuestion(Guid sessionId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<bool, Error>(
                Error.Unauthorized("Session.Unauthorized", "User not authenticated"));
        }

        var isHost = await _sessionRepository.CheckUserIsHost(sessionId, currentUserId.Value);
        if (!isHost)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("Session.NotHost", "Only the host can skip questions"));
        }

        var activeQuestion = await _sessionQuestionRepository.GetActiveQuestion(sessionId);
        if (activeQuestion == null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Session.NoActiveQuestion", "No active question to skip"));
        }

        var skipped = await _sessionQuestionRepository.SkipQuestion(activeQuestion.SessionQuestionId);
        return skipped
            ? Option.Some<bool, Error>(true)
            : Option.None<bool, Error>(Error.Failure("Session.SkipFailed", "Failed to skip question"));
    }

    public async Task<Option<bool, Error>> ExtendTime(Guid sessionQuestionId, int additionalSeconds)
    {
        var sessionQuestion = await _sessionQuestionRepository.GetSessionQuestionById(sessionQuestionId);
        if (sessionQuestion == null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("SessionQuestion.NotFound", "Session question not found"));
        }

        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<bool, Error>(
                Error.Unauthorized("Session.Unauthorized", "User not authenticated"));
        }

        var isHost = await _sessionRepository.CheckUserIsHost(sessionQuestion.SessionId, currentUserId.Value);
        if (!isHost)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("Session.NotHost", "Only the host can extend time"));
        }

        if (additionalSeconds <= 0 || additionalSeconds > 120)
        {
            return Option.None<bool, Error>(
                Error.ValidationError("Session.InvalidTimeExtension",
                    "Time extension must be between 1 and 120 seconds"));
        }

        var extended = await _sessionQuestionRepository.ExtendTimeLimit(sessionQuestionId, additionalSeconds);
        if (!extended)
        {
            return Option.None<bool, Error>(Error.Failure("Session.ExtendFailed", "Failed to extend time"));
        }

        // Get updated question details
        var updatedQuestion = await _sessionQuestionRepository.GetSessionQuestionById(sessionQuestionId);
        if (updatedQuestion?.Question != null)
        {
            var newTimeLimit = updatedQuestion.TimeLimitOverride ?? updatedQuestion.Question.TimeLimit;

            // Broadcast time extended event
            await _sessionHubContext.Clients.Group($"session:{sessionQuestion.SessionId}")
                .SendAsync("TimeExtended", new TimeExtendedEvent
                {
                    SessionQuestionId = sessionQuestionId,
                    AdditionalSeconds = additionalSeconds,
                    NewTimeLimit = newTimeLimit,
                    ExtendedAt = DateTime.UtcNow
                });
        }

        return Option.Some<bool, Error>(true);
    }

    public async Task<Guid?> ResolveAndActivateSessionQuestion(Guid sessionId, string questionId)
    {
        var sessionQuestions = await _sessionQuestionRepository.GetQuestionsBySessionId(sessionId);
        var sessionQuestion = sessionQuestions
            .FirstOrDefault(q => q.QuestionId.ToString() == questionId);

        if (sessionQuestion == null)
        {
            return null;
        }

        await _sessionQuestionRepository.ActivateQuestion(sessionQuestion.SessionQuestionId);
        return sessionQuestion.SessionQuestionId;
    }

    public async Task<Option<SubmitResponseResponse, Error>> SubmitResponse(Guid participantId,
        SubmitResponseRequest request)
    {
        // Get participant
        var participant = await _participantRepository.GetParticipantById(participantId);
        if (participant == null)
        {
            return Option.None<SubmitResponseResponse, Error>(
                Error.NotFound("Participant.NotFound", "Participant not found"));
        }

        // Get session question with question details
        var sessionQuestion = await _sessionQuestionRepository.GetSessionQuestionById(request.SessionQuestionId);
        if (sessionQuestion == null)
        {
            return Option.None<SubmitResponseResponse, Error>(
                Error.NotFound("SessionQuestion.NotFound", "Session question not found"));
        }

        // Validate question belongs to participant's session
        if (sessionQuestion.SessionId != participant.SessionId)
        {
            return Option.None<SubmitResponseResponse, Error>(
                Error.ValidationError("Session.QuestionMismatch", "Question does not belong to this session"));
        }

        // Check if question is active
        if (sessionQuestion.Status != SessionQuestionStatusEnum.ACTIVE)
        {
            return Option.None<SubmitResponseResponse, Error>(
                Error.ValidationError("SessionQuestion.NotActive", "Question is not currently active"));
        }

        // Check if already answered
        var alreadyAnswered = await _responseRepository.CheckParticipantAlreadyAnswered(
            request.SessionQuestionId, participantId);
        if (alreadyAnswered)
        {
            return Option.None<SubmitResponseResponse, Error>(
                Error.Conflict("Response.AlreadySubmitted", "You have already submitted a response for this question"));
        }

        var question = sessionQuestion.Question!;
        List<QuestionOption>? questionOptions = null;
        if (question.QuestionType is QuestionTypeEnum.MULTIPLE_CHOICE or QuestionTypeEnum.TRUE_FALSE)
        {
            questionOptions = await _questionOptionRepository.GetQuestionOptionsByQuestionId(question.QuestionId);
        }

        bool isCorrect = false;
        decimal? distanceError = null;
        QuestionOption? selectedOption = null; // Store selected option for event broadcasting

        // Validate and score based on question type
        switch (question.QuestionType)
        {
            case QuestionTypeEnum.MULTIPLE_CHOICE:
            case QuestionTypeEnum.TRUE_FALSE:
                if (request.QuestionOptionId == null)
                {
                    return Option.None<SubmitResponseResponse, Error>(
                        Error.ValidationError("Response.MissingOption", "Question option is required"));
                }

                selectedOption = questionOptions?.FirstOrDefault(o => o.QuestionOptionId == request.QuestionOptionId);
                if (selectedOption == null)
                {
                    return Option.None<SubmitResponseResponse, Error>(
                        Error.ValidationError("Response.InvalidOption", "Invalid question option"));
                }

                isCorrect = selectedOption.IsCorrect;
                break;

            case QuestionTypeEnum.SHORT_ANSWER:
                if (string.IsNullOrWhiteSpace(request.ResponseText))
                {
                    return Option.None<SubmitResponseResponse, Error>(
                        Error.ValidationError("Response.MissingText", "Response text is required"));
                }

                isCorrect = string.Equals(
                    request.ResponseText?.Trim(),
                    question.CorrectAnswerText?.Trim(),
                    StringComparison.OrdinalIgnoreCase);
                break;

            case QuestionTypeEnum.PIN_ON_MAP:
                if (request.ResponseLatitude == null || request.ResponseLongitude == null)
                {
                    return Option.None<SubmitResponseResponse, Error>(
                        Error.ValidationError("Response.MissingCoordinates", "Latitude and longitude are required"));
                }

                if (question.CorrectLatitude == null || question.CorrectLongitude == null)
                {
                    return Option.None<SubmitResponseResponse, Error>(
                        Error.ValidationError("Question.NoCorrectLocation",
                            "Question does not have a correct location set"));
                }

                // Calculate distance using Haversine formula
                distanceError = CalculateDistance(
                    (double)question.CorrectLatitude.Value,
                    (double)question.CorrectLongitude.Value,
                    (double)request.ResponseLatitude.Value,
                    (double)request.ResponseLongitude.Value);

                var acceptanceRadius = question.AcceptanceRadiusMeters ?? 1000; // Default 1km
                isCorrect = distanceError <= acceptanceRadius;
                break;

            default:
                return Option.None<SubmitResponseResponse, Error>(
                    Error.ValidationError("Question.UnsupportedType", "Question type is not supported"));
        }

        // Calculate points
        var basePoints = sessionQuestion.PointsOverride ?? question.Points;
        var pointsEarned = 0;

        if (isCorrect)
        {
            pointsEarned = basePoints;

            // Bonus points for speed if enabled
            var session = await _sessionRepository.GetSessionById(participant.SessionId);
            if (session?.PointsForSpeed == true && request.ResponseTimeSeconds > 0)
            {
                var timeLimit = sessionQuestion.TimeLimitOverride ?? question.TimeLimit;
                if (timeLimit > 0)
                {
                    // Bonus: 0-50% based on speed (faster = more bonus)
                    var speedRatio = 1 - (request.ResponseTimeSeconds / timeLimit);
                    if (speedRatio > 0)
                    {
                        var bonus = (int)(basePoints * 0.5m * (decimal)speedRatio);
                        pointsEarned += bonus;
                    }
                }
            }
        }

        // Create response
        var response = new StudentResponse
        {
            StudentResponseId = Guid.NewGuid(),
            SessionQuestionId = request.SessionQuestionId,
            SessionParticipantId = participantId,
            QuestionOptionId = request.QuestionOptionId,
            ResponseText = request.ResponseText,
            ResponseLatitude = request.ResponseLatitude,
            ResponseLongitude = request.ResponseLongitude,
            IsCorrect = isCorrect,
            PointsEarned = pointsEarned,
            ResponseTimeSeconds = request.ResponseTimeSeconds,
            UsedHint = request.UsedHint,
            DistanceErrorMeters = distanceError,
            SubmittedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _responseRepository.CreateResponse(response);
        if (!created)
        {
            return Option.None<SubmitResponseResponse, Error>(
                Error.Failure("Response.CreateFailed", "Failed to submit response"));
        }

        // Update session question stats
        await _sessionQuestionRepository.IncrementResponseCount(request.SessionQuestionId, isCorrect);

        // Update participant stats and score
        await _participantRepository.UpdateParticipantScore(participantId, pointsEarned);
        await _participantRepository.UpdateParticipantStats(participantId);

        // Update rankings
        await _participantRepository.UpdateParticipantRankings(participant.SessionId);

        // Get updated rank
        var currentRank = await _participantRepository.GetParticipantRank(participantId);

        // Get updated participant for total score
        var updatedParticipant = await _participantRepository.GetParticipantById(participantId);

        // Get total responses for this question
        var totalResponses = await _responseRepository.GetTotalResponseCount(request.SessionQuestionId);

        // Broadcast response submitted event with detailed answer content for teacher
        await _sessionHubContext.Clients.Group($"session:{participant.SessionId}")
            .SendAsync("ResponseSubmitted", new ResponseSubmittedEvent
            {
                SessionQuestionId = request.SessionQuestionId,
                ParticipantId = participantId,
                DisplayName = participant.DisplayName,
                IsCorrect = isCorrect,
                PointsEarned = pointsEarned,
                ResponseTimeSeconds = request.ResponseTimeSeconds,
                TotalResponses = totalResponses,
                SubmittedAt = response.SubmittedAt,
                // Answer content details for teacher to see in real-time
                QuestionOptionId = request.QuestionOptionId,
                OptionText = selectedOption?.OptionText,
                ResponseText = request.ResponseText,
                ResponseLatitude = request.ResponseLatitude,
                ResponseLongitude = request.ResponseLongitude,
                DistanceErrorMeters = distanceError
            });

        // Get and broadcast updated leaderboard
        var topParticipants = await _participantRepository.GetLeaderboard(participant.SessionId, 10);
        await _sessionHubContext.Clients.Group($"session:{participant.SessionId}")
            .SendAsync("LeaderboardUpdate", new LeaderboardUpdateEvent
            {
                SessionId = participant.SessionId,
                TopParticipants = topParticipants.Select(p => new EventLeaderboardEntry
                {
                    SessionParticipantId = p.SessionParticipantId,
                    DisplayName = p.DisplayName,
                    TotalScore = p.TotalScore,
                    TotalCorrect = p.TotalCorrect,
                    TotalAnswered = p.TotalAnswered,
                    AverageResponseTime = p.AverageResponseTime,
                    Rank = p.Rank
                }).ToList(),
                UpdatedAt = DateTime.UtcNow
            });

        // Broadcast updated question responses list (realtime for teacher to see who answered what)
        await BroadcastQuestionResponsesUpdate(request.SessionQuestionId, participant.SessionId);

        return Option.Some<SubmitResponseResponse, Error>(new SubmitResponseResponse
        {
            StudentResponseId = response.StudentResponseId,
            IsCorrect = isCorrect,
            PointsEarned = pointsEarned,
            TotalScore = updatedParticipant?.TotalScore ?? 0,
            CurrentRank = currentRank,
            Explanation = question.Explanation,
            Message = isCorrect ? "Correct answer!" : "Incorrect answer",
            SubmittedAt = response.SubmittedAt
        });
    }

    public async Task<Option<WordCloudDataDto, Error>> GetWordCloudData(Guid sessionQuestionId)
    {
        // Validate session question exists
        var sessionQuestion = await _sessionQuestionRepository.GetSessionQuestionById(sessionQuestionId);
        if (sessionQuestion == null)
        {
            return Option.None<WordCloudDataDto, Error>(
                Error.NotFound("SessionQuestion.NotFound", "Session question not found"));
        }

        var question = sessionQuestion.Question!;

        // Check question type is SHORT_ANSWER (WordCloud questions are stored as SHORT_ANSWER type)
        // Note: If WordCloud becomes a separate enum value, update this check
        if (question.QuestionType != QuestionTypeEnum.SHORT_ANSWER)
        {
            return Option.None<WordCloudDataDto, Error>(
                Error.ValidationError("Question.InvalidType", "Question is not a Word Cloud question"));
        }

        // Get all responses for this question
        var responses = await _responseRepository.GetResponsesBySessionQuestion(sessionQuestionId);

        // Aggregate word cloud data
        var wordCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var response in responses)
        {
            if (!string.IsNullOrWhiteSpace(response.ResponseText))
            {
                // Split by common delimiters (space, comma, semicolon, etc.)
                var words = response.ResponseText
                    .Split(new[] { ' ', ',', ';', '\n', '\r', '\t', '.', '!', '?' },
                        StringSplitOptions.RemoveEmptyEntries)
                    .Select(w => w.Trim().ToLowerInvariant())
                    .Where(w => w.Length > 0 && w.Length < 50) // Filter out very long words
                    .ToList();

                foreach (var word in words)
                {
                    if (wordCount.ContainsKey(word))
                    {
                        wordCount[word]++;
                    }
                    else
                    {
                        wordCount[word] = 1;
                    }
                }
            }
        }

        // Build response
        var totalResponses = responses.Count;
        var entries = wordCount
            .OrderByDescending(x => x.Value)
            .Select(x => new WordCloudEntryDto
            {
                Word = x.Key,
                Count = x.Value,
                Frequency = totalResponses > 0 ? (int)((double)x.Value / totalResponses * 100) : 0
            })
            .ToList();

        return Option.Some<WordCloudDataDto, Error>(new WordCloudDataDto
        {
            SessionQuestionId = sessionQuestionId,
            Entries = entries,
            TotalResponses = totalResponses
        });
    }

    public async Task<Option<MapPinsDataDto, Error>> GetMapPinsData(Guid sessionQuestionId)
    {
        // Validate session question exists
        var sessionQuestion = await _sessionQuestionRepository.GetSessionQuestionById(sessionQuestionId);
        if (sessionQuestion == null)
        {
            return Option.None<MapPinsDataDto, Error>(
                Error.NotFound("SessionQuestion.NotFound", "Session question not found"));
        }

        var question = sessionQuestion.Question!;

        // Check question type is PIN_ON_MAP
        if (question.QuestionType != QuestionTypeEnum.PIN_ON_MAP)
        {
            return Option.None<MapPinsDataDto, Error>(
                Error.ValidationError("Question.InvalidType", "Question is not a Pin on Map question"));
        }

        // Get all responses for this question
        var responses = await _responseRepository.GetResponsesBySessionQuestion(sessionQuestionId);

        // Build map pins data
        var pins = new List<MapPinEntryDto>();

        foreach (var response in responses)
        {
            if (response.ResponseLatitude.HasValue && response.ResponseLongitude.HasValue)
            {
                var isCorrect = response.IsCorrect;
                var distanceFromCorrect = (double)(response.DistanceErrorMeters ?? 0);
                var pointsEarned = response.PointsEarned;

                // Get participant info
                var participant = response.SessionParticipant;
                var displayName = participant?.DisplayName ?? "Unknown";

                pins.Add(new MapPinEntryDto
                {
                    ParticipantId = response.SessionParticipantId,
                    DisplayName = displayName,
                    Latitude = response.ResponseLatitude.Value,
                    Longitude = response.ResponseLongitude.Value,
                    IsCorrect = isCorrect,
                    DistanceFromCorrect = distanceFromCorrect,
                    PointsEarned = pointsEarned
                });
            }
        }

        return Option.Some<MapPinsDataDto, Error>(new MapPinsDataDto
        {
            SessionQuestionId = sessionQuestionId,
            Pins = pins,
            TotalResponses = responses.Count,
            CorrectLatitude = question.CorrectLatitude,
            CorrectLongitude = question.CorrectLongitude,
            AcceptanceRadiusMeters = question.AcceptanceRadiusMeters
        });
    }

    public async Task<Option<QuestionResponsesResponse, Error>> GetQuestionResponses(Guid sessionQuestionId)
    {
        // Validate session question exists
        var sessionQuestion = await _sessionQuestionRepository.GetSessionQuestionById(sessionQuestionId);
        if (sessionQuestion == null)
        {
            return Option.None<QuestionResponsesResponse, Error>(
                Error.NotFound("SessionQuestion.NotFound", "Session question not found"));
        }

        // Get all responses with participant and option details
        var responses = await _responseRepository.GetResponsesBySessionQuestion(sessionQuestionId);

        // Map responses to DTOs
        var answerDetails = responses.Select(response => new StudentAnswerDetailDto
        {
            StudentResponseId = response.StudentResponseId,
            ParticipantId = response.SessionParticipantId,
            DisplayName = response.SessionParticipant?.DisplayName ?? "Unknown",
            IsCorrect = response.IsCorrect,
            PointsEarned = response.PointsEarned,
            ResponseTimeSeconds = response.ResponseTimeSeconds,
            SubmittedAt = response.SubmittedAt,
            // Answer content based on question type
            QuestionOptionId = response.QuestionOptionId,
            OptionText = response.QuestionOption?.OptionText,
            ResponseText = response.ResponseText,
            ResponseLatitude = response.ResponseLatitude,
            ResponseLongitude = response.ResponseLongitude,
            DistanceErrorMeters = response.DistanceErrorMeters
        }).ToList();

        return Option.Some<QuestionResponsesResponse, Error>(new QuestionResponsesResponse
        {
            SessionQuestionId = sessionQuestionId,
            TotalResponses = responses.Count,
            Answers = answerDetails
        });
    }

    // Helper method to calculate distance between two points (Haversine formula)
    private decimal CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // Earth's radius in meters
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = R * c;

        return (decimal)distance;
    }

    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    private async Task BroadcastQuestionResponsesUpdate(Guid sessionQuestionId, Guid sessionId)
    {
        var responses = await _responseRepository.GetResponsesBySessionQuestion(sessionQuestionId);

        // Map responses to event DTOs
        var answerDetails = responses.Select(response => new StudentAnswerDetailEvent
        {
            StudentResponseId = response.StudentResponseId,
            ParticipantId = response.SessionParticipantId,
            DisplayName = response.SessionParticipant?.DisplayName ?? "Unknown",
            IsCorrect = response.IsCorrect,
            PointsEarned = response.PointsEarned,
            ResponseTimeSeconds = response.ResponseTimeSeconds,
            SubmittedAt = response.SubmittedAt,
            // Answer content based on question type
            QuestionOptionId = response.QuestionOptionId,
            OptionText = response.QuestionOption?.OptionText,
            ResponseText = response.ResponseText,
            ResponseLatitude = response.ResponseLatitude,
            ResponseLongitude = response.ResponseLongitude,
            DistanceErrorMeters = response.DistanceErrorMeters
        }).ToList();

        var updateEvent = new QuestionResponsesUpdateEvent
        {
            SessionQuestionId = sessionQuestionId,
            SessionId = sessionId,
            TotalResponses = responses.Count,
            Answers = answerDetails,
            UpdatedAt = DateTime.UtcNow
        };

        // Broadcast to all participants in the session
        await _sessionHubContext.Clients.Group($"session:{sessionId}")
            .SendAsync("QuestionResponsesUpdate", updateEvent);
    }


    private string GenerateParticipantKeyForUser(Guid userId, Guid sessionId)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var keyBytes = System.Text.Encoding.UTF8.GetBytes($"{userId}:{sessionId}");
        var hashBytes = sha256.ComputeHash(keyBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private string GenerateGuestParticipantKey()
    {
        return Guid.NewGuid().ToString("N");
    }

    public async Task<Option<SessionSummaryResponse, Error>> GetSessionSummary(Guid sessionId)
    {
        // 1. Validate session exists and get full details
        var session = await _sessionRepository.GetSessionWithFullDetails(sessionId);
        if (session == null)
        {
            return Option.None<SessionSummaryResponse, Error>(
                Error.NotFound("Session.NotFound", "Session not found"));
        }

        // Verify current user is the host (teacher)
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null || session.HostUserId != currentUserId)
        {
            return Option.None<SessionSummaryResponse, Error>(
                Error.Forbidden("Session.NotHost", "Only the session host can view the summary"));
        }

        // 2. Get all participants
        var participants = await _participantRepository.GetParticipantsBySessionId(sessionId);
        
        // 3. Get all session questions with their questions
        var sessionQuestions = await _sessionQuestionRepository.GetQuestionsBySessionId(sessionId);
        
        // 4. Get all responses for this session (via session questions)
        var allResponses = new List<StudentResponse>();
        foreach (var sq in sessionQuestions)
        {
            var responses = await _responseRepository.GetResponsesBySessionQuestion(sq.SessionQuestionId);
            allResponses.AddRange(responses);
        }

        // 5. Calculate statistics
        var totalParticipants = participants.Count;
        var totalQuestions = sessionQuestions.Count;
        var totalResponses = allResponses.Count;
        var totalCorrect = allResponses.Count(r => r.IsCorrect);
        var totalIncorrect = totalResponses - totalCorrect;

        // Score analysis
        var scores = participants.Select(p => p.TotalScore).OrderBy(s => s).ToList();
        var avgScore = scores.Count > 0 ? scores.Average() : 0;
        var highestScore = scores.Count > 0 ? scores.Max() : 0;
        var lowestScore = scores.Count > 0 ? scores.Min() : 0;
        var medianScore = scores.Count > 0 ? scores[scores.Count / 2] : 0m;

        // Accuracy and time
        var avgAccuracy = totalResponses > 0 ? (decimal)totalCorrect / totalResponses * 100 : 0;
        var avgResponseTime = allResponses.Count > 0 
            ? allResponses.Average(r => r.ResponseTimeSeconds) 
            : 0m;

        // Participation stats
        var expectedResponses = totalParticipants * totalQuestions;
        var participationRate = expectedResponses > 0 
            ? (decimal)totalResponses / expectedResponses * 100 
            : 0;
        var completionRate = participationRate;

        var perfectScoreCount = 0;
        var zeroScoreCount = 0;
        foreach (var p in participants)
        {
            var maxPossibleScore = sessionQuestions.Sum(sq => sq.Question?.Points ?? 100);
            if (p.TotalScore >= maxPossibleScore && maxPossibleScore > 0) perfectScoreCount++;
            if (p.TotalScore == 0) zeroScoreCount++;
        }

        // 6. Build Question Breakdowns
        var questionBreakdowns = new List<QuestionBreakdown>();
        foreach (var sq in sessionQuestions.OrderBy(q => q.QueueOrder))
        {
            var question = sq.Question;
            if (question == null) continue;

            var responses = allResponses.Where(r => r.SessionQuestionId == sq.SessionQuestionId).ToList();
            var correctCount = responses.Count(r => r.IsCorrect);
            var incorrectCount = responses.Count - correctCount;
            var correctPct = responses.Count > 0 ? (decimal)correctCount / responses.Count * 100 : 0;
            var avgTime = responses.Count > 0 ? responses.Average(r => r.ResponseTimeSeconds) : 0m;
            var avgPoints = responses.Count > 0 ? responses.Average(r => (decimal)r.PointsEarned) : 0m;

            var breakdown = new QuestionBreakdown
            {
                SessionQuestionId = sq.SessionQuestionId,
                QuestionId = question.QuestionId,
                QueueOrder = sq.QueueOrder,
                QuestionType = question.QuestionType,
                QuestionText = question.QuestionText,
                QuestionImageUrl = question.QuestionImageUrl,
                Points = question.Points,
                TimeLimit = question.TimeLimit,
                TotalResponses = responses.Count,
                CorrectResponses = correctCount,
                IncorrectResponses = incorrectCount,
                CorrectPercentage = Math.Round(correctPct, 1),
                AverageResponseTimeSeconds = Math.Round(avgTime, 1),
                AveragePointsEarned = Math.Round(avgPoints, 1),
                DifficultyLevel = correctPct >= 80 ? "Dễ" : (correctPct >= 50 ? "Trung bình" : "Khó")
            };

            // Option analysis for MULTIPLE_CHOICE and TRUE_FALSE
            if (question.QuestionType == QuestionTypeEnum.MULTIPLE_CHOICE || 
                question.QuestionType == QuestionTypeEnum.TRUE_FALSE)
            {
                var options = await _questionOptionRepository.GetQuestionOptionsByQuestionId(question.QuestionId);
                breakdown.OptionAnalysis = options.Select(opt =>
                {
                    var selectCount = responses.Count(r => r.QuestionOptionId == opt.QuestionOptionId);
                    return new OptionAnalysis
                    {
                        OptionId = opt.QuestionOptionId,
                        OptionText = opt.OptionText,
                        IsCorrect = opt.IsCorrect,
                        SelectCount = selectCount,
                        SelectPercentage = responses.Count > 0 
                            ? Math.Round((decimal)selectCount / responses.Count * 100, 1) 
                            : 0
                    };
                }).ToList();
            }

            // Distance error for PIN_ON_MAP
            if (question.QuestionType == QuestionTypeEnum.PIN_ON_MAP)
            {
                var distanceErrors = responses
                    .Where(r => r.DistanceErrorMeters.HasValue)
                    .Select(r => r.DistanceErrorMeters!.Value)
                    .ToList();
                breakdown.AverageDistanceErrorMeters = distanceErrors.Count > 0 
                    ? Math.Round(distanceErrors.Average(), 1) 
                    : null;
            }

            // Word frequency for SHORT_ANSWER
            if (question.QuestionType == QuestionTypeEnum.SHORT_ANSWER)
            {
                var wordCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var response in responses)
                {
                    if (!string.IsNullOrWhiteSpace(response.ResponseText))
                    {
                        var normalized = response.ResponseText.Trim().ToLowerInvariant();
                        wordCount[normalized] = wordCount.GetValueOrDefault(normalized) + 1;
                    }
                }

                breakdown.CommonAnswers = wordCount
                    .OrderByDescending(x => x.Value)
                    .Take(10)
                    .Select(x => new WordFrequency
                    {
                        Word = x.Key,
                        Count = x.Value,
                        Percentage = responses.Count > 0 
                            ? Math.Round((decimal)x.Value / responses.Count * 100, 1) 
                            : 0
                    }).ToList();
            }

            questionBreakdowns.Add(breakdown);
        }

        // 7. Top Performers
        var topPerformers = participants
            .OrderByDescending(p => p.TotalScore)
            .ThenByDescending(p => p.TotalCorrect)
            .ThenBy(p => p.AverageResponseTime)
            .Take(10)
            .Select((p, index) => new TopPerformer
            {
                Rank = index + 1,
                SessionParticipantId = p.SessionParticipantId,
                DisplayName = p.DisplayName,
                TotalScore = p.TotalScore,
                TotalCorrect = p.TotalCorrect,
                TotalAnswered = p.TotalAnswered,
                AccuracyPercent = p.TotalAnswered > 0 
                    ? Math.Round((decimal)p.TotalCorrect / p.TotalAnswered * 100, 1) 
                    : 0,
                AverageResponseTimeSeconds = Math.Round(p.AverageResponseTime, 1)
            }).ToList();

        // 8. Score Distribution for histogram
        var scoreDistribution = new List<ScoreDistributionBucket>();
        if (scores.Count > 0)
        {
            var maxScore = highestScore;
            var bucketSize = maxScore > 0 ? Math.Max(100, (int)Math.Ceiling(maxScore / 10.0)) : 100;
            var bucketCount = maxScore > 0 ? (int)Math.Ceiling((double)maxScore / bucketSize) : 1;
            bucketCount = Math.Max(1, Math.Min(bucketCount, 10));
            bucketSize = maxScore > 0 ? (int)Math.Ceiling((double)maxScore / bucketCount) : 100;

            for (int i = 0; i < bucketCount; i++)
            {
                var minScore = i * bucketSize;
                var maxBucketScore = (i + 1) * bucketSize;
                var count = scores.Count(s => s >= minScore && s < maxBucketScore);
                
                // Include max score in last bucket
                if (i == bucketCount - 1)
                {
                    count = scores.Count(s => s >= minScore);
                }

                scoreDistribution.Add(new ScoreDistributionBucket
                {
                    Label = $"{minScore}-{maxBucketScore}",
                    MinScore = minScore,
                    MaxScore = maxBucketScore,
                    Count = count,
                    Percentage = totalParticipants > 0 
                        ? Math.Round((decimal)count / totalParticipants * 100, 1) 
                        : 0
                });
            }
        }

        // 9. Participant Analysis
        var participantAnalysis = new ParticipantAnalysis
        {
            TotalJoined = totalParticipants,
            ActiveThroughout = participants.Count(p => !p.LeftAt.HasValue || p.LeftAt >= session.EndTime),
            LeftEarly = participants.Count(p => p.LeftAt.HasValue && (session.EndTime == null || p.LeftAt < session.EndTime)),
            GuestParticipants = participants.Count(p => p.IsGuest),
            RegisteredUsers = participants.Count(p => !p.IsGuest),
            AverageQuestionsAnswered = totalParticipants > 0 
                ? Math.Round((decimal)totalResponses / totalParticipants, 1) 
                : 0
        };

        // 10. Question Banks Summary
        var questionBankSummaries = new List<QuestionBankSummary>();
        var sessionQuestionBanks = await _sessionQuestionBankRepository.GetQuestionBanks(sessionId);
        foreach (var sqb in sessionQuestionBanks)
        {
            var qb = await _questionBankRepository.GetQuestionBankById(sqb.QuestionBankId);
            if (qb != null)
            {
                var questionCount = await _questionRepository.GetActiveQuestionCountByQuestionBankId(sqb.QuestionBankId);
                questionBankSummaries.Add(new QuestionBankSummary
                {
                    QuestionBankId = qb.QuestionBankId,
                    QuestionBankName = qb.BankName,
                    TotalQuestions = questionCount
                });
            }
        }

        // Calculate duration
        var durationMinutes = 0;
        if (session.ActualStartTime.HasValue && session.EndTime.HasValue)
        {
            durationMinutes = (int)(session.EndTime.Value - session.ActualStartTime.Value).TotalMinutes;
        }

        // 11. Build final response
        var summary = new SessionSummaryResponse
        {
            SessionId = sessionId,
            SessionCode = session.SessionCode,
            SessionName = session.SessionName,
            Description = session.Description,
            Status = session.Status.ToString(),
            ActualStartTime = session.ActualStartTime,
            EndTime = session.EndTime,
            DurationMinutes = durationMinutes,
            MapId = session.MapId,
            MapName = session.Map?.MapName ?? "",
            QuestionBanks = questionBankSummaries,
            Statistics = new OverallStatistics
            {
                TotalParticipants = totalParticipants,
                TotalQuestions = totalQuestions,
                TotalResponses = totalResponses,
                AverageScore = Math.Round((decimal)avgScore, 1),
                AverageAccuracyPercent = Math.Round(avgAccuracy, 1),
                AverageResponseTimeSeconds = Math.Round(avgResponseTime, 1),
                HighestScore = highestScore,
                LowestScore = lowestScore,
                MedianScore = medianScore,
                ParticipationRatePercent = Math.Round(participationRate, 1),
                CompletionRatePercent = Math.Round(completionRate, 1),
                ParticipantsWithPerfectScore = perfectScoreCount,
                ParticipantsWithZeroScore = zeroScoreCount,
                TotalCorrectAnswers = totalCorrect,
                TotalIncorrectAnswers = totalIncorrect
            },
            ParticipantAnalysis = participantAnalysis,
            QuestionBreakdowns = questionBreakdowns,
            TopPerformers = topPerformers,
            ScoreDistribution = scoreDistribution
        };

        return Option.Some<SessionSummaryResponse, Error>(summary);
    }
}