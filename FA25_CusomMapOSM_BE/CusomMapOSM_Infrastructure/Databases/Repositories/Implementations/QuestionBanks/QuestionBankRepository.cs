using CusomMapOSM_Domain.Entities.QuestionBanks;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.QuestionBanks;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.QuestionBanks;

public class QuestionBankRepository : IQuestionBankRepository
{
    private readonly CustomMapOSMDbContext _context;

    public QuestionBankRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateQuestionBank(QuestionBank questionBank)
    {
        _context.QuestionBanks.Add(questionBank);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<QuestionBank?> GetQuestionBankById(Guid questionBankId)
    {
        return await _context.QuestionBanks
            .Include(qb => qb.User)
            .Include(qb => qb.Workspace)
            .FirstOrDefaultAsync(qb => qb.QuestionBankId == questionBankId && qb.IsActive);
    }

    public async Task<List<QuestionBank>> GetQuestionBanksByUserId(Guid userId)
    {
        return await _context.QuestionBanks
            .Include(qb => qb.Workspace)
            .Where(qb => qb.UserId == userId && qb.IsActive)
            .OrderByDescending(qb => qb.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<QuestionBank>> GetQuestionBanksByWorkspaceId(Guid workspaceId)
    {
        return await _context.QuestionBanks
            .Include(qb => qb.User)
            .OrderByDescending(qb => qb.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<QuestionBank>> GetPublicQuestionBanks()
    {
        return await _context.QuestionBanks
            .Include(qb => qb.User)
            .Include(qb => qb.Workspace)
            .Where(qb => qb.IsPublic && qb.IsActive)
            .OrderByDescending(qb => qb.TotalQuestions)
            .ThenByDescending(qb => qb.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<QuestionBank>> GetQuestionBanksByCategory(string category)
    {
        return await _context.QuestionBanks
            .Include(qb => qb.User)
            .Where(qb => qb.Category == category && qb.IsActive)
            .OrderByDescending(qb => qb.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> UpdateQuestionBank(QuestionBank questionBank)
    {
        questionBank.UpdatedAt = DateTime.UtcNow;
        _context.QuestionBanks.Update(questionBank);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteQuestionBank(Guid questionBankId)
    {
        var questionBank = await _context.QuestionBanks.FindAsync(questionBankId);
        if (questionBank == null) return false;

        questionBank.IsActive = false;
        questionBank.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<QuestionBank>> GetQuestionBankTemplates()
    {
        return await _context.QuestionBanks
            .Include(qb => qb.User)
            .Where(qb => qb.IsTemplate && qb.IsActive)
            .OrderByDescending(qb => qb.CreatedAt)
            .ToListAsync();
    }

    public async Task<QuestionBank?> GetQuestionBankTemplateById(Guid templateId)
    {
        return await _context.QuestionBanks
            .Include(qb => qb.User)
            .FirstOrDefaultAsync(qb => qb.QuestionBankId == templateId && qb.IsTemplate && qb.IsActive);
    }

    public async Task<bool> CheckQuestionBankExists(Guid questionBankId)
    {
        return await _context.QuestionBanks
            .AnyAsync(qb => qb.QuestionBankId == questionBankId && qb.IsActive);
    }

    public async Task<bool> CheckUserOwnsQuestionBank(Guid questionBankId, Guid userId)
    {
        return await _context.QuestionBanks
            .AnyAsync(qb => qb.QuestionBankId == questionBankId && qb.UserId == userId && qb.IsActive);
    }

    public async Task<int> GetTotalQuestionCount(Guid questionBankId)
    {
        return await _context.Questions
            .CountAsync(q => q.QuestionBankId == questionBankId && q.IsActive);
    }

    public async Task<bool> UpdateQuestionCount(Guid questionBankId)
    {
        var questionBank = await _context.QuestionBanks.FindAsync(questionBankId);
        if (questionBank == null) return false;

        questionBank.TotalQuestions = await GetTotalQuestionCount(questionBankId);
        questionBank.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }
}