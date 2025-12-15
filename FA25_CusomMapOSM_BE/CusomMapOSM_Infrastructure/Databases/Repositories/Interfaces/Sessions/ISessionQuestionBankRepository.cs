using CusomMapOSM_Domain.Entities.Sessions;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;

public interface ISessionQuestionBankRepository
{
    Task<List<SessionQuestionBank>> GetQuestionBanks(Guid sessionId);
    Task<List<SessionQuestionBank>> GetSessions(Guid questionBankId);
    Task<bool> AddQuestionBank(SessionQuestionBank sessionQuestionBank);
    Task<bool> RemoveQuestionBank(SessionQuestionBank sessionQuestionBank);
    Task<bool> CheckQuestionBankAttached(Guid sessionId, Guid questionBankId);
    Task<List<Guid>> GetSessionsWithQuestionBanks(List<Guid> sessionIds);
}

