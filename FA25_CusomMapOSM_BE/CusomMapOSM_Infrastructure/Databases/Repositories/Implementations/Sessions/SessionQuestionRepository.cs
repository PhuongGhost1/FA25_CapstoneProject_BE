using CusomMapOSM_Domain.Entities.Sessions;
using CusomMapOSM_Domain.Entities.Sessions.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Sessions;

public class SessionQuestionRepository : ISessionQuestionRepository
{
    private readonly CustomMapOSMDbContext _context;

    public SessionQuestionRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateSessionQuestion(SessionQuestion sessionQuestion)
    {
        _context.SessionQuestions.Add(sessionQuestion);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<SessionQuestion?> GetSessionQuestionById(Guid sessionQuestionId)
    {
        return await _context.SessionQuestions
            .Include(sq => sq.Session)
            .Include(sq => sq.Question)
                .ThenInclude(q => q!.QuestionOptions!.OrderBy(o => o.DisplayOrder))
            .FirstOrDefaultAsync(sq => sq.SessionQuestionId == sessionQuestionId);
    }

    public async Task<List<SessionQuestion>> GetQuestionsBySessionId(Guid sessionId)
    {
        return await _context.SessionQuestions
            .Include(sq => sq.Question)
                .ThenInclude(q => q!.QuestionOptions)
            .Where(sq => sq.SessionId == sessionId)
            .OrderBy(sq => sq.QueueOrder)
            .ToListAsync();
    }

    public async Task<SessionQuestion?> GetActiveQuestion(Guid sessionId)
    {
        return await _context.SessionQuestions
            .Include(sq => sq.Question)
                .ThenInclude(q => q!.QuestionOptions!.OrderBy(o => o.DisplayOrder))
            .FirstOrDefaultAsync(sq => sq.SessionId == sessionId && sq.Status == SessionQuestionStatusEnum.ACTIVE);
    }

    public async Task<SessionQuestion?> GetNextQueuedQuestion(Guid sessionId)
    {
        return await _context.SessionQuestions
            .Include(sq => sq.Question)
                .ThenInclude(q => q!.QuestionOptions)
            .Where(sq => sq.SessionId == sessionId && sq.Status == SessionQuestionStatusEnum.QUEUED)
            .OrderBy(sq => sq.QueueOrder)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateSessionQuestion(SessionQuestion sessionQuestion)
    {
        sessionQuestion.UpdatedAt = DateTime.UtcNow;
        _context.SessionQuestions.Update(sessionQuestion);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteSessionQuestion(Guid sessionQuestionId)
    {
        var sessionQuestion = await _context.SessionQuestions.FindAsync(sessionQuestionId);
        if (sessionQuestion == null) return false;

        _context.SessionQuestions.Remove(sessionQuestion);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateQuestionStatus(Guid sessionQuestionId, SessionQuestionStatusEnum status)
    {
        var sessionQuestion = await _context.SessionQuestions.FindAsync(sessionQuestionId);
        if (sessionQuestion == null) return false;

        sessionQuestion.Status = status;
        sessionQuestion.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ActivateQuestion(Guid sessionQuestionId)
    {
        var sessionQuestion = await _context.SessionQuestions.FindAsync(sessionQuestionId);
        if (sessionQuestion == null) return false;

        sessionQuestion.Status = SessionQuestionStatusEnum.ACTIVE;
        sessionQuestion.StartedAt = DateTime.UtcNow;
        sessionQuestion.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> CompleteQuestion(Guid sessionQuestionId)
    {
        var sessionQuestion = await _context.SessionQuestions.FindAsync(sessionQuestionId);
        if (sessionQuestion == null) return false;

        sessionQuestion.Status = SessionQuestionStatusEnum.COMPLETED;
        sessionQuestion.EndedAt = DateTime.UtcNow;
        sessionQuestion.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> SkipQuestion(Guid sessionQuestionId)
    {
        var sessionQuestion = await _context.SessionQuestions.FindAsync(sessionQuestionId);
        if (sessionQuestion == null) return false;

        sessionQuestion.Status = SessionQuestionStatusEnum.SKIPPED;
        sessionQuestion.EndedAt = DateTime.UtcNow;
        sessionQuestion.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExtendTimeLimit(Guid sessionQuestionId, int additionalSeconds)
    {
        var sessionQuestion = await _context.SessionQuestions.FindAsync(sessionQuestionId);
        if (sessionQuestion == null) return false;

        sessionQuestion.TimeLimitOverride = (sessionQuestion.TimeLimitOverride ?? sessionQuestion.Question!.TimeLimit) + additionalSeconds;
        sessionQuestion.TimeLimitExtensions++;
        sessionQuestion.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateStartTime(Guid sessionQuestionId)
    {
        var sessionQuestion = await _context.SessionQuestions.FindAsync(sessionQuestionId);
        if (sessionQuestion == null) return false;

        sessionQuestion.StartedAt = DateTime.UtcNow;
        sessionQuestion.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateEndTime(Guid sessionQuestionId)
    {
        var sessionQuestion = await _context.SessionQuestions.FindAsync(sessionQuestionId);
        if (sessionQuestion == null) return false;

        sessionQuestion.EndedAt = DateTime.UtcNow;
        sessionQuestion.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateResponseStats(Guid sessionQuestionId)
    {
        var sessionQuestion = await _context.SessionQuestions.FindAsync(sessionQuestionId);
        if (sessionQuestion == null) return false;

        var responses = await _context.StudentResponses
            .Where(sr => sr.SessionQuestionId == sessionQuestionId)
            .ToListAsync();

        sessionQuestion.TotalResponses = responses.Count;
        sessionQuestion.CorrectResponses = responses.Count(r => r.IsCorrect);
        sessionQuestion.UpdatedAt = DateTime.UtcNow;

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> IncrementResponseCount(Guid sessionQuestionId, bool isCorrect)
    {
        var sessionQuestion = await _context.SessionQuestions.FindAsync(sessionQuestionId);
        if (sessionQuestion == null) return false;

        sessionQuestion.TotalResponses++;
        if (isCorrect) sessionQuestion.CorrectResponses++;
        sessionQuestion.UpdatedAt = DateTime.UtcNow;

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<int> GetTotalQuestionsInSession(Guid sessionId)
    {
        return await _context.SessionQuestions
            .CountAsync(sq => sq.SessionId == sessionId);
    }

    public async Task<bool> CheckSessionQuestionExists(Guid sessionQuestionId)
    {
        return await _context.SessionQuestions.AnyAsync(sq => sq.SessionQuestionId == sessionQuestionId);
    }

    public async Task<bool> CheckQuestionBelongsToSession(Guid sessionQuestionId, Guid sessionId)
    {
        return await _context.SessionQuestions
            .AnyAsync(sq => sq.SessionQuestionId == sessionQuestionId && sq.SessionId == sessionId);
    }

    public async Task<bool> CreateSessionQuestions(List<SessionQuestion> sessionQuestions)
    {
        await _context.SessionQuestions.AddRangeAsync(sessionQuestions);
        return await _context.SaveChangesAsync() > 0;
    }
}