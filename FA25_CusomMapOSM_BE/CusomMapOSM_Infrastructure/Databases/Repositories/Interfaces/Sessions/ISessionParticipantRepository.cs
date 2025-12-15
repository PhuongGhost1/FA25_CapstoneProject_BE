using CusomMapOSM_Domain.Entities.Sessions;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;

public interface ISessionParticipantRepository
{
    // CRUD Operations
    Task<bool> CreateParticipant(SessionParticipant participant);
    Task<SessionParticipant?> GetParticipantById(Guid participantId);
    Task<List<SessionParticipant>> GetParticipantsBySessionId(Guid sessionId);
    Task<bool> UpdateParticipant(SessionParticipant participant);
    Task<bool> RemoveParticipant(Guid participantId);

    // Leaderboard
    Task<List<SessionParticipant>> GetLeaderboard(Guid sessionId, int limit = 10);
    Task<int> GetParticipantRank(Guid participantId);
    Task<bool> UpdateParticipantRankings(Guid sessionId);

    // Statistics
    Task<bool> UpdateParticipantScore(Guid participantId, int points);
    Task<bool> UpdateParticipantStats(Guid participantId);
    Task<int> GetActiveParticipantCount(Guid sessionId);

    // Validation
    Task<bool> CheckParticipantExists(Guid participantId);
    Task<bool> CheckParticipantBelongsToSession(Guid participantId, Guid sessionId);
    Task<bool> CheckParticipantKeyExistsInSession(Guid sessionId, string participantKey);
    Task<SessionParticipant?> GetParticipantBySessionAndParticipantKey(Guid sessionId, string participantKey);

    // Session Management
    Task<bool> MarkParticipantAsLeft(Guid participantId);
    Task<bool> MarkAllParticipantsAsLeft(Guid sessionId);
}