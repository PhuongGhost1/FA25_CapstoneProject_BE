using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Maps;

public interface IMapFeatureService
{
    Task<Option<MapFeatureResponse, Error>> Create(CreateMapFeatureRequest req);
    Task<Option<MapFeatureResponse, Error>> Update(Guid featureId, UpdateMapFeatureRequest req);
    Task<Option<bool, Error>> Delete(Guid featureId);
    Task<Option<List<MapFeatureResponse>, Error>> GetByMap(Guid mapId);
    Task<Option<List<MapFeatureResponse>, Error>> GetByMapAndCategory(Guid mapId, FeatureCategoryEnum category);
    Task<Option<List<MapFeatureResponse>, Error>> GetByMapAndLayer(Guid mapId, Guid layerId);
    Task<Option<bool, Error>> ApplySnapshot(Guid mapId, string snapshotJson);
}


