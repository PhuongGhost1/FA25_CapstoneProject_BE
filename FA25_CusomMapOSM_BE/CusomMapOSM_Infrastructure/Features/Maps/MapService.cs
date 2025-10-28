using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.Layers.Enums;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using Optional;
using System.Text.Json;
using CusomMapOSM_Application.Interfaces.Services.Cache;
using CusomMapOSM_Application.Interfaces.Services.LayerData;

namespace CusomMapOSM_Infrastructure.Features.Maps;

public class MapService : IMapService
{
    private readonly IMapRepository _mapRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly IMapHistoryService _mapHistoryService;
    private readonly ILayerDataStore _layerDataStore;

    public MapService(
        IMapRepository mapRepository,
        ICurrentUserService currentUserService,
        ICacheService cacheService,
        IMapHistoryService mapHistoryService,
        ILayerDataStore layerDataStore)
    {
        _mapRepository = mapRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _mapHistoryService = mapHistoryService;
        _layerDataStore = layerDataStore;
    }

    public async Task<Option<CreateMapFromTemplateResponse, Error>> CreateFromTemplate(CreateMapFromTemplateRequest req)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<CreateMapFromTemplateResponse, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        // Get template with all content
        var template = await _mapRepository.GetMapTemplateById(req.TemplateId);
        if (template is null || !template.IsActive)
        {
            return Option.None<CreateMapFromTemplateResponse, Error>(
                Error.NotFound("Map.TemplateNotFound", "Map template not found"));
        }

        // Create map from template
        var newMap = new Map
        {
            MapName = req.CustomName ?? template.MapName,
            Description = req.CustomDescription ?? template.Description,
            IsPublic = req.IsPublic,
            UserId = currentUserId.Value,
            // OrgId removed; organization inferred via workspace when applicable
            DefaultBounds = template.DefaultBounds,
            ViewState = template.ViewState,
            BaseLayer = template.BaseLayer,
            ParentMapId = template.MapId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        if (req.CustomInitialLatitude.HasValue && req.CustomInitialLongitude.HasValue)
        {
            newMap.DefaultBounds = $"{req.CustomInitialLatitude},{req.CustomInitialLongitude}";
            newMap.ViewState = req.CustomInitialZoom.HasValue
                ? $"{{\"center\":[{req.CustomInitialLatitude},{req.CustomInitialLongitude}],\"zoom\":{req.CustomInitialZoom}}}"
                : $"{{\"center\":[{req.CustomInitialLatitude},{req.CustomInitialLongitude}],\"zoom\":10}}";
        }

        var createResult = await _mapRepository.CreateMap(newMap);
        if (!createResult)
        {
            return Option.None<CreateMapFromTemplateResponse, Error>(
                Error.Failure("Map.CreateFailed", "Failed to create map from template"));
        }

        var layersCreated = await CopyTemplateLayersToMap(template.MapId, newMap.MapId, currentUserId.Value);
        var imagesCreated = await CopyTemplateImagesToMap(template.MapId, newMap.MapId, currentUserId.Value);

        template.UsageCount++;
        await _mapRepository.UpdateMapTemplate(template);

        return Option.Some<CreateMapFromTemplateResponse, Error>(new CreateMapFromTemplateResponse
        {
            MapId = newMap.MapId,
            MapName = newMap.MapName,
            TemplateName = template.MapName,
            LayersCreated = layersCreated,
            ImagesCreated = imagesCreated,
            CreatedAt = newMap.CreatedAt
        });
    }

    public async Task<Option<CreateMapResponse, Error>> Create(CreateMapRequest req)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<CreateMapResponse, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        var newMap = new Map
        {
            MapName = req.Name,
            Description = req.Description,
            IsPublic = req.IsPublic,
            UserId = currentUserId.Value,
            // OrgId removed
            DefaultBounds = req.DefaultBounds,
            ViewState = req.ViewState,
            BaseLayer = req.BaseMapProvider ?? "osm",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var createResult = await _mapRepository.CreateMap(newMap);
        if (!createResult)
        {
            return Option.None<CreateMapResponse, Error>(
                Error.Failure("Map.CreateFailed", "Failed to create map"));
        }

        // Auto-create a default layer for the new map
        var defaultLayer = new Layer
        {
            LayerId = Guid.NewGuid(),
            MapId = newMap.MapId,
            UserId = newMap.UserId,
            LayerName = "Default Layer",
            LayerType = LayerTypeEnum.GEOJSON,
            SourceType = LayerSourceEnum.UserUploaded,
            LayerStyle = JsonSerializer.Serialize(new { color = "#2563eb", weight = 2, fillColor = "#3b82f6", fillOpacity = 0.2 }),
            IsPublic = false,
            IsVisible = true,
            ZIndex = 1,
            LayerOrder = 1,
            FeatureCount = 0,
            DataSizeKB = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _layerDataStore.SetDataAsync(defaultLayer,
            JsonSerializer.Serialize(new { type = "FeatureCollection", features = Array.Empty<object>() }));
        await _mapRepository.CreateLayer(defaultLayer);

        return Option.Some<CreateMapResponse, Error>(new CreateMapResponse
        {
            MapId = newMap.MapId,
            CreatedAt = newMap.CreatedAt
        });
    }

    public async Task<Option<GetMapByIdResponse, Error>> GetById(Guid mapId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<GetMapByIdResponse, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        var map = await _mapRepository.GetMapById(mapId);
        if (map is null || !map.IsActive)
        {
            return Option.None<GetMapByIdResponse, Error>(
                Error.NotFound("Map.NotFound", "Map not found"));
        }

        // Check if user has access to this map
        if (!map.IsPublic && map.UserId != currentUserId.Value)
        {
            return Option.None<GetMapByIdResponse, Error>(
                Error.Forbidden("Map.AccessDenied", "You don't have access to this map"));
        }

        var mapDto = await MapToMapDetailDTO(map, currentUserId.Value);

        return Option.Some<GetMapByIdResponse, Error>(new GetMapByIdResponse
        {
            Map = mapDto
        });
    }

    public async Task<Option<GetMyMapsResponse, Error>> GetMyMaps()
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<GetMyMapsResponse, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        var maps = await _mapRepository.GetUserMaps(currentUserId.Value);
        var mapDtos = new List<MapDetailDTO>();

        foreach (var map in maps.Where(m => m.IsActive))
        {
            var mapDto = await MapToMapDetailDTO(map, currentUserId.Value);
            mapDtos.Add(mapDto);
        }

        return Option.Some<GetMyMapsResponse, Error>(new GetMyMapsResponse
        {
            Maps = mapDtos,
            TotalCount = mapDtos.Count
        });
    }

    public async Task<Option<GetOrganizationMapsResponse, Error>> GetOrganizationMaps(Guid orgId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<GetOrganizationMapsResponse, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        // TODO: Check if user is member of the organization

        var maps = await _mapRepository.GetOrganizationMaps(orgId);
        var mapDtos = new List<MapDetailDTO>();

        foreach (var map in maps.Where(m => m.IsActive))
        {
            var mapDto = await MapToMapDetailDTO(map, currentUserId.Value);
            mapDtos.Add(mapDto);
        }

        return Option.Some<GetOrganizationMapsResponse, Error>(new GetOrganizationMapsResponse
        {
            Maps = mapDtos,
            TotalCount = mapDtos.Count,
            OrganizationName = "Organization Name" // TODO: Get from organization service
        });
    }

    public async Task<Option<UpdateMapResponse, Error>> Update(Guid mapId, UpdateMapRequest req)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<UpdateMapResponse, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        var map = await _mapRepository.GetMapById(mapId);
        if (map is null || !map.IsActive)
        {
            return Option.None<UpdateMapResponse, Error>(
                Error.NotFound("Map.NotFound", "Map not found"));
        }

        if (map.UserId != currentUserId.Value)
        {
            return Option.None<UpdateMapResponse, Error>(
                Error.Forbidden("Map.NotOwner", "Only the map owner can update it"));
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(req.Name))
            map.MapName = req.Name;
        if (!string.IsNullOrEmpty(req.Description))
            map.Description = req.Description;
        if (req.IsPublic.HasValue)
            map.IsPublic = req.IsPublic.Value;
        if (!string.IsNullOrEmpty(req.BaseMapProvider))
            map.BaseLayer = req.BaseMapProvider;
        if (!string.IsNullOrEmpty(req.GeographicBounds))
            map.DefaultBounds = req.GeographicBounds;
        if (!string.IsNullOrEmpty(req.ViewState))
            map.ViewState = req.ViewState;

        map.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _mapRepository.UpdateMap(map);
        if (!updateResult)
        {
            return Option.None<UpdateMapResponse, Error>(
                Error.Failure("Map.UpdateFailed", "Failed to update map"));
        }

        // Record snapshot after map update
        var features = await _mapRepository.GetMapFeatures(mapId);
        await _mapHistoryService.RecordSnapshot(mapId, currentUserId.Value, JsonSerializer.Serialize(features));

        return Option.Some<UpdateMapResponse, Error>(new UpdateMapResponse());
    }

    public async Task<Option<DeleteMapResponse, Error>> Delete(Guid mapId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<DeleteMapResponse, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        var map = await _mapRepository.GetMapById(mapId);
        if (map is null || !map.IsActive)
        {
            return Option.None<DeleteMapResponse, Error>(
                Error.NotFound("Map.NotFound", "Map not found"));
        }

        if (map.UserId != currentUserId.Value)
        {
            return Option.None<DeleteMapResponse, Error>(
                Error.Forbidden("Map.NotOwner", "Only the map owner can delete it"));
        }

        var deleteResult = await _mapRepository.DeleteMap(mapId);
        if (!deleteResult)
        {
            return Option.None<DeleteMapResponse, Error>(
                Error.Failure("Map.DeleteFailed", "Failed to delete map"));
        }

        return Option.Some<DeleteMapResponse, Error>(new DeleteMapResponse());
    }

    public async Task<Option<GetMapTemplatesResponse, Error>> GetTemplates()
    {
        var cacheKey = "templates:all:response";

        // Try get from cache first
        var cachedResponse = await _cacheService.GetAsync<GetMapTemplatesResponse>(cacheKey);
        if (cachedResponse != null)
        {
            return Option.Some<GetMapTemplatesResponse, Error>(cachedResponse);
        }

        // Cache miss - get data from repository
        var templates = await _mapRepository.GetMapTemplates();
        var templateDtos = new List<MapTemplateDTO>();

        foreach (var template in templates.Where(t => t.IsActive && t.IsTemplate))
        {
            var layers = await _mapRepository.GetTemplateLayers(template.MapId);

            var templateDto = new MapTemplateDTO
            {
                TemplateId = template.MapId,
                TemplateName = template.MapName,
                Description = template.Description ?? "",
                PreviewImage = template.PreviewImage ?? "",
                Category = template.Category ?? MapTemplateCategoryEnum.General,
                BaseLayer = template.BaseLayer,
                DefaultBounds = template.DefaultBounds ?? "",
                IsPublic = template.IsPublic,
                IsFeatured = template.IsFeatured,
                UsageCount = template.UsageCount,
                CreatedAt = template.CreatedAt
            };
            templateDtos.Add(templateDto);
        }

        var response = new GetMapTemplatesResponse
        {
            Templates = templateDtos
        };

        // Cache for 30 minutes
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(30));

        return Option.Some<GetMapTemplatesResponse, Error>(response);
    }

    public async Task<Option<GetMapTemplateByIdResponse, Error>> GetTemplateById(Guid templateId)
    {
        var cacheKey = $"templates:id:response:{templateId}";

        // Try get from cache first
        var cachedResponse = await _cacheService.GetAsync<GetMapTemplateByIdResponse>(cacheKey);
        if (cachedResponse != null)
        {
            return Option.Some<GetMapTemplateByIdResponse, Error>(cachedResponse);
        }

        // Cache miss - get data from repository
        var template = await _mapRepository.GetMapTemplateById(templateId);
        if (template is null || !template.IsActive || !template.IsTemplate)
        {
            return Option.None<GetMapTemplateByIdResponse, Error>(
                Error.NotFound("Map.TemplateNotFound", "Map template not found"));
        }

        var layers = await _mapRepository.GetTemplateLayers(template.MapId);
        var layerDtos = layers.Select(l => new MapTemplateLayerDTO
        {
            LayerId = l.LayerId,
            LayerName = l.LayerName ?? "Unknown Layer",
            LayerTypeId = (int)l.LayerType,
            LayerStyle = l.LayerStyle ?? "",
            IsVisible = l.IsVisible,
            ZIndex = l.ZIndex,
            LayerOrder = l.LayerOrder,
            FeatureCount = l.FeatureCount,
            DataSizeKB = l.DataSizeKB,
            DataBounds = l.DataBounds
        }).ToList();

        var templateDetailDto = new MapTemplateDetailDTO
        {
            TemplateId = template.MapId,
            TemplateName = template.MapName,
            Description = template.Description ?? "",
            PreviewImage = template.PreviewImage ?? "",
            Category = template.Category ?? MapTemplateCategoryEnum.General,
            BaseLayer = template.BaseLayer,
            DefaultBounds = template.DefaultBounds ?? "",
            IsPublic = template.IsPublic,
            IsFeatured = template.IsFeatured,
            UsageCount = template.UsageCount,
            CreatedAt = template.CreatedAt,
            Layers = layerDtos
        };

        var response = new GetMapTemplateByIdResponse
        {
            Template = templateDetailDto
        };

        // Cache for 30 minutes
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(30));

        return Option.Some<GetMapTemplateByIdResponse, Error>(response);
    }

    public async Task<Option<GetMapTemplateWithDetailsResponse, Error>> GetTemplateWithDetails(Guid templateId)
    {
        var cacheKey = $"templates:details:response:{templateId}";

        // Try get from cache first (Service level caching)
        var cachedResponse = await _cacheService.GetAsync<GetMapTemplateWithDetailsResponse>(cacheKey);
        if (cachedResponse != null)
        {
            return Option.Some<GetMapTemplateWithDetailsResponse, Error>(cachedResponse);
        }

        // Cache miss - get data from repository
        var templateWithDetails = await _mapRepository.GetMapTemplateWithDetails(templateId);
        if (templateWithDetails == null)
        {
            return Option.None<GetMapTemplateWithDetailsResponse, Error>(
                Error.NotFound("Map.TemplateNotFound", "Map template not found"));
        }

        var template = templateWithDetails.Map;

        // Map Template info
        var templateDto = new MapTemplateDTO
        {
            TemplateId = template.MapId,
            TemplateName = template.MapName,
            Description = template.Description ?? "",
            PreviewImage = template.PreviewImage ?? "",
            Category = template.Category ?? MapTemplateCategoryEnum.General,
            BaseLayer = template.BaseLayer,
            DefaultBounds = template.DefaultBounds ?? "",
            IsPublic = template.IsPublic,
            IsFeatured = template.IsFeatured,
            UsageCount = template.UsageCount,
            CreatedAt = template.CreatedAt
        };

        // Map Layers
        var layerDtos = new List<MapLayerDTO>();
        foreach (var layer in templateWithDetails.Layers)
        {
            var layerData = await _layerDataStore.GetDataAsync(layer);
            layerDtos.Add(new MapLayerDTO
            {
                MapLayerId = layer.LayerId, // Use LayerId since MapLayerId doesn't exist anymore
                LayerName = layer.LayerName ?? "Unknown Layer",
                LayerTypeId = (int)layer.LayerType,
                IsVisible = layer.IsVisible,
                ZIndex = layer.ZIndex,
                LayerOrder = layer.LayerOrder,
                LayerData = layerData ?? string.Empty,
                LayerStyle = layer.LayerStyle ?? "",
                FeatureCount = layer.FeatureCount,
                DataSizeKB = layer.DataSizeKB,
                DataBounds = layer.DataBounds
            });
        }

        // Map Images
        var imageDtos = templateWithDetails.MapImages.Select(mi => new MapImageDTO
        {
            MapImageId = mi.MapImageId,
            ImageName = mi.ImageName,
            ImageUrl = mi.ImageUrl,
            Latitude = mi.Latitude,
            Longitude = mi.Longitude,
            Width = mi.Width,
            Height = mi.Height,
            Rotation = mi.Rotation,
            ZIndex = mi.ZIndex,
            IsVisible = mi.IsVisible,
            Description = mi.Description,
            CreatedAt = mi.CreatedAt
        }).ToList();

        var response = new GetMapTemplateWithDetailsResponse
        {
            Template = templateDto,
            Layers = layerDtos,
            Images = imageDtos
        };

        // Cache the processed response for 30 minutes
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(30));

        return Option.Some<GetMapTemplateWithDetailsResponse, Error>(response);
    }

    // Layer management methods
    public async Task<Option<AddLayerToMapResponse, Error>> AddLayerToMap(Guid mapId, AddLayerToMapRequest req)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<AddLayerToMapResponse, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        var map = await _mapRepository.GetMapById(mapId);
        if (map is null || !map.IsActive)
        {
            return Option.None<AddLayerToMapResponse, Error>(
                Error.NotFound("Map.NotFound", "Map not found"));
        }

        if (map.UserId != currentUserId.Value)
        {
            return Option.None<AddLayerToMapResponse, Error>(
                Error.Forbidden("Map.NotOwner", "Only the map owner can add layers"));
        }

        // Get the layer and update its map association
        var layer = await _mapRepository.GetLayerById(req.LayerId);
        if (layer == null)
        {
            return Option.None<AddLayerToMapResponse, Error>(
                Error.NotFound("Layer.NotFound", "Layer not found"));
        }

        layer.MapId = mapId;
        layer.IsVisible = req.IsVisible;
        layer.ZIndex = req.ZIndex;
        layer.LayerOrder = 0;
        layer.UpdatedAt = DateTime.UtcNow;

        var result = await _mapRepository.UpdateLayer(layer);
        if (!result)
        {
            return Option.None<AddLayerToMapResponse, Error>(
                Error.Failure("Map.AddLayerFailed", "Failed to add layer to map"));
        }

        // Record snapshot after adding layer
        var featuresAfterAdd = await _mapRepository.GetMapFeatures(mapId);
        await _mapHistoryService.RecordSnapshot(mapId, currentUserId.Value, JsonSerializer.Serialize(featuresAfterAdd));

        return Option.Some<AddLayerToMapResponse, Error>(new AddLayerToMapResponse
        {
            MapLayerId = layer.LayerId // Using LayerId since MapLayerId no longer exists
        });
    }

    public async Task<Option<RemoveLayerFromMapResponse, Error>> RemoveLayerFromMap(Guid mapId, Guid layerId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<RemoveLayerFromMapResponse, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        var map = await _mapRepository.GetMapById(mapId);
        if (map is null || !map.IsActive)
        {
            return Option.None<RemoveLayerFromMapResponse, Error>(
                Error.NotFound("Map.NotFound", "Map not found"));
        }

        if (map.UserId != currentUserId.Value)
        {
            return Option.None<RemoveLayerFromMapResponse, Error>(
                Error.Forbidden("Map.NotOwner", "Only the map owner can remove layers"));
        }

        var result = await _mapRepository.RemoveLayerFromMap(mapId, layerId);
        if (!result)
        {
            return Option.None<RemoveLayerFromMapResponse, Error>(
                Error.Failure("Map.RemoveLayerFailed", "Failed to remove layer from map"));
        }

        // Record snapshot after removing layer
        var featuresAfterRemove = await _mapRepository.GetMapFeatures(mapId);
        await _mapHistoryService.RecordSnapshot(mapId, currentUserId.Value,
            JsonSerializer.Serialize(featuresAfterRemove));

        return Option.Some<RemoveLayerFromMapResponse, Error>(new RemoveLayerFromMapResponse());
    }

    public async Task<Option<UpdateMapLayerResponse, Error>> UpdateMapLayer(Guid mapId, Guid layerId,
        UpdateMapLayerRequest req)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<UpdateMapLayerResponse, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        var map = await _mapRepository.GetMapById(mapId);
        if (map is null || !map.IsActive)
        {
            return Option.None<UpdateMapLayerResponse, Error>(
                Error.NotFound("Map.NotFound", "Map not found"));
        }

        if (map.UserId != currentUserId.Value)
        {
            return Option.None<UpdateMapLayerResponse, Error>(
                Error.Forbidden("Map.NotOwner", "Only the map owner can update layers"));
        }

        var mapLayer = await _mapRepository.GetMapLayer(mapId, layerId);
        if (mapLayer is null)
        {
            return Option.None<UpdateMapLayerResponse, Error>(
                Error.NotFound("Map.LayerNotFound", "Layer not found in map"));
        }

        // Update fields if provided
        if (req.IsVisible.HasValue)
            mapLayer.IsVisible = req.IsVisible.Value;
        if (req.ZIndex.HasValue)
            mapLayer.ZIndex = req.ZIndex.Value;

        mapLayer.UpdatedAt = DateTime.UtcNow;

        var result = await _mapRepository.UpdateLayer(mapLayer);
        if (!result)
        {
            return Option.None<UpdateMapLayerResponse, Error>(
                Error.Failure("Map.UpdateLayerFailed", "Failed to update map layer"));
        }

        // Record snapshot after update layer
        var featuresAfterUpdateLayer = await _mapRepository.GetMapFeatures(mapId);
        await _mapHistoryService.RecordSnapshot(mapId, currentUserId.Value,
            JsonSerializer.Serialize(featuresAfterUpdateLayer));

        return Option.Some<UpdateMapLayerResponse, Error>(new UpdateMapLayerResponse());
    }

    // Collaboration methods
    public async Task<Option<ShareMapResponse, Error>> ShareMap(ShareMapRequest req)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<ShareMapResponse, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        var map = await _mapRepository.GetMapById(req.MapId);
        if (map is null || !map.IsActive)
        {
            return Option.None<ShareMapResponse, Error>(
                Error.NotFound("Map.NotFound", "Map not found"));
        }

        if (map.UserId != currentUserId.Value)
        {
            return Option.None<ShareMapResponse, Error>(
                Error.Forbidden("Map.NotOwner", "Only the map owner can share it"));
        }

        // TODO: Implement collaboration logic
        // For now, return success
        return Option.Some<ShareMapResponse, Error>(new ShareMapResponse
        {
            Permission = req.Permission
        });
    }

    public async Task<Option<UnshareMapResponse, Error>> UnshareMap(UnshareMapRequest req)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<UnshareMapResponse, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        var map = await _mapRepository.GetMapById(req.MapId);
        if (map is null || !map.IsActive)
        {
            return Option.None<UnshareMapResponse, Error>(
                Error.NotFound("Map.NotFound", "Map not found"));
        }

        if (map.UserId != currentUserId.Value)
        {
            return Option.None<UnshareMapResponse, Error>(
                Error.Forbidden("Map.NotOwner", "Only the map owner can unshare it"));
        }

        // TODO: Implement unshare logic
        // For now, return success
        return Option.Some<UnshareMapResponse, Error>(new UnshareMapResponse());
    }

    private async Task<MapDetailDTO> MapToMapDetailDTO(Map map, Guid currentUserId)
    {
        // Get layers for this map
        var mapLayers = await _mapRepository.GetMapLayers(map.MapId);
        var layerDtos = new List<LayerDTO>();
        foreach (var layer in mapLayers)
        {
            var layerData = await _layerDataStore.GetDataAsync(layer);
            layerDtos.Add(new LayerDTO
            {
                Id = layer.LayerId,
                Name = layer.LayerName ?? "Unknown Layer",
                LayerTypeId = (int)layer.LayerType,
                LayerTypeName = layer.LayerType.ToString(),
                LayerTypeIcon = "",
                SourceName = layer.SourceType.ToString(),
                FilePath = layer.FilePath ?? "",
                LayerData = layerData ?? "",
                LayerStyle = layer.LayerStyle ?? "",
                IsPublic = layer.IsPublic,
                CreatedAt = layer.CreatedAt,
                UpdatedAt = layer.UpdatedAt,
                OwnerId = layer.UserId,
                OwnerName = layer.User?.FullName ?? "Unknown",
                MapLayerId = layer.LayerId,
                IsVisible = layer.IsVisible,
                ZIndex = layer.ZIndex,
                LayerOrder = layer.LayerOrder,
            });
        }

        // Parse geographic bounds - Support both legacy and new format
        double latitude = 0, longitude = 0;
        if (!string.IsNullOrEmpty(map.DefaultBounds))
        {
            // Try legacy simple format first: "lat,lng"
            var parts = map.DefaultBounds.Split(',');
            if (parts.Length == 2 &&
                double.TryParse(parts[0], out var lat) &&
                double.TryParse(parts[1], out var lng))
            {
                latitude = lat;
                longitude = lng;
            }
            else
            {
                // Try new GeoJSON Polygon format
                try
                {
                    using var boundsDoc = JsonDocument.Parse(map.DefaultBounds);
                    if (boundsDoc.RootElement.TryGetProperty("type", out var typeElement) &&
                        typeElement.GetString() == "Polygon")
                    {
                        var coordinates = boundsDoc.RootElement.GetProperty("coordinates")[0];
                        double minLat = double.MaxValue, maxLat = double.MinValue;
                        double minLng = double.MaxValue, maxLng = double.MinValue;

                        foreach (var coord in coordinates.EnumerateArray())
                        {
                            var coordLng = coord[0].GetDouble();
                            var coordLat = coord[1].GetDouble();
                            minLat = Math.Min(minLat, coordLat);
                            maxLat = Math.Max(maxLat, coordLat);
                            minLng = Math.Min(minLng, coordLng);
                            maxLng = Math.Max(maxLng, coordLng);
                        }

                        latitude = (minLat + maxLat) / 2;
                        longitude = (minLng + maxLng) / 2;
                    }
                }
                catch
                {
                    // If parsing fails, keep latitude/longitude as 0
                }
            }
        }

        var viewState = string.IsNullOrEmpty(map.ViewState) ? null : JsonDocument.Parse(map.ViewState);

        return new MapDetailDTO
        {
            Id = map.MapId,
            Name = map.MapName,
            Description = map.Description ?? "",
            IsPublic = map.IsPublic,
            CreatedAt = map.CreatedAt,
            UpdatedAt = map.UpdatedAt,
            InitialLatitude = latitude,
            InitialLongitude = longitude,
            ViewState = viewState,
            BaseLayer = map.BaseLayer,
            OwnerId = map.UserId,
            OwnerName = map.User?.FullName ?? "Unknown",
            IsOwner = map.UserId == currentUserId,
            UserRole = map.UserId == currentUserId ? "Owner" : "Viewer",
            Layers = layerDtos
        };
    }

    // Helper methods for copying template content
    private async Task<int> CopyTemplateLayersToMap(Guid templateId, Guid newMapId, Guid userId)
    {
        var templateLayers = await _mapRepository.GetTemplateLayers(templateId);
        int count = 0;
        foreach (var templateLayer in templateLayers)
        {
            var templateLayerData = await _layerDataStore.GetDataAsync(templateLayer);
            var newLayer = new Layer
            {
                LayerId = Guid.NewGuid(),
                MapId = newMapId,
                UserId = userId,
                LayerName = templateLayer.LayerName,
                LayerType = templateLayer.LayerType,
                SourceType = templateLayer.SourceType,
                LayerStyle = templateLayer.LayerStyle,
                IsPublic = false,


                IsVisible = templateLayer.IsVisible,
                ZIndex = templateLayer.ZIndex,
                LayerOrder = templateLayer.LayerOrder,

                FeatureCount = templateLayer.FeatureCount,
                DataSizeKB = templateLayer.DataSizeKB,
                DataBounds = templateLayer.DataBounds,

                CreatedAt = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(templateLayerData))
            {
                await _layerDataStore.SetDataAsync(newLayer, templateLayerData);
            }

            await _mapRepository.CreateLayer(newLayer);
            count++;
        }

        return count;
    }


    private async Task<int> CopyTemplateImagesToMap(Guid templateId, Guid mapId, Guid userId)
    {
        var templateImages = await _mapRepository.GetTemplateImages(templateId);
        var imagesCreated = 0;

        foreach (var templateImage in templateImages)
        {
            var newImage = new MapImage
            {
                MapId = mapId,
                ImageName = templateImage.ImageName,
                ImageUrl = templateImage.ImageUrl,
                ImageData = templateImage.ImageData,
                Latitude = templateImage.Latitude,
                Longitude = templateImage.Longitude,
                Width = templateImage.Width,
                Height = templateImage.Height,
                Rotation = templateImage.Rotation,
                ZIndex = templateImage.ZIndex,
                IsVisible = templateImage.IsVisible,
                Description = templateImage.Description,
                CreatedAt = DateTime.UtcNow
            };

            imagesCreated++;
        }

        return imagesCreated;
    }

    public async Task<Option<CreateMapTemplateFromGeoJsonResponse, Error>> CreateMapTemplateFromGeoJson(
        CreateMapTemplateFromGeoJsonRequest req)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<CreateMapTemplateFromGeoJsonResponse, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        try
        {
            // Guard: ensure user exists to satisfy FK(layers.user_id -> users.user_id)
            var userExists = await _mapRepository.CheckUserExists(currentUserId.Value);
            if (!userExists)
            {
                return Option.None<CreateMapTemplateFromGeoJsonResponse, Error>(
                    Error.Failure("Map.InvalidUser", "User does not exist in database"));
            }

            var mapTemplate = new Map
            {
                MapName = req.TemplateName,
                Description = req.Description,
                Category = req.Category,
                DefaultBounds = req.DataBounds,
                BaseLayer = "OSM",
                IsPublic = req.IsPublic,
                IsActive = true,
                IsTemplate = true,
                IsFeatured = false,
                UsageCount = 0,
                UserId = currentUserId.Value,
                // OrgId removed
                CreatedAt = DateTime.UtcNow
            };

            var templateCreated = await _mapRepository.CreateMapTemplate(mapTemplate);
            if (!templateCreated)
            {
                return Option.None<CreateMapTemplateFromGeoJsonResponse, Error>(
                    Error.Failure("Map.CreateTemplateFailed", "Failed to create map template"));
            }

            var compressedGeoJsonData = req.DataSizeKB > 5000
                ? // > 5MB
                await CompressGeoJsonDataAsync(req.GeoJsonData)
                : req.GeoJsonData;

            var layerId = Guid.NewGuid();

            var layer = new Layer
            {
                LayerId = layerId,
                MapId = mapTemplate.MapId,
                UserId = mapTemplate.UserId,
                LayerName = req.LayerName,
                LayerType = LayerTypeEnum.GEOJSON,
                SourceType = LayerSourceEnum.UserUploaded,
                LayerStyle = req.LayerStyle,
                IsPublic = req.IsPublic,
                IsVisible = true,
                ZIndex = 1,
                LayerOrder = 1,
                FeatureCount = req.FeatureCount,
                DataSizeKB = req.DataSizeKB,
                DataBounds = req.DataBounds,
                CreatedAt = DateTime.UtcNow
            };

            await _layerDataStore.SetDataAsync(layer, compressedGeoJsonData);

            var layerCreated = await _mapRepository.CreateLayer(layer);
            if (!layerCreated)
            {
                return Option.None<CreateMapTemplateFromGeoJsonResponse, Error>(
                    Error.Failure("Map.CreateTemplateLayerFailed", "Failed to create template layer"));
            }

            return Option.Some<CreateMapTemplateFromGeoJsonResponse, Error>(new CreateMapTemplateFromGeoJsonResponse
            {
                TemplateId = mapTemplate.MapId,
                TemplateName = mapTemplate.MapName,
                Description = mapTemplate.Description ?? string.Empty,
                Category = mapTemplate.Category ?? MapTemplateCategoryEnum.General,
                IsPublic = mapTemplate.IsPublic,
                LayerCount = 1,
                TotalFeatures = req.FeatureCount,
                CreatedAt = mapTemplate.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return Option.None<CreateMapTemplateFromGeoJsonResponse, Error>(
                Error.Failure("Map.CreateTemplateException", $"Failed to create template: {ex.Message}"));
        }
    }

    private async Task<string> CompressGeoJsonDataAsync(string geoJsonData)
    {
        try
        {
            return await Task.Run(() =>
            {
                var compressedJson = JsonSerializer.Serialize(
                    JsonSerializer.Deserialize<object>(geoJsonData),
                    new JsonSerializerOptions { WriteIndented = false }
                );
                return compressedJson;
            });
        }
        catch
        {
            return geoJsonData;
        }
    }

    public async Task<Option<string, Error>> GetLayerData(Guid templateId, Guid layerId)
    {
        try
        {
            var layer = await _mapRepository.GetMapLayer(templateId, layerId);

            if (layer is null)
            {
                return Option.None<string, Error>(
                    Error.NotFound("Map.LayerNotFound", "Layer not found"));
            }

            var layerData = await _layerDataStore.GetDataAsync(layer);

            if (string.IsNullOrEmpty(layerData))
            {
                return Option.None<string, Error>(
                    Error.NotFound("Map.LayerDataNotFound", "Layer data not found"));
            }

            return Option.Some<string, Error>(layerData);
        }
        catch (Exception ex)
        {
            return Option.None<string, Error>(
                Error.Failure("Map.GetLayerDataException", $"Failed to get layer data: {ex.Message}"));
        }
    }

    public async Task<Option<List<LayerInfoResponse>, Error>> GetMapLayers(Guid mapId)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserId();
            if (currentUserId is null)
            {
                return Option.None<List<LayerInfoResponse>, Error>(
                    Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
            }

            var map = await _mapRepository.GetMapById(mapId);
            if (map is null)
            {
                return Option.None<List<LayerInfoResponse>, Error>(
                    Error.NotFound("Map.NotFound", "Map not found"));
            }

            if (map.UserId != currentUserId)
            {
                return Option.None<List<LayerInfoResponse>, Error>(
                    Error.Forbidden("Map.Forbidden", "You don't have permission to access this map"));
            }

            var layers = await _mapRepository.GetMapLayers(mapId);
            var layerInfos = layers.Select(layer => new LayerInfoResponse
            {
                LayerId = layer.LayerId.ToString(),
                LayerName = layer.LayerName,
                Description = null, // Layer entity doesn't have Description property
                LayerType = layer.LayerType.ToString(),
                FeatureCount = layer.FeatureCount ?? 0, // Handle nullable FeatureCount
                IsVisible = layer.IsVisible,
                ZIndex = layer.ZIndex // ZIndex is not nullable
            }).ToList();

            return Option.Some<List<LayerInfoResponse>, Error>(layerInfos);
        }
        catch (Exception ex)
        {
            return Option.None<List<LayerInfoResponse>, Error>(
                Error.Failure("Map.GetLayersException", $"Failed to get map layers: {ex.Message}"));
        }
    }

    public async Task<bool> HasEditPermission(Guid mapId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null) return false;
        var map = await _mapRepository.GetMapById(mapId);
        if (map is null) return false;
        // Owner can edit; future: check collaboration permissions for editors
        return map.UserId == currentUserId.Value;
    }

    #region Zone/Feature Operations

    public async Task<Option<CopyFeatureToLayerResponse, Error>> CopyFeatureToLayer(
        Guid mapId,
        Guid sourceLayerId,
        CopyFeatureToLayerRequest req)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserId();
            if (currentUserId is null)
            {
                return Option.None<CopyFeatureToLayerResponse, Error>(
                    Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
            }

            // Get map and verify ownership
            var map = await _mapRepository.GetMapById(mapId);
            if (map is null)
            {
                return Option.None<CopyFeatureToLayerResponse, Error>(
                    Error.NotFound("Map.NotFound", "Map not found"));
            }

            if (map.UserId != currentUserId)
            {
                return Option.None<CopyFeatureToLayerResponse, Error>(
                    Error.Forbidden("Map.Forbidden", "You don't have permission to modify this map"));
            }

            // Get source layer
            var sourceLayer = await _mapRepository.GetMapLayer(mapId, sourceLayerId);
            if (sourceLayer is null)
            {
                return Option.None<CopyFeatureToLayerResponse, Error>(
                    Error.NotFound("Map.SourceLayerNotFound", "Source layer not found"));
            }

            // Handle target layer (existing or new)
            Layer? targetLayer;
            bool newLayerCreated = false;

            if (!string.IsNullOrEmpty(req.TargetLayerId))
            {
                // Copy to existing layer
                targetLayer = await _mapRepository.GetMapLayer(mapId, Guid.Parse(req.TargetLayerId));
                if (targetLayer is null)
                {
                    return Option.None<CopyFeatureToLayerResponse, Error>(
                        Error.NotFound("Map.TargetLayerNotFound", "Target layer not found"));
                }
            }
            else
            {
                // Create new layer
                if (string.IsNullOrEmpty(req.NewLayerName))
                {
                    return Option.None<CopyFeatureToLayerResponse, Error>(
                        Error.ValidationError("Map.NewLayerNameRequired",
                            "New layer name is required when creating a new layer"));
                }

                var newLayerId = Guid.NewGuid();
                targetLayer = new Layer
                {
                    LayerId = newLayerId,
                    MapId = mapId,
                    UserId = currentUserId.Value,
                    LayerName = req.NewLayerName,
                    LayerType = LayerTypeEnum.GEOJSON,
                    SourceType = LayerSourceEnum.UserUploaded,
                    LayerStyle = JsonSerializer.Serialize(new
                        { color = "#3388ff", weight = 2, fillColor = "#3388ff", fillOpacity = 0.2 }),
                    IsPublic = false,
                    IsVisible = true,
                    ZIndex = 1,
                    LayerOrder = 1,
                    FeatureCount = 0,
                    DataSizeKB = 0,
                    CreatedAt = DateTime.UtcNow
                };

                await _layerDataStore.SetDataAsync(targetLayer,
                    JsonSerializer.Serialize(new { type = "FeatureCollection", features = new object[0] }));

                var layerCreated = await _mapRepository.CreateLayer(targetLayer);
                if (!layerCreated)
                {
                    return Option.None<CopyFeatureToLayerResponse, Error>(
                        Error.Failure("Map.CreateLayerFailed", "Failed to create new layer"));
                }

                newLayerCreated = true;
            }

            // Parse source layer GeoJSON
            var sourceLayerData = await _layerDataStore.GetDataAsync(sourceLayer);
            if (string.IsNullOrEmpty(sourceLayerData))
            {
                return Option.None<CopyFeatureToLayerResponse, Error>(
                    Error.NotFound("Map.SourceLayerEmpty", "Source layer has no data"));
            }

            var sourceGeoJson = JsonSerializer.Deserialize<JsonElement>(sourceLayerData);
            if (!sourceGeoJson.TryGetProperty("features", out var featuresArray) ||
                featuresArray.ValueKind != JsonValueKind.Array)
            {
                return Option.None<CopyFeatureToLayerResponse, Error>(
                    Error.ValidationError("Map.InvalidGeoJson",
                        "Source layer does not contain valid GeoJSON FeatureCollection"));
            }

            var features = featuresArray.EnumerateArray().ToList();

            // Validate feature index
            if (req.FeatureIndex < 0 || req.FeatureIndex >= features.Count)
            {
                return Option.None<CopyFeatureToLayerResponse, Error>(
                    Error.ValidationError("Map.InvalidFeatureIndex",
                        $"Feature index {req.FeatureIndex} is out of range (0-{features.Count - 1})"));
            }

            // Get the feature to copy
            var featureToCopy = features[req.FeatureIndex];

            // Parse target layer GeoJSON
            var targetLayerData = await _layerDataStore.GetDataAsync(targetLayer) ??
                                  JsonSerializer.Serialize(new { type = "FeatureCollection", features = Array.Empty<object>() });
            var targetGeoJson = JsonSerializer.Deserialize<JsonElement>(targetLayerData);
            var targetFeatures = new List<JsonElement>();

            if (targetGeoJson.TryGetProperty("features", out var targetFeaturesArray) &&
                targetFeaturesArray.ValueKind == JsonValueKind.Array)
            {
                targetFeatures = targetFeaturesArray.EnumerateArray().ToList();
            }

            // Add feature to target
            targetFeatures.Add(featureToCopy);

            // Reconstruct target GeoJSON
            var updatedTargetGeoJson = new Dictionary<string, object>
            {
                ["type"] = "FeatureCollection",
                ["features"] = targetFeatures.Select(f => JsonSerializer.Deserialize<object>(f.GetRawText())).ToList()
            };

            // Copy other properties if they exist
            if (targetGeoJson.TryGetProperty("name", out var name))
            {
                updatedTargetGeoJson["name"] = name.GetString() ?? string.Empty;
            }

            if (targetGeoJson.TryGetProperty("crs", out var crs))
            {
                updatedTargetGeoJson["crs"] = JsonSerializer.Deserialize<object>(crs.GetRawText())!;
            }

            // Update target layer
            var updatedTargetLayerData = JsonSerializer.Serialize(updatedTargetGeoJson);
            await _layerDataStore.SetDataAsync(targetLayer, updatedTargetLayerData);
            targetLayer.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _mapRepository.UpdateLayer(targetLayer);
            if (!updateResult)
            {
                return Option.None<CopyFeatureToLayerResponse, Error>(
                    Error.Failure("Map.UpdateFailed", "Failed to update target layer"));
            }

            // Record snapshot after copy feature
            var featuresAfterCopy = await _mapRepository.GetMapFeatures(mapId);
            await _mapHistoryService.RecordSnapshot(mapId, currentUserId.Value,
                JsonSerializer.Serialize(featuresAfterCopy));

            return Option.Some<CopyFeatureToLayerResponse, Error>(new CopyFeatureToLayerResponse
            {
                Success = true,
                Message = newLayerCreated ? "Feature copied to new layer successfully" : "Feature copied successfully",
                TargetLayerFeatureCount = targetFeatures.Count,
                TargetLayerId = targetLayer.LayerId.ToString(),
                TargetLayerName = targetLayer.LayerName,
                NewLayerCreated = newLayerCreated
            });
        }
        catch (Exception ex)
        {
            return Option.None<CopyFeatureToLayerResponse, Error>(
                Error.Failure("Map.CopyFeatureException", $"Failed to copy feature: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> DeleteFeatureFromLayer(
        Guid mapId,
        Guid layerId,
        int featureIndex)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserId();
            if (currentUserId is null)
            {
                return Option.None<bool, Error>(
                    Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
            }

            // Get map and verify ownership
            var map = await _mapRepository.GetMapById(mapId);
            if (map is null)
            {
                return Option.None<bool, Error>(
                    Error.NotFound("Map.NotFound", "Map not found"));
            }

            if (map.UserId != currentUserId)
            {
                return Option.None<bool, Error>(
                    Error.Forbidden("Map.Forbidden", "You don't have permission to modify this map"));
            }

            // Get layer
            var layer = await _mapRepository.GetMapLayer(mapId, layerId);
            if (layer is null)
            {
                return Option.None<bool, Error>(
                    Error.NotFound("Map.LayerNotFound", "Layer not found"));
            }

            // Parse layer GeoJSON
            var layerData = await _layerDataStore.GetDataAsync(layer);
            if (string.IsNullOrEmpty(layerData))
            {
                return Option.None<bool, Error>(
                    Error.NotFound("Map.LayerDataNotFound", "Layer data not found"));
            }

            var geoJson = JsonSerializer.Deserialize<JsonElement>(layerData);
            if (!geoJson.TryGetProperty("features", out var featuresArray) ||
                featuresArray.ValueKind != JsonValueKind.Array)
            {
                return Option.None<bool, Error>(
                    Error.ValidationError("Map.InvalidGeoJson",
                        "Layer does not contain valid GeoJSON FeatureCollection"));
            }

            var features = featuresArray.EnumerateArray().ToList();

            // Validate feature index
            if (featureIndex < 0 || featureIndex >= features.Count)
            {
                return Option.None<bool, Error>(
                    Error.ValidationError("Map.InvalidFeatureIndex",
                        $"Feature index {featureIndex} is out of range (0-{features.Count - 1})"));
            }

            // Remove feature
            features.RemoveAt(featureIndex);

            // Reconstruct GeoJSON
            var updatedGeoJson = new Dictionary<string, object>
            {
                ["type"] = "FeatureCollection",
                ["features"] = features.Select(f => JsonSerializer.Deserialize<object>(f.GetRawText())).ToList()
            };

            // Copy other properties if they exist
            if (geoJson.TryGetProperty("name", out var name))
            {
                updatedGeoJson["name"] = name.GetString() ?? string.Empty;
            }

            if (geoJson.TryGetProperty("crs", out var crs))
            {
                updatedGeoJson["crs"] = JsonSerializer.Deserialize<object>(crs.GetRawText())!;
            }

            // Update layer
            var updatedLayerData = JsonSerializer.Serialize(updatedGeoJson);
            await _layerDataStore.SetDataAsync(layer, updatedLayerData);
            layer.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _mapRepository.UpdateLayer(layer);
            if (!updateResult)
            {
                return Option.None<bool, Error>(
                    Error.Failure("Map.UpdateFailed", "Failed to update layer"));
            }

            // Record snapshot after delete feature
            var featuresAfterDelete = await _mapRepository.GetMapFeatures(mapId);
            await _mapHistoryService.RecordSnapshot(mapId, currentUserId.Value,
                JsonSerializer.Serialize(featuresAfterDelete));

            return Option.Some<bool, Error>(true);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(
                Error.Failure("Map.DeleteFeatureException", $"Failed to delete feature: {ex.Message}"));
        }
    }

    public async Task<Option<UpdateLayerDataResponse, Error>> UpdateLayerData(
        Guid mapId,
        Guid layerId,
        UpdateLayerDataRequest req)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserId();
            if (currentUserId is null)
            {
                return Option.None<UpdateLayerDataResponse, Error>(
                    Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
            }

            // Get map and verify ownership
            var map = await _mapRepository.GetMapById(mapId);
            if (map is null)
            {
                return Option.None<UpdateLayerDataResponse, Error>(
                    Error.NotFound("Map.NotFound", "Map not found"));
            }

            if (map.UserId != currentUserId)
            {
                return Option.None<UpdateLayerDataResponse, Error>(
                    Error.Forbidden("Map.Forbidden", "You don't have permission to modify this map"));
            }

            // Get layer
            var layer = await _mapRepository.GetMapLayer(mapId, layerId);
            if (layer is null)
            {
                return Option.None<UpdateLayerDataResponse, Error>(
                    Error.NotFound("Map.LayerNotFound", "Layer not found"));
            }

            // Validate GeoJSON
            try
            {
                var geoJson = JsonSerializer.Deserialize<JsonElement>(req.LayerData);

                if (!geoJson.TryGetProperty("type", out var typeProperty) ||
                    typeProperty.GetString() != "FeatureCollection")
                {
                    return Option.None<UpdateLayerDataResponse, Error>(
                        Error.ValidationError("Map.InvalidGeoJson", "Data must be a valid GeoJSON FeatureCollection"));
                }

                if (!geoJson.TryGetProperty("features", out var featuresArray) ||
                    featuresArray.ValueKind != JsonValueKind.Array)
                {
                    return Option.None<UpdateLayerDataResponse, Error>(
                        Error.ValidationError("Map.InvalidGeoJson",
                            "FeatureCollection must contain a 'features' array"));
                }

                var featureCount = featuresArray.GetArrayLength();

                // Update layer data
                await _layerDataStore.SetDataAsync(layer, req.LayerData);
                layer.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _mapRepository.UpdateLayer(layer);
                if (!updateResult)
                {
                    return Option.None<UpdateLayerDataResponse, Error>(
                        Error.Failure("Map.UpdateFailed", "Failed to update layer data"));
                }

                // Record snapshot after update layer data
                var featuresAfterUpdateData = await _mapRepository.GetMapFeatures(mapId);
                await _mapHistoryService.RecordSnapshot(mapId, currentUserId.Value,
                    JsonSerializer.Serialize(featuresAfterUpdateData));

                return Option.Some<UpdateLayerDataResponse, Error>(new UpdateLayerDataResponse
                {
                    Success = true,
                    Message = "Layer data updated successfully",
                    FeatureCount = featureCount
                });
            }
            catch (JsonException)
            {
                return Option.None<UpdateLayerDataResponse, Error>(
                    Error.ValidationError("Map.InvalidJson", "Invalid JSON format"));
            }
        }
        catch (Exception ex)
        {
            return Option.None<UpdateLayerDataResponse, Error>(
                Error.Failure("Map.UpdateLayerDataException", $"Failed to update layer data: {ex.Message}"));
        }
    }

    #endregion
}
