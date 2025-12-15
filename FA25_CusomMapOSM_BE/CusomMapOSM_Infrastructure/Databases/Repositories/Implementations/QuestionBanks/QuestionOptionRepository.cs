using CusomMapOSM_Domain.Entities.QuestionBanks;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.QuestionBanks;
using Microsoft.EntityFrameworkCore;


namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.QuestionBanks;

public class QuestionOptionRepository : IQuestionOptionRepository
{
    private readonly CustomMapOSMDbContext _context;

    public QuestionOptionRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }


    public async Task<bool> DeleteQuestionOption(Guid questionOptionId)
    {
        var questionOption = await _context.QuestionOptions.FindAsync(questionOptionId);
        if (questionOption == null)
        {
            return false;
        }

        _context.QuestionOptions.Remove(questionOption);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateQuestionOption(QuestionOption request)
    {
        var questionOption = await _context.QuestionOptions.FindAsync(request.QuestionOptionId);
        if (questionOption == null)
        {
            return false;
        }

        questionOption.OptionText = request.OptionText;
        questionOption.OptionImageUrl = request.OptionImageUrl;
        questionOption.IsCorrect = request.IsCorrect;
        questionOption.DisplayOrder = request.DisplayOrder;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CreateQuestionOption(QuestionOption request)
    {
        _context.QuestionOptions.Add(request);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CreateQuestionOptions(List<QuestionOption> options)
    {
        if (options == null || !options.Any())
        {
            return true; // Nothing to create
        }

        _context.QuestionOptions.AddRange(options);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<QuestionOption>> GetQuestionOptionsByQuestionId(Guid questionId)
    {
        return await _context.QuestionOptions.Where(qo => qo.QuestionId == questionId).ToListAsync();
    }
}
