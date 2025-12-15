using CusomMapOSM_Domain.Entities.QuestionBanks;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.QuestionBanks;

public interface IQuestionBankRepository
{
    // CRUD Operations
    Task<bool> CreateQuestionBank(QuestionBank questionBank);
    Task<QuestionBank?> GetQuestionBankById(Guid questionBankId);
    Task<List<QuestionBank>> GetQuestionBanksByUserId(Guid userId);
    Task<List<QuestionBank>> GetQuestionBanksByWorkspaceId(Guid workspaceId);
    Task<List<QuestionBank>> GetPublicQuestionBanks();
    Task<List<QuestionBank>> GetQuestionBanksByCategory(string category);
    Task<bool> UpdateQuestionBank(QuestionBank questionBank);
    Task<bool> DeleteQuestionBank(Guid questionBankId);

    // Organization-based filtering
    Task<List<QuestionBank>> GetQuestionBanksByUserIdAndWorkspaceIds(Guid userId, List<Guid> workspaceIds);
    Task<List<QuestionBank>> GetPublicQuestionBanksByWorkspaceIds(List<Guid> workspaceIds);

    // Templates
    Task<List<QuestionBank>> GetQuestionBankTemplates();
    Task<QuestionBank?> GetQuestionBankTemplateById(Guid templateId);

    // Validation
    Task<bool> CheckQuestionBankExists(Guid questionBankId);
    Task<bool> CheckUserOwnsQuestionBank(Guid questionBankId, Guid userId);

    // Statistics
    Task<int> GetTotalQuestionCount(Guid questionBankId);
    Task<bool> UpdateQuestionCount(Guid questionBankId);
}