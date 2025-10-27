using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Common;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Interfaces.Services.MapFeatures;
using CusomMapOSM_Application.Models.Documents;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using Optional;
using System.Text.Json;

namespace CusomMapOSM_Infrastructure.Features.Maps;

public class MapFeatureService : IMapFeatureService
{
    private readonly IMapFeatureRepository _repository;
    private readonly IMapFeatureStore _mongoStore;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapHistoryService _mapHistoryService;

    public MapFeatureService(
        IMapFeatureRepository repository,
        IMapFeatureStore mongoStore,
        ICurrentUserService currentUserService,
        IMapHistoryService mapHistoryService)
    {
        _repository = repository;
        _mongoStore = mongoStore;
        _currentUserService = currentUserService;
        _mapHistoryService = mapHistoryService;
    }

    public async Task<Option<MapFeatureResponse, Error>> Create(CreateMapFeatureRequest req)
    {
        var userId = _currentUserService.GetUserId();
        if (userId == null)
        {
            return Option.None<MapFeatureResponse, Error>(Error.Unauthorized("Feature.Unauthorized", "Unauthorized"));
        }

        if (!IsGeometryValidForRequest(req.FeatureCategory, req.AnnotationType, req.GeometryType))
        {
            return Option.None<MapFeatureResponse, Error>(Error.ValidationError("Feature.InvalidGeometry", "Invalid geometry for tool"));
        }

        var featureId = Guid.NewGuid();
        
        // Validate and normalize geometry coordinates
        var normalizedCoordinates = GeometryValidator.ValidateAndNormalizeGeometry(
            req.Coordinates, 
            req.GeometryType.ToString()
        );
        
        var mongoDoc = new MapFeatureDocument
        {
            Id = featureId.ToString(),
            MapId = req.MapId,
            LayerId = req.LayerId,
            Name = req.Name,
            FeatureCategory = req.FeatureCategory.ToString(),
            AnnotationType = req.AnnotationType?.ToString(),
            GeometryType = req.GeometryType.ToString(),
            Geometry = normalizedCoordinates,
            Properties = string.IsNullOrEmpty(req.Properties) 
                ? null 
                : JsonSerializer.Deserialize<Dictionary<string, object>>(req.Properties),
            Style = string.IsNullOrEmpty(req.Style) 
                ? null 
                : JsonSerializer.Deserialize<Dictionary<string, object>>(req.Style),
            CreatedBy = userId.Value,
            CreatedAt = DateTime.UtcNow,
            IsVisible = req.IsVisible ?? true,
            ZIndex = req.ZIndex ?? 0
        };
        
        var mongoDocId = await _mongoStore.CreateAsync(mongoDoc);
        
        var entity = new MapFeature
        {
            FeatureId = featureId,
            MapId = req.MapId,
            LayerId = req.LayerId,
            Name = req.Name,
            Description = req.Description,
            FeatureCategory = req.FeatureCategory,
            AnnotationType = req.AnnotationType,
            GeometryType = req.GeometryType,
            MongoDocumentId = mongoDocId,
            CreatedBy = userId.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsVisible = req.IsVisible ?? true,
            ZIndex = req.ZIndex ?? 0
        };

        var id = await _repository.Create(entity);
        
        var snapshot = await _repository.GetByMap(req.MapId);
        await _mapHistoryService.RecordSnapshot(req.MapId, userId.Value, JsonSerializer.Serialize(snapshot));
        
        var created = await _repository.GetById(id);
        if (created == null)
        {
            return Option.None<MapFeatureResponse, Error>(Error.Failure("Feature.CreateFailed", "Create failed"));
        }
        
        var mongoData = await _mongoStore.GetAsync(featureId);
        return Option.Some<MapFeatureResponse, Error>(ToResponse(created, mongoData));
    }

    public async Task<Option<MapFeatureResponse, Error>> Update(Guid featureId, UpdateMapFeatureRequest req)
    {
        var existed = await _repository.GetById(featureId);
        if (existed == null)
        {
            return Option.None<MapFeatureResponse, Error>(Error.NotFound("Feature.NotFound", "Feature not found"));
        }

        if (req.FeatureCategory.HasValue || req.AnnotationType.HasValue || req.GeometryType.HasValue)
        {
            var category = req.FeatureCategory ?? existed.FeatureCategory;
            var anno = req.AnnotationType ?? existed.AnnotationType;
            var geom = req.GeometryType ?? existed.GeometryType;
            if (!IsGeometryValidForRequest(category, anno, geom))
            {
                return Option.None<MapFeatureResponse, Error>(Error.ValidationError("Feature.InvalidGeometry", "Invalid geometry for tool"));
            }
        }

        // Update metadata
        existed.Name = req.Name ?? existed.Name;
        existed.Description = req.Description ?? existed.Description;
        existed.FeatureCategory = req.FeatureCategory ?? existed.FeatureCategory;
        existed.AnnotationType = req.AnnotationType ?? existed.AnnotationType;
        existed.GeometryType = req.GeometryType ?? existed.GeometryType;
        existed.IsVisible = req.IsVisible ?? existed.IsVisible;
        existed.ZIndex = req.ZIndex ?? existed.ZIndex;
        existed.LayerId = req.LayerId ?? existed.LayerId;
        existed.UpdatedAt = DateTime.UtcNow;

        if (req.Coordinates != null || req.Properties != null || req.Style != null)
        {
            var mongoDoc = await _mongoStore.GetAsync(featureId);
            if (mongoDoc != null)
            {
                if (req.Coordinates != null)
                {
                    // Validate and normalize geometry coordinates
                    var normalizedCoordinates = GeometryValidator.ValidateAndNormalizeGeometry(
                        req.Coordinates, 
                        existed.GeometryType.ToString()
                    );
                    mongoDoc.Geometry = normalizedCoordinates;
                }
                if (req.Properties != null)
                    mongoDoc.Properties = JsonSerializer.Deserialize<Dictionary<string, object>>(req.Properties);
                if (req.Style != null)
                    mongoDoc.Style = JsonSerializer.Deserialize<Dictionary<string, object>>(req.Style);
                    
                mongoDoc.UpdatedAt = DateTime.UtcNow;
                await _mongoStore.UpdateAsync(mongoDoc);
            }
        }

        var ok = await _repository.Update(existed);
        if (ok)
        {
            var snapshot = await _repository.GetByMap(existed.MapId);
            await _mapHistoryService.RecordSnapshot(existed.MapId, _currentUserService.GetUserId()!.Value, JsonSerializer.Serialize(snapshot));
        }
        
        var mongoData = await _mongoStore.GetAsync(featureId);
        return ok
            ? Option.Some<MapFeatureResponse, Error>(ToResponse(existed, mongoData))
            : Option.None<MapFeatureResponse, Error>(Error.Failure("Feature.UpdateFailed", "Update failed"));
    }

    public async Task<Option<bool, Error>> Delete(Guid featureId)
    {
        var existed = await _repository.GetById(featureId);
        if (existed == null)
        {
            return Option.None<bool, Error>(Error.NotFound("Feature.NotFound", "Feature not found"));
        }
        
        await _mongoStore.DeleteAsync(featureId);
        
        var ok = await _repository.Delete(featureId);
        if (ok)
        {
            var snapshot = await _repository.GetByMap(existed.MapId);
            var uid = _currentUserService.GetUserId();
            if (uid != null)
                await _mapHistoryService.RecordSnapshot(existed.MapId, uid.Value, JsonSerializer.Serialize(snapshot));
        }
        return ok
            ? Option.Some<bool, Error>(true)
            : Option.None<bool, Error>(Error.Failure("Feature.DeleteFailed", "Delete failed"));
    }

    public async Task<Option<List<MapFeatureResponse>, Error>> GetByMap(Guid mapId)
    {
        var metadataList = await _repository.GetByMap(mapId);
        
        var mongoDataList = await _mongoStore.GetByMapAsync(mapId);
        var mongoDataDict = mongoDataList.ToDictionary(d => Guid.Parse(d.Id));
        
        var result = metadataList.Select(metadata =>
        {
            mongoDataDict.TryGetValue(metadata.FeatureId, out var mongoData);
            return ToResponse(metadata, mongoData);
        }).ToList();
        
        return Option.Some<List<MapFeatureResponse>, Error>(result);
    }

    public async Task<Option<List<MapFeatureResponse>, Error>> GetByMapAndCategory(Guid mapId, FeatureCategoryEnum category)
    {
        var list = await _repository.GetByMapAndCategory(mapId, category);
        return Option.Some<List<MapFeatureResponse>, Error>(list.Select(f => ToResponse(f, null)).ToList());
    }

    public async Task<Option<List<MapFeatureResponse>, Error>> GetByMapAndLayer(Guid mapId, Guid layerId)
    {
        var list = await _repository.GetByMapAndLayer(mapId, layerId);
        return Option.Some<List<MapFeatureResponse>, Error>(list.Select(f => ToResponse(f, null)).ToList());
    }

    private static MapFeatureResponse ToResponse(MapFeature f, MapFeatureDocument? mongoData = null)
    {
        var coordinates = mongoData?.Geometry?.ToString() ?? string.Empty;
        var properties = mongoData?.Properties != null 
            ? JsonSerializer.Serialize(mongoData.Properties) 
            : null;
        var style = mongoData?.Style != null 
            ? JsonSerializer.Serialize(mongoData.Style) 
            : null;
            
        return new MapFeatureResponse
        {
            FeatureId = f.FeatureId,
            MapId = f.MapId,
            LayerId = f.LayerId,
            Name = f.Name,
            Description = f.Description,
            FeatureCategory = f.FeatureCategory,
            AnnotationType = f.AnnotationType,
            GeometryType = f.GeometryType,
            Coordinates = coordinates,
            Properties = properties,
            Style = style,
            IsVisible = f.IsVisible,
            ZIndex = f.ZIndex,
            CreatedBy = f.CreatedBy,
            CreatedAt = f.CreatedAt,
            UpdatedAt = f.UpdatedAt
        };
    }

    public async Task<Option<bool, Error>> ApplySnapshot(Guid mapId, string snapshotJson)
    {
        try
        {
            var currentUser = _currentUserService.GetUserId();
            if (currentUser == null)
            {
                return Option.None<bool, Error>(Error.Unauthorized("Feature.Unauthorized", "Unauthorized"));
            }

            var features = JsonSerializer.Deserialize<List<MapFeature>>(snapshotJson);
            if (features == null)
            {
                return Option.None<bool, Error>(Error.ValidationError("Feature.InvalidSnapshot", "Snapshot parse failed"));
            }

            foreach (var f in features)
            {
                f.MapId = mapId;
                if (f.FeatureId == Guid.Empty) f.FeatureId = Guid.NewGuid();
            }

            await _repository.DeleteByMap(mapId);
            await _repository.AddRange(features);

            var appliedSnapshotJson = JsonSerializer.Serialize(features);
            await _mapHistoryService.RecordSnapshot(mapId, currentUser.Value, appliedSnapshotJson);

            return Option.Some<bool, Error>(true);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("Feature.ApplySnapshotFailed", ex.Message));
        }
    }

    private static bool IsGeometryValidForRequest(FeatureCategoryEnum category, AnnotationTypeEnum? annotationType, GeometryTypeEnum geometryType)
    {
        if (category == FeatureCategoryEnum.Data)
        {
            return geometryType == GeometryTypeEnum.Point
                   || geometryType == GeometryTypeEnum.LineString
                   || geometryType == GeometryTypeEnum.Polygon
                   || geometryType == GeometryTypeEnum.Circle;
        }

        if (annotationType == null) return false;
        return annotationType switch
        {
            AnnotationTypeEnum.Marker => geometryType == GeometryTypeEnum.Point || geometryType == GeometryTypeEnum.Circle,
            AnnotationTypeEnum.Highlighter => geometryType == GeometryTypeEnum.LineString || geometryType == GeometryTypeEnum.Polygon,
            AnnotationTypeEnum.Text => geometryType == GeometryTypeEnum.Point,
            AnnotationTypeEnum.Note => geometryType == GeometryTypeEnum.Point,
            AnnotationTypeEnum.Link => geometryType == GeometryTypeEnum.Point,
            AnnotationTypeEnum.Video => geometryType == GeometryTypeEnum.Point,
            _ => false
        };
    }
}


