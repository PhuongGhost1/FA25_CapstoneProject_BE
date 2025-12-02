using CusomMapOSM_Domain.Entities.Sessions;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Sessions;

public class SessionQuestionBankRepository : ISessionQuestionBankRepository
{
    private readonly CustomMapOSMDbContext _context;

    public SessionQuestionBankRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<List<SessionQuestionBank>> GetQuestionBanks(Guid sessionId)
    {
        return await _context.SessionQuestionBanks
            .Include(sqb => sqb.QuestionBank)
            .Where(sqb => sqb.SessionId == sessionId)
            .ToListAsync();
    }

    public async Task<List<SessionQuestionBank>> GetSessions(Guid questionBankId)
    {
        return await _context.SessionQuestionBanks
            .Include(sqb => sqb.Session)
            .Where(sqb => sqb.QuestionBankId == questionBankId)
            .ToListAsync();
    }

    public async Task<bool> AddQuestionBank(SessionQuestionBank sessionQuestionBank)
    {
        _context.SessionQuestionBanks.Add(sessionQuestionBank);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> RemoveQuestionBank(SessionQuestionBank sessionQuestionBank)
    {
        _context.SessionQuestionBanks.Remove(sessionQuestionBank);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> CheckQuestionBankAttached(Guid sessionId, Guid questionBankId)
    {
        return await _context.SessionQuestionBanks
            .AnyAsync(sqb => sqb.SessionId == sessionId && sqb.QuestionBankId == questionBankId);
    }
}

