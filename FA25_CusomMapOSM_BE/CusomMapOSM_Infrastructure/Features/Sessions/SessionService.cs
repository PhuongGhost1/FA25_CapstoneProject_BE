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
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.QuestionBanks;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;
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

    public SessionService(
        ISessionRepository sessionRepository,
        ISessionParticipantRepository participantRepository,
        ISessionQuestionRepository sessionQuestionRepository,
        IStudentResponseRepository responseRepository,
        IQuestionRepository questionRepository,
        IQuestionOptionRepository questionOptionRepository,
        ISessionQuestionBankRepository sessionQuestionBankRepository,
        ICurrentUserService currentUserService,
        IHubContext<SessionHub> sessionHubContext)
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
    }

    public async Task<Option<CreateSessionResponse, Error>> CreateSession(CreateSessionRequest request)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<CreateSessionResponse, Error>(
                Error.Unauthorized("Session.Unauthorized", "User not authenticated"));
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

        // Attach question bank if provided
        if (request.QuestionBankId != null)
        {
            var sessionQuestionBank = new SessionQuestionBank
            {
                SessionQuestionBankId = Guid.NewGuid(),
                SessionId = session.SessionId,
                QuestionBankId = request.QuestionBankId.Value,
                AttachedAt = DateTime.UtcNow
            };
            await _sessionQuestionBankRepository.AddQuestionBank(sessionQuestionBank);

            // Create SessionQuestions from the attached question bank
            var questions = await _questionRepository.GetQuestionsByQuestionBankId(request.QuestionBankId.Value);
            if (request.ShuffleQuestions)
            {
                questions = questions.OrderBy(_ => Guid.NewGuid()).ToList();
            }

            var sessionQuestions = questions.Select((q, index) => new SessionQuestion
            {
                SessionQuestionId = Guid.NewGuid(),
                SessionId = session.SessionId,
                QuestionId = q.QuestionId,
                QueueOrder = index + 1,
                Status = SessionQuestionStatusEnum.QUEUED,
                CreatedAt = DateTime.UtcNow
            }).ToList();
            await _sessionQuestionRepository.CreateSessionQuestions(sessionQuestions);
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

        // Get first question bank attached to this session (if any)
        var sessionQuestionBanks = await _sessionQuestionBankRepository.GetQuestionBanks(session.SessionId);
        var firstQuestionBank = sessionQuestionBanks.FirstOrDefault()?.QuestionBank;

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
            QuestionBankId = firstQuestionBank?.QuestionBankId,
            QuestionBankName = firstQuestionBank?.BankName ?? string.Empty,
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

    public async Task<Option<List<GetSessionResponse>, Error>> GetMySessionsAsHost()
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<List<GetSessionResponse>, Error>(
                Error.Unauthorized("Session.Unauthorized", "User not authenticated"));
        }

        var sessions = await _sessionRepository.GetSessionsByHostUserId(currentUserId.Value);

        var response = new List<GetSessionResponse>();
        foreach (var s in sessions)
        {
            // Get first question bank attached to this session (if any)
            var sessionQuestionBanks = await _sessionQuestionBankRepository.GetQuestionBanks(s.SessionId);
            var firstQuestionBank = sessionQuestionBanks.FirstOrDefault()?.QuestionBank;

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
                QuestionBankId = firstQuestionBank?.QuestionBankId,
                QuestionBankName = firstQuestionBank?.BankName ?? string.Empty,
                HostUserId = s.HostUserId,
                HostUserName = s.HostUser?.FullName ?? string.Empty,
                TotalParticipants = s.TotalParticipants,
                TotalResponses = s.TotalResponses,
                CreatedAt = s.CreatedAt
            });
        }

        return Option.Some<List<GetSessionResponse>, Error>(response);
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

        // Check max participants
        if (session.MaxParticipants > 0 && session.TotalParticipants >= session.MaxParticipants)
        {
            return Option.None<JoinSessionResponse, Error>(
                Error.ValidationError("Session.Full", "Session has reached maximum participants"));
        }

        var currentUserId = _currentUserService.GetUserId();

        // Check if user already joined
        if (currentUserId != null)
        {
            var alreadyJoined =
                await _participantRepository.CheckUserAlreadyJoined(session.SessionId, currentUserId.Value);
            if (alreadyJoined)
            {
                return Option.None<JoinSessionResponse, Error>(
                    Error.Conflict("Session.AlreadyJoined", "You have already joined this session"));
            }
        }

        // Create participant
        var participant = new SessionParticipant
        {
            SessionParticipantId = Guid.NewGuid(),
            SessionId = session.SessionId,
            UserId = currentUserId,
            DisplayName = request.DisplayName,
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
        var sessionExists = await _sessionRepository.CheckSessionExists(sessionId);
        if (!sessionExists)
        {
            return Option.None<LeaderboardResponse, Error>(
                Error.NotFound("Session.NotFound", "Session not found"));
        }

        var participants = await _participantRepository.GetLeaderboard(sessionId, limit);
        var currentUserId = _currentUserService.GetUserId();

        var leaderboard = participants.Select((p, index) => new ResponseLeaderboardEntry
        {
            Rank = index + 1,
            SessionParticipantId = p.SessionParticipantId,
            DisplayName = p.DisplayName,
            TotalScore = p.TotalScore,
            TotalCorrect = p.TotalCorrect,
            TotalAnswered = p.TotalAnswered,
            AverageResponseTime = p.AverageResponseTime,
            IsCurrentUser = currentUserId != null && p.UserId == currentUserId.Value
        }).ToList();

        return Option.Some<LeaderboardResponse, Error>(new LeaderboardResponse
        {
            SessionId = sessionId,
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

                var option = questionOptions?.FirstOrDefault(o => o.QuestionOptionId == request.QuestionOptionId);
                if (option == null)
                {
                    return Option.None<SubmitResponseResponse, Error>(
                        Error.ValidationError("Response.InvalidOption", "Invalid question option"));
                }

                isCorrect = option.IsCorrect;
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

        // Broadcast response submitted event
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
                SubmittedAt = response.SubmittedAt
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
}