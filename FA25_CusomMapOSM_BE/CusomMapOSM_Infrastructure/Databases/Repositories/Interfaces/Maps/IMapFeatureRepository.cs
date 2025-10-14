using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Maps.Enums;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;

public interface IMapFeatureRepository
{
    Task<MapFeature?> GetById(Guid featureId);
    Task<List<MapFeature>> GetByMap(Guid mapId);
    Task<List<MapFeature>> GetByMapAndCategory(Guid mapId, FeatureCategoryEnum category);
    Task<List<MapFeature>> GetByMapAndLayer(Guid mapId, Guid layerId);
    Task<Guid> Create(MapFeature feature);
    Task<bool> Update(MapFeature feature);
    Task<bool> Delete(Guid featureId);
    Task<int> DeleteByMap(Guid mapId);
    Task<int> AddRange(IEnumerable<MapFeature> features);
}


