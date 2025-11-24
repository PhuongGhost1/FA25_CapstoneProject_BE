using CusomMapOSM_Domain.Entities.QuestionBanks;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.QuestionBanks;

public interface IQuestionOptionRepository
{
    Task<bool> DeleteQuestionOption(Guid questionOptionId);
    Task<bool> UpdateQuestionOption(QuestionOption request);
    Task<bool> CreateQuestionOption(QuestionOption request);
    Task<bool> CreateQuestionOptions(List<QuestionOption> options);
    Task<List<QuestionOption>> GetQuestionOptionsByQuestionId(Guid questionId);
}