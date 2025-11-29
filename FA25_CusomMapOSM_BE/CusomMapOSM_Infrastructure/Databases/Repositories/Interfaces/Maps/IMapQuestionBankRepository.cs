using CusomMapOSM_Domain.Entities.QuestionBanks;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;

public interface IMapQuestionBankRepository
{
    Task<bool> AddQuestionBank(MapQuestionBank mapQuestionBank, CancellationToken ct = default);
    Task<bool> RemoveQuestionBank(MapQuestionBank mapQuestionBank, CancellationToken ct = default);
    Task<bool> UpdateQuestionBank(MapQuestionBank mapQuestionBank, CancellationToken ct = default);
    Task<MapQuestionBank?> GetQuestionBank(Guid mapId, CancellationToken ct = default);
    Task<List<MapQuestionBank>> GetQuestionBanks(Guid mapId, CancellationToken ct = default);
    Task<List<MapQuestionBank>> GetMaps(Guid questionBankId, CancellationToken ct = default);
}