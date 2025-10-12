using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.Layers.Enums;
using CusomMapOSM_Domain.Entities.Annotations;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Infrastructure.Databases;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using Optional;
using System.Text.Json;
using CusomMapOSM_Application.Interfaces.Services.Cache;

namespace CusomMapOSM_Infrastructure.Features.Maps;

public class MapService : IMapService
{
    private readonly IMapRepository _mapRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService; 

    public MapService(
        IMapRepository mapRepository,
        ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _mapRepository = mapRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
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
            OrgId = null, // TODO: Get from user's organization context when implemented
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
            OrgId = null, 
            DefaultBounds = $"{req.InitialLatitude},{req.InitialLongitude}",
            ViewState = $"{{\"center\":[{req.InitialLatitude},{req.InitialLongitude}],\"zoom\":{req.InitialZoom}}}",
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
        var layerDtos = templateWithDetails.Layers.Select(l => new MapLayerDTO
        {
            MapLayerId = l.LayerId, // Use LayerId since MapLayerId doesn't exist anymore
            LayerName = l.LayerName ?? "Unknown Layer",
            LayerTypeId = (int)l.LayerType,
            IsVisible = l.IsVisible,
            ZIndex = l.ZIndex,
            LayerOrder = l.LayerOrder,
            LayerData = l.LayerData ?? "",
            LayerStyle = l.LayerStyle ?? "",
            CustomStyle = l.CustomStyle,
            FeatureCount = l.FeatureCount,
            DataSizeKB = l.DataSizeKB,
            DataBounds = l.DataBounds
        }).ToList();

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

        // Update layer properties for this map
        layer.MapId = mapId;
        layer.IsVisible = req.IsVisible;
        layer.ZIndex = req.ZIndex;
        layer.LayerOrder = 0; // TODO: Calculate proper order
        layer.CustomStyle = req.CustomStyle;
        layer.FilterConfig = req.FilterConfig;
        layer.UpdatedAt = DateTime.UtcNow;

        var result = await _mapRepository.UpdateLayer(layer);
        if (!result)
        {
            return Option.None<AddLayerToMapResponse, Error>(
                Error.Failure("Map.AddLayerFailed", "Failed to add layer to map"));
        }

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

        return Option.Some<RemoveLayerFromMapResponse, Error>(new RemoveLayerFromMapResponse());
    }

    public async Task<Option<UpdateMapLayerResponse, Error>> UpdateMapLayer(Guid mapId, Guid layerId, UpdateMapLayerRequest req)
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
        if (!string.IsNullOrEmpty(req.CustomStyle))
            mapLayer.CustomStyle = req.CustomStyle;
        if (!string.IsNullOrEmpty(req.FilterConfig))
            mapLayer.FilterConfig = req.FilterConfig;

        mapLayer.UpdatedAt = DateTime.UtcNow;

        var result = await _mapRepository.UpdateLayer(mapLayer);
        if (!result)
        {
            return Option.None<UpdateMapLayerResponse, Error>(
                Error.Failure("Map.UpdateLayerFailed", "Failed to update map layer"));
        }

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
        var layerDtos = mapLayers.Select(l => new LayerDTO
        {
            Id = l.LayerId,
            Name = l.LayerName ?? "Unknown Layer",
            LayerTypeId = (int)l.LayerType,
            LayerTypeName = l.LayerType.ToString(),
            LayerTypeIcon = "",
            SourceName = l.SourceType.ToString(),
            FilePath = l.FilePath ?? "",
            LayerData = l.LayerData ?? "",
            LayerStyle = l.LayerStyle ?? "",
            IsPublic = l.IsPublic,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt,
            OwnerId = l.UserId,
            OwnerName = l.User?.FullName ?? "Unknown",
            // Layer specific properties (moved from MapLayer)
            MapLayerId = l.LayerId, // Use LayerId since MapLayerId doesn't exist
            IsVisible = l.IsVisible,
            ZIndex = l.ZIndex,
            LayerOrder = l.LayerOrder,
            CustomStyle = l.CustomStyle ?? "",
            FilterConfig = l.FilterConfig ?? ""
        }).ToList();

        // Parse geographic bounds
        double latitude = 0, longitude = 0;
        if (!string.IsNullOrEmpty(map.DefaultBounds))
        {
            var parts = map.DefaultBounds.Split(',');
            if (parts.Length >= 2)
            {
                double.TryParse(parts[0], out latitude);
                double.TryParse(parts[1], out longitude);
            }
        }

        var viewState = JsonDocument.Parse(map.ViewState);
        
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
    private async Task<int> CopyTemplateLayersToMap(Guid templateId, Guid mapId, Guid userId)
    {
        var templateLayers = await _mapRepository.GetTemplateLayers(templateId);
        var layersCreated = 0;

        foreach (var templateLayer in templateLayers)
        {
            // Create Layer entity first for the copy
            var newLayerId = Guid.NewGuid();
            
            var layer = new Layer
            {
                LayerId = newLayerId,
                MapId = mapId,
                UserId = userId,
                LayerName = templateLayer.LayerName,
                LayerType = templateLayer.LayerType,
                SourceType = templateLayer.SourceType,
                LayerData = templateLayer.LayerData,
                LayerStyle = templateLayer.LayerStyle,
                IsPublic = false, // New map layers are private by default
                
                // Copy layer display properties
                IsVisible = templateLayer.IsVisible,
                ZIndex = templateLayer.ZIndex,
                LayerOrder = templateLayer.LayerOrder,
                CustomStyle = templateLayer.CustomStyle,
                FilterConfig = templateLayer.FilterConfig,
                FeatureCount = templateLayer.FeatureCount,
                DataSizeKB = templateLayer.DataSizeKB,
                DataBounds = templateLayer.DataBounds,
                
                CreatedAt = DateTime.UtcNow
            };

            var layerEntityCreated = await _mapRepository.CreateLayer(layer);
            if (layerEntityCreated)
            {
                layersCreated++;
            }
        }

        return layersCreated;
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

    public async Task<Option<CreateMapTemplateFromGeoJsonResponse, Error>> CreateMapTemplateFromGeoJson(CreateMapTemplateFromGeoJsonRequest req)
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
                BaseLayer = "osm",
                IsPublic = req.IsPublic,
                IsActive = true,
                IsTemplate = true,
                IsFeatured = false,
                UsageCount = 0,
                UserId = currentUserId.Value,
                OrgId = null,
                CreatedAt = DateTime.UtcNow
            };

            var templateCreated = await _mapRepository.CreateMapTemplate(mapTemplate);
            if (!templateCreated)
            {
                return Option.None<CreateMapTemplateFromGeoJsonResponse, Error>(
                    Error.Failure("Map.CreateTemplateFailed", "Failed to create map template"));
            }

            var compressedGeoJsonData = req.DataSizeKB > 5000 ? // > 5MB
                await CompressGeoJsonDataAsync(req.GeoJsonData) : req.GeoJsonData;

            var layerId = Guid.NewGuid();

            var layer = new Layer
            {
                LayerId = layerId,
                MapId = mapTemplate.MapId,
                UserId = mapTemplate.UserId, // Ensure we use the same user ID that was verified with the map
                LayerName = req.LayerName,
                LayerType = LayerTypeEnum.GEOJSON,
                SourceType = LayerSourceEnum.UserUploaded,
                LayerData = compressedGeoJsonData,
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
            var layerData = await _mapRepository.GetLayerDataById(templateId, layerId);
            
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
}
