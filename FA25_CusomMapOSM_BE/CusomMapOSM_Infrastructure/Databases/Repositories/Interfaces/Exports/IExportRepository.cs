using CusomMapOSM_Domain.Entities.Exports;
using CusomMapOSM_Domain.Entities.Exports.Enums;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Exports;

public interface IExportRepository
{
    Task<Export?> GetByIdAsync(int exportId);
    Task<Export?> GetByIdWithIncludesAsync(int exportId);
    Task<List<Export>> GetByUserIdAsync(Guid userId);
    Task<List<Export>> GetByMapIdAsync(Guid mapId);
    Task<List<Export>> GetPendingExportsAsync();
    Task<List<Export>> GetPendingApprovalExportsAsync();
    Task<Export> CreateAsync(Export export);
    Task<bool> UpdateAsync(Export export);
    Task<bool> DeleteAsync(int exportId);
}

