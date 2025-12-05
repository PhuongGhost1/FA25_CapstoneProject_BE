using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Sessions.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Sessions.Response;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Sessions;

public interface ISessionService
{
    // Session CRUD
    Task<Option<CreateSessionResponse, Error>> CreateSession(CreateSessionRequest request);
    Task<Option<GetSessionResponse, Error>> GetSessionById(Guid sessionId);
    Task<Option<GetSessionResponse, Error>> GetSessionByCode(string sessionCode);
    Task<Option<List<GetSessionResponse>, Error>> GetMySessionsAsHost();
    Task<Option<bool, Error>> DeleteSession(Guid sessionId);

    // Session Control
    Task<Option<bool, Error>> StartSession(Guid sessionId);
    Task<Option<bool, Error>> PauseSession(Guid sessionId);
    Task<Option<bool, Error>> ResumeSession(Guid sessionId);
    Task<Option<bool, Error>> EndSession(Guid sessionId);

    // Participant Management
    Task<Option<JoinSessionResponse, Error>> JoinSession(JoinSessionRequest request);
    Task<Option<bool, Error>> LeaveSession(Guid sessionParticipantId);
    Task<Option<LeaderboardResponse, Error>> GetLeaderboard(Guid sessionId, int limit = 10);

    // Question Management
    Task<Option<bool, Error>> ActivateNextQuestion(Guid sessionId);
    Task<Option<bool, Error>> SkipCurrentQuestion(Guid sessionId);
    Task<Option<bool, Error>> ExtendTime(Guid sessionQuestionId, int additionalSeconds);
    Task<Guid?> ResolveAndActivateSessionQuestion(Guid sessionId, string questionId);

    // Response Submission
    Task<Option<SubmitResponseResponse, Error>> SubmitResponse(Guid participantId, SubmitResponseRequest request);

    // Analytics
    Task<Option<WordCloudDataDto, Error>> GetWordCloudData(Guid sessionQuestionId);
    Task<Option<MapPinsDataDto, Error>> GetMapPinsData(Guid sessionQuestionId);
    
    // Get all responses for a question (Teacher can see who answered what)
    Task<Option<QuestionResponsesResponse, Error>> GetQuestionResponses(Guid sessionQuestionId);
}