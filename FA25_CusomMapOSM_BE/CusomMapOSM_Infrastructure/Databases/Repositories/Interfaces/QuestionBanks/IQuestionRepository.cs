using CusomMapOSM_Domain.Entities.QuestionBanks;
using CusomMapOSM_Domain.Entities.QuestionBanks.Enums;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.QuestionBanks;

public interface IQuestionRepository
{
    // CRUD Operations
    Task<bool> CreateQuestion(Question question);
    Task<Question?> GetQuestionById(Guid questionId);
    Task<List<Question>> GetQuestionsByQuestionBankId(Guid? questionBankId);
    Task<List<Question>> GetQuestionsByType(QuestionTypeEnum questionType);
    Task<List<Question>> GetQuestionsByLocationId(Guid locationId);
    Task<bool> UpdateQuestion(Question question);
    Task<bool> DeleteQuestion(Guid questionId);

    // With Options
    Task<Question?> GetQuestionWithOptions(Guid questionId);
    Task<List<Question>> GetQuestionsWithOptions(Guid questionBankId);

    // Validation
    Task<bool> CheckQuestionExists(Guid questionId);
    Task<bool> CheckQuestionBelongsToBank(Guid questionId, Guid questionBankId);

    // Bulk Operations
    Task<bool> CreateQuestions(List<Question> questions);
    Task<bool> DeleteQuestionsByBankId(Guid questionBankId);
}