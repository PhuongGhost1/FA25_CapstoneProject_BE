using CusomMapOSM_Domain.Entities.QuestionBanks;
using CusomMapOSM_Domain.Entities.QuestionBanks.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.QuestionBanks;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.QuestionBanks;

public class QuestionRepository : IQuestionRepository
{
    private readonly CustomMapOSMDbContext _context;

    public QuestionRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateQuestion(Question question)
    {
        _context.Questions.Add(question);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Question?> GetQuestionById(Guid questionId)
    {
        return await _context.Questions
            .Include(q => q.QuestionBank)
            .Include(q => q.Location)
            .FirstOrDefaultAsync(q => q.QuestionId == questionId && q.IsActive);
    }

    public async Task<List<Question>> GetQuestionsByQuestionBankId(Guid questionBankId)
    {
        return await _context.Questions
            .Include(q => q.Location)
            .Where(q => q.QuestionBankId == questionBankId && q.IsActive)
            .OrderBy(q => q.DisplayOrder)
            .ThenBy(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Question>> GetQuestionsByType(QuestionTypeEnum questionType)
    {
        return await _context.Questions
            .Include(q => q.QuestionBank)
            .Where(q => q.QuestionType == questionType && q.IsActive)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Question>> GetQuestionsByLocationId(Guid locationId)
    {
        return await _context.Questions
            .Include(q => q.QuestionBank)
            .Where(q => q.LocationId == locationId && q.IsActive)
            .OrderBy(q => q.DisplayOrder)
            .ToListAsync();
    }

    public async Task<bool> UpdateQuestion(Question question)
    {
        question.UpdatedAt = DateTime.UtcNow;
        _context.Questions.Update(question);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteQuestion(Guid questionId)
    {
        var question = await _context.Questions.FindAsync(questionId);
        if (question == null) return false;

        question.IsActive = false;
        question.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Question?> GetQuestionWithOptions(Guid questionId)
    {
        return await _context.Questions
            .Include(q => q.QuestionBank)
            .Include(q => q.Location)
            .Include(q => q.QuestionOptions!.OrderBy(o => o.DisplayOrder))
            .FirstOrDefaultAsync(q => q.QuestionId == questionId && q.IsActive);
    }

    public async Task<List<Question>> GetQuestionsWithOptions(Guid questionBankId)
    {
        return await _context.Questions
            .Include(q => q.Location)
            .Include(q => q.QuestionOptions!.OrderBy(o => o.DisplayOrder))
            .Where(q => q.QuestionBankId == questionBankId && q.IsActive)
            .OrderBy(q => q.DisplayOrder)
            .ThenBy(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> CheckQuestionExists(Guid questionId)
    {
        return await _context.Questions
            .AnyAsync(q => q.QuestionId == questionId && q.IsActive);
    }

    public async Task<bool> CheckQuestionBelongsToBank(Guid questionId, Guid questionBankId)
    {
        return await _context.Questions
            .AnyAsync(q => q.QuestionId == questionId && q.QuestionBankId == questionBankId && q.IsActive);
    }

    public async Task<bool> CreateQuestions(List<Question> questions)
    {
        await _context.Questions.AddRangeAsync(questions);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteQuestionsByBankId(Guid questionBankId)
    {
        var questions = await _context.Questions
            .Where(q => q.QuestionBankId == questionBankId)
            .ToListAsync();

        foreach (var question in questions)
        {
            question.IsActive = false;
            question.UpdatedAt = DateTime.UtcNow;
        }

        return await _context.SaveChangesAsync() > 0;
    }
}