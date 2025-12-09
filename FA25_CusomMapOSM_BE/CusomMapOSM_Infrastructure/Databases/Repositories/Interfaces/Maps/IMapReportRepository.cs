using CusomMapOSM_Domain.Entities.Maps;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;

public interface IMapReportRepository
{
    Task<MapReport?> GetReportByIdAsync(Guid reportId);
    Task<List<MapReport>> GetReportsByMapIdAsync(Guid mapId);
    Task<List<MapReport>> GetAllReportsAsync(int page = 1, int pageSize = 20);
    Task<List<MapReport>> GetReportsByStatusAsync(int status, int page = 1, int pageSize = 20);
    Task<bool> CreateReportAsync(MapReport report);
    Task<bool> UpdateReportAsync(MapReport report);
    Task<int> GetReportsCountAsync();
    Task<int> GetPendingReportsCountAsync();
}

