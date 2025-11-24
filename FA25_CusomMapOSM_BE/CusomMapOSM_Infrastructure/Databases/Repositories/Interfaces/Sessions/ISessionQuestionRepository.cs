using CusomMapOSM_Domain.Entities.Sessions;
using CusomMapOSM_Domain.Entities.Sessions.Enums;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;

public interface ISessionQuestionRepository
{
    // CRUD Operations
    Task<bool> CreateSessionQuestion(SessionQuestion sessionQuestion);
    Task<SessionQuestion?> GetSessionQuestionById(Guid sessionQuestionId);
    Task<List<SessionQuestion>> GetQuestionsBySessionId(Guid sessionId);
    Task<SessionQuestion?> GetActiveQuestion(Guid sessionId);
    Task<SessionQuestion?> GetNextQueuedQuestion(Guid sessionId);
    Task<bool> UpdateSessionQuestion(SessionQuestion sessionQuestion);
    Task<bool> DeleteSessionQuestion(Guid sessionQuestionId);

    // Queue Management
    Task<bool> UpdateQuestionStatus(Guid sessionQuestionId, SessionQuestionStatusEnum status);
    Task<bool> ActivateQuestion(Guid sessionQuestionId);
    Task<bool> CompleteQuestion(Guid sessionQuestionId);
    Task<bool> SkipQuestion(Guid sessionQuestionId);

    // Time Management
    Task<bool> ExtendTimeLimit(Guid sessionQuestionId, int additionalSeconds);
    Task<bool> UpdateStartTime(Guid sessionQuestionId);
    Task<bool> UpdateEndTime(Guid sessionQuestionId);

    // Statistics
    Task<bool> UpdateResponseStats(Guid sessionQuestionId);
    Task<bool> IncrementResponseCount(Guid sessionQuestionId, bool isCorrect);
    Task<int> GetTotalQuestionsInSession(Guid sessionId);

    // Validation
    Task<bool> CheckSessionQuestionExists(Guid sessionQuestionId);
    Task<bool> CheckQuestionBelongsToSession(Guid sessionQuestionId, Guid sessionId);

    // Bulk Operations
    Task<bool> CreateSessionQuestions(List<SessionQuestion> sessionQuestions);
}