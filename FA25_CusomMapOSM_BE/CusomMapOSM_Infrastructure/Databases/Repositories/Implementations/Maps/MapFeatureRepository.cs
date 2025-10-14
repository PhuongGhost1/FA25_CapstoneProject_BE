using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Infrastructure.Databases;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Maps;

public class MapFeatureRepository : IMapFeatureRepository
{
    private readonly CustomMapOSMDbContext _dbContext;

    public MapFeatureRepository(CustomMapOSMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MapFeature?> GetById(Guid featureId)
    {
        return await _dbContext.MapFeatures.AsNoTracking().FirstOrDefaultAsync(f => f.FeatureId == featureId);
    }

    public async Task<List<MapFeature>> GetByMap(Guid mapId)
    {
        return await _dbContext.MapFeatures
            .AsNoTracking()
            .Where(f => f.MapId == mapId)
            .OrderBy(f => f.ZIndex)
            .ThenBy(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<MapFeature>> GetByMapAndCategory(Guid mapId, FeatureCategoryEnum category)
    {
        return await _dbContext.MapFeatures
            .AsNoTracking()
            .Where(f => f.MapId == mapId && f.FeatureCategory == category)
            .OrderBy(f => f.ZIndex)
            .ThenBy(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<MapFeature>> GetByMapAndLayer(Guid mapId, Guid layerId)
    {
        return await _dbContext.MapFeatures
            .AsNoTracking()
            .Where(f => f.MapId == mapId && f.LayerId == layerId)
            .OrderBy(f => f.ZIndex)
            .ThenBy(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<Guid> Create(MapFeature feature)
    {
        feature.FeatureId = feature.FeatureId == Guid.Empty ? Guid.NewGuid() : feature.FeatureId;
        await _dbContext.MapFeatures.AddAsync(feature);
        await _dbContext.SaveChangesAsync();
        return feature.FeatureId;
    }

    public async Task<bool> Update(MapFeature feature)
    {
        _dbContext.MapFeatures.Update(feature);
        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> Delete(Guid featureId)
    {
        var existed = await _dbContext.MapFeatures.FirstOrDefaultAsync(f => f.FeatureId == featureId);
        if (existed == null) return false;
        _dbContext.MapFeatures.Remove(existed);
        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<int> DeleteByMap(Guid mapId)
    {
        var features = await _dbContext.MapFeatures.Where(f => f.MapId == mapId).ToListAsync();
        if (features.Count == 0) return 0;
        _dbContext.MapFeatures.RemoveRange(features);
        return await _dbContext.SaveChangesAsync();
    }

    public async Task<int> AddRange(IEnumerable<MapFeature> features)
    {
        await _dbContext.MapFeatures.AddRangeAsync(features);
        return await _dbContext.SaveChangesAsync();
    }
}


