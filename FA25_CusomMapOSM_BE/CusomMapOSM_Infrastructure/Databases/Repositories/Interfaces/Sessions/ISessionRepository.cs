using CusomMapOSM_Domain.Entities.Sessions;
using CusomMapOSM_Domain.Entities.Sessions.Enums;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;

public interface ISessionRepository
{
    // CRUD Operations
    Task<bool> CreateSession(Session session);
    Task<Session?> GetSessionById(Guid sessionId);
    Task<Session?> GetSessionByCode(string sessionCode);
    Task<List<Session>> GetSessionsByHostUserId(Guid hostUserId);
    Task<List<Session>> GetSessionsByOrganizationId(Guid organizationId);
    Task<List<Session>> GetSessionsByMapId(Guid mapId);
    Task<List<Session>> GetSessionsByStatus(SessionStatusEnum status);
    Task<bool> UpdateSession(Session session);
    Task<bool> DeleteSession(Guid sessionId);

    // With Related Data
    Task<Session?> GetSessionWithQuestions(Guid sessionId);
    Task<Session?> GetSessionWithParticipants(Guid sessionId);
    Task<Session?> GetSessionWithFullDetails(Guid sessionId);

    // Code Generation
    Task<string> GenerateUniqueSessionCode();
    Task<bool> CheckSessionCodeExists(string sessionCode);

    // Validation
    Task<bool> CheckSessionExists(Guid sessionId);
    Task<bool> CheckUserIsHost(Guid sessionId, Guid userId);

    // Status Management
    Task<bool> UpdateSessionStatus(Guid sessionId, SessionStatusEnum status);
    Task<bool> StartSession(Guid sessionId);
    Task<bool> EndSession(Guid sessionId);
    Task<bool> PauseSession(Guid sessionId);
    Task<bool> ResumeSession(Guid sessionId);

    // Statistics
    Task<bool> UpdateParticipantCount(Guid sessionId);
    Task<bool> UpdateResponseCount(Guid sessionId);
}