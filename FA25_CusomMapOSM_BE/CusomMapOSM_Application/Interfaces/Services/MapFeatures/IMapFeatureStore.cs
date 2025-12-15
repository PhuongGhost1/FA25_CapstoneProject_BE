using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CusomMapOSM_Application.Models.Documents;

namespace CusomMapOSM_Application.Interfaces.Services.MapFeatures;

public interface IMapFeatureStore
{
    Task<MapFeatureDocument?> GetAsync(Guid featureId, CancellationToken ct = default);
    Task<List<MapFeatureDocument>> GetByMapAsync(Guid mapId, CancellationToken ct = default);
    Task<List<MapFeatureDocument>> GetByLayerAsync(Guid mapId, Guid layerId, CancellationToken ct = default);
    Task<List<MapFeatureDocument>> GetByCategoryAsync(Guid mapId, string category, CancellationToken ct = default);
    Task<List<MapFeatureDocument>> GetByBoundsAsync(Guid mapId, double[] bbox, CancellationToken ct = default);
    Task<string> CreateAsync(MapFeatureDocument document, CancellationToken ct = default);
    Task UpdateAsync(MapFeatureDocument document, CancellationToken ct = default);
    Task DeleteAsync(Guid featureId, CancellationToken ct = default);
    Task DeleteByMapAsync(Guid mapId, CancellationToken ct = default);
}
