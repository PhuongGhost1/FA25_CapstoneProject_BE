using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.Maps;

public class MapFeatureService : IMapFeatureService
{
    private readonly IMapFeatureRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public MapFeatureService(IMapFeatureRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Option<MapFeatureResponse, Error>> Create(CreateMapFeatureRequest req)
    {
        var userId = _currentUserService.GetUserId();
        if (userId == null)
        {
            return Option.None<MapFeatureResponse, Error>(Error.Unauthorized("Feature.Unauthorized", "Unauthorized"));
        }

        // Basic validation of tool-to-geometry mapping
        if (!IsGeometryValidForRequest(req.FeatureCategory, req.AnnotationType, req.GeometryType))
        {
            return Option.None<MapFeatureResponse, Error>(Error.ValidationError("Feature.InvalidGeometry", "Invalid geometry for tool"));
        }

        var entity = new MapFeature
        {
            FeatureId = Guid.NewGuid(),
            MapId = req.MapId,
            LayerId = req.LayerId,
            Name = req.Name,
            Description = req.Description,
            FeatureCategory = req.FeatureCategory,
            AnnotationType = req.AnnotationType,
            GeometryType = req.GeometryType,
            Coordinates = req.Coordinates,
            Properties = req.Properties,
            Style = req.Style,
            CreatedBy = userId.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsVisible = req.IsVisible ?? true,
            ZIndex = req.ZIndex ?? 0
        };

        var id = await _repository.Create(entity);
        var created = await _repository.GetById(id);
        return created == null
            ? Option.None<MapFeatureResponse, Error>(Error.Failure("Feature.CreateFailed", "Create failed"))
            : Option.Some<MapFeatureResponse, Error>(ToResponse(created));
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

        existed.Name = req.Name ?? existed.Name;
        existed.Description = req.Description ?? existed.Description;
        existed.FeatureCategory = req.FeatureCategory ?? existed.FeatureCategory;
        existed.AnnotationType = req.AnnotationType ?? existed.AnnotationType;
        existed.GeometryType = req.GeometryType ?? existed.GeometryType;
        existed.Coordinates = req.Coordinates ?? existed.Coordinates;
        existed.Properties = req.Properties ?? existed.Properties;
        existed.Style = req.Style ?? existed.Style;
        existed.IsVisible = req.IsVisible ?? existed.IsVisible;
        existed.ZIndex = req.ZIndex ?? existed.ZIndex;
        existed.LayerId = req.LayerId ?? existed.LayerId;
        existed.UpdatedAt = DateTime.UtcNow;

        var ok = await _repository.Update(existed);
        return ok
            ? Option.Some<MapFeatureResponse, Error>(ToResponse(existed))
            : Option.None<MapFeatureResponse, Error>(Error.Failure("Feature.UpdateFailed", "Update failed"));
    }

    public async Task<Option<bool, Error>> Delete(Guid featureId)
    {
        var ok = await _repository.Delete(featureId);
        return ok
            ? Option.Some<bool, Error>(true)
            : Option.None<bool, Error>(Error.NotFound("Feature.NotFound", "Feature not found"));
    }

    public async Task<Option<List<MapFeatureResponse>, Error>> GetByMap(Guid mapId)
    {
        var list = await _repository.GetByMap(mapId);
        return Option.Some<List<MapFeatureResponse>, Error>(list.Select(ToResponse).ToList());
    }

    public async Task<Option<List<MapFeatureResponse>, Error>> GetByMapAndCategory(Guid mapId, FeatureCategoryEnum category)
    {
        var list = await _repository.GetByMapAndCategory(mapId, category);
        return Option.Some<List<MapFeatureResponse>, Error>(list.Select(ToResponse).ToList());
    }

    public async Task<Option<List<MapFeatureResponse>, Error>> GetByMapAndLayer(Guid mapId, Guid layerId)
    {
        var list = await _repository.GetByMapAndLayer(mapId, layerId);
        return Option.Some<List<MapFeatureResponse>, Error>(list.Select(ToResponse).ToList());
    }

    private static MapFeatureResponse ToResponse(MapFeature f)
    {
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
            Coordinates = f.Coordinates,
            Properties = f.Properties,
            Style = f.Style,
            IsVisible = f.IsVisible,
            ZIndex = f.ZIndex,
            CreatedBy = f.CreatedBy,
            CreatedAt = f.CreatedAt,
            UpdatedAt = f.UpdatedAt
        };
    }

    private static bool IsGeometryValidForRequest(FeatureCategoryEnum category, AnnotationTypeEnum? annotationType, GeometryTypeEnum geometryType)
    {
        if (category == FeatureCategoryEnum.Data)
        {
            // Pin(Point), Line(LineString), Route(LineString), Polygon(Polygon), Circle(Circle)
            return geometryType == GeometryTypeEnum.Point
                   || geometryType == GeometryTypeEnum.LineString
                   || geometryType == GeometryTypeEnum.Polygon
                   || geometryType == GeometryTypeEnum.Circle;
        }

        // Annotation mapping
        if (annotationType == null) return false;
        return annotationType switch
        {
            AnnotationTypeEnum.Marker => geometryType == GeometryTypeEnum.Point,
            AnnotationTypeEnum.Highlighter => geometryType == GeometryTypeEnum.LineString || geometryType == GeometryTypeEnum.Polygon,
            AnnotationTypeEnum.Text => geometryType == GeometryTypeEnum.Point,
            AnnotationTypeEnum.Note => geometryType == GeometryTypeEnum.Point,
            AnnotationTypeEnum.Link => geometryType == GeometryTypeEnum.Point,
            AnnotationTypeEnum.Video => geometryType == GeometryTypeEnum.Point,
            _ => false
        };
    }
}


