using CusomMapOSM_Domain.Entities.Maps;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;

public interface IMapLegendItemRepository
{
    Task<List<MapLegendItem>> GetByMapIdAsync(Guid mapId, CancellationToken ct = default);
    Task<MapLegendItem?> GetByIdAsync(Guid legendItemId, CancellationToken ct = default);
    Task<bool> CreateAsync(MapLegendItem item, CancellationToken ct = default);
    Task<bool> UpdateAsync(MapLegendItem item, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid legendItemId, CancellationToken ct = default);
    Task<bool> ReorderAsync(Guid mapId, List<Guid> itemIds, CancellationToken ct = default);
    Task<int> GetMaxDisplayOrderAsync(Guid mapId, CancellationToken ct = default);
}
