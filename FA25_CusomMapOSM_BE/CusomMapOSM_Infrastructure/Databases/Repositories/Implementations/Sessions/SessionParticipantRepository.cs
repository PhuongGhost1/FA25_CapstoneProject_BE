using CusomMapOSM_Domain.Entities.Sessions;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Sessions;

public class SessionParticipantRepository : ISessionParticipantRepository
{
    private readonly CustomMapOSMDbContext _context;

    public SessionParticipantRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateParticipant(SessionParticipant participant)
    {
        _context.SessionParticipants.Add(participant);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<SessionParticipant?> GetParticipantById(Guid participantId)
    {
        return await _context.SessionParticipants
            .Include(sp => sp.Session)
            .FirstOrDefaultAsync(sp => sp.SessionParticipantId == participantId);
    }

    public async Task<List<SessionParticipant>> GetParticipantsBySessionId(Guid sessionId)
    {
        return await _context.SessionParticipants
            .Where(sp => sp.SessionId == sessionId)
            .OrderByDescending(sp => sp.TotalScore)
            .ThenBy(sp => sp.AverageResponseTime)
            .ToListAsync();
    }

    public async Task<bool> UpdateParticipant(SessionParticipant participant)
    {
        participant.UpdatedAt = DateTime.UtcNow;
        _context.SessionParticipants.Update(participant);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> RemoveParticipant(Guid participantId)
    {
        var participant = await _context.SessionParticipants.FindAsync(participantId);
        if (participant == null) return false;

        _context.SessionParticipants.Remove(participant);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<SessionParticipant>> GetLeaderboard(Guid sessionId, int limit = 10)
    {
        return await _context.SessionParticipants
            .Where(sp => sp.SessionId == sessionId && sp.IsActive)
            .OrderByDescending(sp => sp.TotalScore)
            .ThenBy(sp => sp.AverageResponseTime)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> GetParticipantRank(Guid participantId)
    {
        var participant = await _context.SessionParticipants.FindAsync(participantId);
        if (participant == null) return 0;

        return await _context.SessionParticipants
            .Where(sp => sp.SessionId == participant.SessionId && sp.IsActive)
            .Where(sp => sp.TotalScore > participant.TotalScore ||
                        (sp.TotalScore == participant.TotalScore && sp.AverageResponseTime < participant.AverageResponseTime))
            .CountAsync() + 1;
    }

    public async Task<bool> UpdateParticipantRankings(Guid sessionId)
    {
        var participants = await _context.SessionParticipants
            .Where(sp => sp.SessionId == sessionId && sp.IsActive)
            .OrderByDescending(sp => sp.TotalScore)
            .ThenBy(sp => sp.AverageResponseTime)
            .ToListAsync();

        for (int i = 0; i < participants.Count; i++)
        {
            participants[i].Rank = i + 1;
            participants[i].UpdatedAt = DateTime.UtcNow;
        }

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateParticipantScore(Guid participantId, int points)
    {
        var participant = await _context.SessionParticipants.FindAsync(participantId);
        if (participant == null) return false;

        participant.TotalScore += points;
        participant.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateParticipantStats(Guid participantId)
    {
        var participant = await _context.SessionParticipants.FindAsync(participantId);
        if (participant == null) return false;

        var responses = await _context.StudentResponses
            .Where(sr => sr.SessionParticipantId == participantId)
            .ToListAsync();

        participant.TotalAnswered = responses.Count;
        participant.TotalCorrect = responses.Count(r => r.IsCorrect);
        participant.TotalScore = responses.Sum(r => r.PointsEarned);
        participant.AverageResponseTime = responses.Any()
            ? responses.Average(r => r.ResponseTimeSeconds)
            : 0;
        participant.UpdatedAt = DateTime.UtcNow;

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<int> GetActiveParticipantCount(Guid sessionId)
    {
        return await _context.SessionParticipants
            .CountAsync(sp => sp.SessionId == sessionId && sp.IsActive);
    }

    public async Task<bool> CheckParticipantExists(Guid participantId)
    {
        return await _context.SessionParticipants.AnyAsync(sp => sp.SessionParticipantId == participantId);
    }

    public async Task<bool> CheckParticipantBelongsToSession(Guid participantId, Guid sessionId)
    {
        return await _context.SessionParticipants
            .AnyAsync(sp => sp.SessionParticipantId == participantId && sp.SessionId == sessionId);
    }

    public async Task<bool> CheckParticipantKeyExistsInSession(Guid sessionId, string participantKey)
    {
        return await _context.SessionParticipants
            .AnyAsync(sp => sp.SessionId == sessionId && sp.ParticipantKey == participantKey);
    }

    public async Task<SessionParticipant?> GetParticipantBySessionAndParticipantKey(Guid sessionId, string participantKey)
    {
        return await _context.SessionParticipants
            .FirstOrDefaultAsync(sp => sp.SessionId == sessionId && sp.ParticipantKey == participantKey);
    }

    public async Task<bool> MarkParticipantAsLeft(Guid participantId)
    {
        var participant = await _context.SessionParticipants.FindAsync(participantId);
        if (participant == null) return false;

        participant.IsActive = false;
        participant.LeftAt = DateTime.UtcNow;
        participant.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> MarkAllParticipantsAsLeft(Guid sessionId)
    {
        var participants = await _context.SessionParticipants
            .Where(sp => sp.SessionId == sessionId && sp.IsActive)
            .ToListAsync();

        foreach (var participant in participants)
        {
            participant.IsActive = false;
            participant.LeftAt = DateTime.UtcNow;
            participant.UpdatedAt = DateTime.UtcNow;
        }

        return await _context.SaveChangesAsync() > 0;
    }
}