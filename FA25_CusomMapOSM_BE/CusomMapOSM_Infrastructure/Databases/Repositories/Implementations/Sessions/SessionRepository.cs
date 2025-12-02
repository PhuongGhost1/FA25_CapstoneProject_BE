using CusomMapOSM_Domain.Entities.Sessions;
using CusomMapOSM_Domain.Entities.Sessions.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Sessions;

public class SessionRepository : ISessionRepository
{
    private readonly CustomMapOSMDbContext _context;

    public SessionRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateSession(Session session)
    {
        _context.Sessions.Add(session);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Session?> GetSessionById(Guid sessionId)
    {
        return await _context.Sessions
            .Include(s => s.Map)
            .Include(s => s.HostUser)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);
    }

    public async Task<Session?> GetSessionByCode(string sessionCode)
    {
        return await _context.Sessions
            .Include(s => s.Map)
            .Include(s => s.HostUser)
            .FirstOrDefaultAsync(s => s.SessionCode == sessionCode);
    }

    public async Task<List<Session>> GetSessionsByHostUserId(Guid hostUserId)
    {
        return await _context.Sessions
            .Include(s => s.Map)
            .Where(s => s.HostUserId == hostUserId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Session>> GetSessionsByMapId(Guid mapId)
    {
        return await _context.Sessions
            .Include(s => s.HostUser)
            .Where(s => s.MapId == mapId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Session>> GetSessionsByStatus(SessionStatusEnum status)
    {
        return await _context.Sessions
            .Include(s => s.Map)
            .Include(s => s.HostUser)
            .Where(s => s.Status == status)
            .OrderByDescending(s => s.ActualStartTime ?? s.ScheduledStartTime ?? s.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> UpdateSession(Session session)
    {
        session.UpdatedAt = DateTime.UtcNow;
        _context.Sessions.Update(session);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteSession(Guid sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session == null) return false;

        _context.Sessions.Remove(session);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Session?> GetSessionWithQuestions(Guid sessionId)
    {
        return await _context.Sessions
            .Include(s => s.Map)
            .Include(s => s.HostUser)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);
    }

    public async Task<Session?> GetSessionWithParticipants(Guid sessionId)
    {
        return await _context.Sessions
            .Include(s => s.Map)
            .Include(s => s.HostUser)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);
    }

    public async Task<Session?> GetSessionWithFullDetails(Guid sessionId)
    {
        return await _context.Sessions
            .Include(s => s.Map)
            .Include(s => s.HostUser)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);
    }

    public async Task<string> GenerateUniqueSessionCode()
    {
        string code;
        bool exists;
        var random = new Random();

        do
        {
            code = random.Next(100000, 999999).ToString();
            exists = await CheckSessionCodeExists(code);
        } while (exists);

        return code;
    }

    public async Task<bool> CheckSessionCodeExists(string sessionCode)
    {
        return await _context.Sessions.AnyAsync(s => s.SessionCode == sessionCode);
    }

    public async Task<bool> CheckSessionExists(Guid sessionId)
    {
        return await _context.Sessions.AnyAsync(s => s.SessionId == sessionId);
    }

    public async Task<bool> CheckUserIsHost(Guid sessionId, Guid userId)
    {
        return await _context.Sessions
            .AnyAsync(s => s.SessionId == sessionId && s.HostUserId == userId);
    }

    public async Task<bool> UpdateSessionStatus(Guid sessionId, SessionStatusEnum status)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session == null) return false;

        session.Status = status;
        session.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> StartSession(Guid sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session == null) return false;

        session.Status = SessionStatusEnum.IN_PROGRESS;
        session.ActualStartTime = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> EndSession(Guid sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session == null) return false;

        session.Status = SessionStatusEnum.COMPLETED;
        session.EndTime = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> PauseSession(Guid sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session == null) return false;

        session.Status = SessionStatusEnum.PAUSED;
        session.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ResumeSession(Guid sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session == null) return false;

        session.Status = SessionStatusEnum.IN_PROGRESS;
        session.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateParticipantCount(Guid sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session == null) return false;

        session.TotalParticipants = await _context.SessionParticipants
            .CountAsync(sp => sp.SessionId == sessionId && sp.IsActive);
        session.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateResponseCount(Guid sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session == null) return false;

        session.TotalResponses = await _context.StudentResponses
            .CountAsync(sr => sr.SessionQuestion!.SessionId == sessionId);
        session.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }
}