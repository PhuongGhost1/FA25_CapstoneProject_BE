using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Common.Mappers;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.Layers.Enums;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Application.Interfaces.Services.Organization;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;
using CusomMapOSM_Domain.Entities.Sessions.Enums;
using Optional;
using System;
using System.Linq;
using System.Text.Json;
using CusomMapOSM_Application.Interfaces.Services.Cache;
using CusomMapOSM_Application.Interfaces.Services.LayerData;
using CusomMapOSM_Domain.Entities.Workspaces;
using CusomMapOSM_Domain.Entities.Workspaces.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Workspaces;
using CusomMapOSM_Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CusomMapOSM_Infrastructure.Features.Maps;

public class MapService : IMapService
{
    private readonly IMapRepository _mapRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRedisCacheService _cacheService;
    private readonly IMapHistoryService _mapHistoryService;
    private readonly ILayerDataStore _layerDataStore;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IHubContext<MapCollaborationHub> _hubContext;
    private readonly IOrganizationPermissionService _organizationPermissionService;
    private readonly ISessionRepository _sessionRepository;

    public MapService(
        IMapRepository mapRepository,
        ICurrentUserService currentUserService,
        IMapHistoryService mapHistoryService,
        ILayerDataStore layerDataStore,
        IWorkspaceRepository workspaceRepository,
        IOrganizationRepository organizationRepository,
        IHubContext<MapCollaborationHub> hubContext, 
        IRedisCacheService cacheService,
        IOrganizationPermissionService organizationPermissionService,
        ISessionRepository sessionRepository)
    {
        _mapRepository = mapRepository;
        _currentUserService = currentUserService;
        _mapHistoryService = mapHistoryService;
        _layerDataStore = layerDataStore;
        _workspaceRepository = workspaceRepository;
        _organizationRepository = organizationRepository;
        _hubContext = hubContext;
        _cacheService = cacheService;
        _organizationPermissionService = organizationPermissionService;
        _sessionRepository = sessionRepository;
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
        if (template is null)
        {
            return Option.None<CreateMapFromTemplateResponse, Error>(
                Error.NotFound("Map.TemplateNotFound", "Map template not found"));
        }

        var workspaceResult = await ResolveWorkspaceIdAsync(currentUserId.Value, req.WorkspaceId);

        return await workspaceResult.Match<Task<Option<CreateMapFromTemplateResponse, Error>>>(
            async workspaceId =>
            {
                var newMap = new Map
                {
                    MapName = req.CustomName ?? template.MapName,
                    Description = req.CustomDescription ?? template.Description,
                    IsPublic = req.IsPublic,
                    UserId = currentUserId.Value,
                    WorkspaceId = workspaceId,
                    DefaultBounds = template.DefaultBounds,
                    ViewState = template.ViewState,
                    BaseLayer = template.BaseLayer,
                    ParentMapId = template.MapId,
                    CreatedAt = DateTime.UtcNow
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
            },
            error => Task.FromResult(Option.None<CreateMapFromTemplateResponse, Error>(error))
        );
    }

    public async Task<Option<CreateMapResponse, Error>> Create(CreateMapRequest req)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<CreateMapResponse, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        var workspaceResult = await ResolveWorkspaceIdAsync(currentUserId.Value, req.WorkspaceId);

        return await workspaceResult.Match<Task<Option<CreateMapResponse, Error>>>(
            async workspaceId =>
            {
                var newMap = new Map
                {
                    MapName = req.Name,
                    Description = req.Description,
                    IsPublic = req.IsPublic,
                    UserId = currentUserId.Value,
                    WorkspaceId = workspaceId,
                    DefaultBounds = req.DefaultBounds,
                    ViewState = req.ViewState,
                    BaseLayer = req.BaseMapProvider ?? "osm",
                    IsStoryMap = req.IsStoryMap,
                    CreatedAt = DateTime.UtcNow
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
                    LayerType = LayerType.GEOJSON,
                    SourceType = LayerSource.UserUploaded,
                    LayerStyle = JsonSerializer.Serialize(new { color = "#2563eb", weight = 2, fillColor = "#3b82f6", fillOpacity = 0.2 }),
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
            },
            error => Task.FromResult(Option.None<CreateMapResponse, Error>(error))
        );
    }

    public async Task<Option<GetMapByIdResponse, Error>> GetById(Guid mapId)
    {
        var currentUserId = _currentUserService.GetUserId();

        var map = await _mapRepository.GetMapById(mapId);
        if (map is null || !map.IsActive)
        {
            return Option.None<GetMapByIdResponse, Error>(
                Error.NotFound("Map.NotFound", "Map not found"));
        }
        
        var hasAccess = map.IsPublic || map.Status == MapStatusEnum.Published;
    
        if (!hasAccess && currentUserId.HasValue && map.UserId == currentUserId.Value)
        {
            hasAccess = true;
        }

        if (!hasAccess && currentUserId.HasValue)
        {
            hasAccess = await _organizationPermissionService.HasOrganizationAccess(currentUserId.Value, map.UserId);
        }

        if (!hasAccess)
        {
            var errorCode = currentUserId.HasValue ? "Map.AccessDenied" : "Map.Unauthorized";
            var errorMessage = currentUserId.HasValue 
                ? "You don't have access to this map" 
                : "User not authenticated";
        
            return Option.None<GetMapByIdResponse, Error>(
                Error.Forbidden(errorCode, errorMessage));
        }
        
        var layers = await _mapRepository.GetMapLayers(mapId);
        
        var layerDataMap = new Dictionary<Guid, string>();
        foreach (var layer in layers)
        {
            var layerData = await _layerDataStore.GetDataAsync(layer);
            if (!string.IsNullOrEmpty(layerData))
            {
                layerDataMap[layer.LayerId] = layerData;
            }
        }
        
        var response = map.ToGetMapByIdResponse(layers, layerDataMap);
        
        return Option.Some<GetMapByIdResponse, Error>(response);
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
        var mapDtos = new List<MapListItemDTO>();

        foreach (var map in maps.Where(m => m.IsActive))
        {
            mapDtos.Add(new MapListItemDTO
            {
                Id = map.MapId,
                Name = map.MapName,
                Description = map.Description ?? "",
                IsPublic = map.IsPublic,
                Status = map.Status,
                IsStoryMap = map.IsStoryMap,
                PreviewImage = map.PreviewImage,
                CreatedAt = map.CreatedAt,
                UpdatedAt = map.UpdatedAt,
                LastActivityAt = map.UpdatedAt ?? map.CreatedAt,
                OwnerId = map.UserId,
                OwnerName = map.User?.FullName ?? "Unknown",
                IsOwner = true,
                WorkspaceName = map.Workspace?.WorkspaceName
            });
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

        var organization = await _organizationRepository.GetOrganizationById(orgId);
        if (organization is null)
        {
            return Option.None<GetOrganizationMapsResponse, Error>(
                Error.NotFound("Organization.NotFound", "Organization not found"));
        }

        var member = await _organizationRepository.GetOrganizationMemberByUserAndOrg(currentUserId.Value, orgId);
        if (member is null)
        {
            return Option.None<GetOrganizationMapsResponse, Error>(
                Error.Forbidden("Organization.NotMember", "User is not a member of this organization"));
        }

        var maps = await _mapRepository.GetOrganizationMaps(orgId);
        var mapDtos = new List<MapListItemDTO>();

        foreach (var map in maps.Where(m => m.IsActive))
        {
            mapDtos.Add(new MapListItemDTO
            {
                Id = map.MapId,
                Name = map.MapName,
                Description = map.Description ?? "",
                IsPublic = map.IsPublic,
                Status = map.Status,
                IsStoryMap = map.IsStoryMap,
                PreviewImage = map.PreviewImage,
                CreatedAt = map.CreatedAt,
                UpdatedAt = map.UpdatedAt,
                LastActivityAt = map.UpdatedAt ?? map.CreatedAt,
                OwnerId = map.UserId,
                OwnerName = map.User?.FullName ?? "Unknown",
                IsOwner = currentUserId.Value == map.UserId,
                WorkspaceName = map.Workspace?.WorkspaceName
            });
        }

        return Option.Some<GetOrganizationMapsResponse, Error>(new GetOrganizationMapsResponse
        {
            Maps = mapDtos,
            TotalCount = mapDtos.Count,
            OrganizationName = organization.OrgName
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

        // Check if map has active sessions (WAITING, IN_PROGRESS, PAUSED)
        var activeSessions = await _sessionRepository.GetSessionsByMapId(mapId);
        var hasActiveSessions = activeSessions.Any(s => 
            s.Status == SessionStatusEnum.WAITING || 
            s.Status == SessionStatusEnum.IN_PROGRESS || 
            s.Status == SessionStatusEnum.PAUSED);
        
        if (hasActiveSessions)
        {
            return Option.None<DeleteMapResponse, Error>(
                Error.ValidationError("Map.HasActiveSessions", 
                    "Cannot delete map while it has active sessions. Please end or cancel all active sessions first."));
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
        var cachedResponse = await _cacheService.Get<GetMapTemplatesResponse>(cacheKey);
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
        await _cacheService.Set(cacheKey, response, TimeSpan.FromMinutes(30));

        return Option.Some<GetMapTemplatesResponse, Error>(response);
    }

    public async Task<Option<GetMapTemplateByIdResponse, Error>> GetTemplateById(Guid templateId)
    {
        var cacheKey = $"templates:id:response:{templateId}";

        // Try get from cache first
        var cachedResponse = await _cacheService.Get<GetMapTemplateByIdResponse>(cacheKey);
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
            IsVisible = true,
            ZIndex = 1,
            LayerOrder = 0,
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
        await _cacheService.Set(cacheKey, response, TimeSpan.FromMinutes(30));

        return Option.Some<GetMapTemplateByIdResponse, Error>(response);
    }

    public async Task<Option<GetMapTemplateWithDetailsResponse, Error>> GetTemplateWithDetails(Guid templateId)
    {
        var cacheKey = $"templates:details:response:{templateId}";

        // Try get from cache first (Service level caching)
        var cachedResponse = await _cacheService.Get<GetMapTemplateWithDetailsResponse>(cacheKey);
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
                IsVisible = true,
                ZIndex = 1,
                LayerOrder = 0,
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
        await _cacheService.Set(cacheKey, response, TimeSpan.FromMinutes(30));

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

        Layer layer;
        
        // Check if LayerId is provided (existing layer) or need to create new layer
        if (req.LayerId != Guid.Empty)
        {
            // Get the existing layer and update its map association
            layer = await _mapRepository.GetLayerById(req.LayerId);
            if (layer == null)
            {
                return Option.None<AddLayerToMapResponse, Error>(
                    Error.NotFound("Layer.NotFound", "Layer not found"));
            }

            layer.MapId = mapId;
            layer.LayerData = req.LayerData;
            layer.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _mapRepository.UpdateLayer(layer);
            if (!updateResult)
            {
                return Option.None<AddLayerToMapResponse, Error>(
                    Error.Failure("Map.AddLayerFailed", "Failed to add layer to map"));
            }
        }
        else
        {
            // Create a new layer with provided data
            if (string.IsNullOrWhiteSpace(req.LayerName) || string.IsNullOrWhiteSpace(req.LayerData))
            {
                return Option.None<AddLayerToMapResponse, Error>(
                    Error.ValidationError("Layer.InvalidData", "LayerName and LayerData are required when creating a new layer"));
            }

            // Parse LayerTypeId to LayerTypeEnum
            LayerType layerType = LayerType.GEOJSON; // Default
            if (!string.IsNullOrWhiteSpace(req.LayerTypeId))
            {
                if (int.TryParse(req.LayerTypeId, out var typeId))
                {
                    layerType = (LayerType)typeId;
                }
                else if (Enum.TryParse<LayerType>(req.LayerTypeId, true, out var parsedType))
                {
                    layerType = parsedType;
                }
            }

            // Calculate data size for compression decision
            var dataSizeKB = req.LayerData.Length / 1024.0;
            
            // Compress large data (> 5MB) to avoid max_allowed_packet issues
            var compressedData = dataSizeKB > 5000
                ? await CompressGeoJsonDataAsync(req.LayerData)
                : req.LayerData;

            layer = new Layer
            {
                LayerId = Guid.NewGuid(),
                MapId = mapId,
                LayerName = req.LayerName,
                LayerType = layerType,
                LayerStyle = req.LayerStyle ?? string.Empty,
                SourceType = LayerSource.UserUploaded,
                FilePath = string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = currentUserId.Value,
                DataSizeKB = dataSizeKB
            };

            // Store layer data using LayerDataStore (Redis/File storage) instead of direct DB insert
            // This prevents "max_allowed_packet" errors when uploading large files
            await _layerDataStore.SetDataAsync(layer, compressedData);

            var createResult = await _mapRepository.CreateLayer(layer);
            if (!createResult)
            {
                return Option.None<AddLayerToMapResponse, Error>(
                    Error.Failure("Map.CreateLayerFailed", "Failed to create new layer"));
            }
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
        
        if (!string.IsNullOrWhiteSpace(req.LayerName))
        {
            mapLayer.LayerName = req.LayerName.Trim();
        }

        mapLayer.IsVisible = req.IsVisible;
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
    
    private async Task<Option<Guid, Error>> ResolveWorkspaceIdAsync(Guid userId, Guid? requestedWorkspaceId)
    {
        var sanitizedWorkspaceId = requestedWorkspaceId.HasValue && requestedWorkspaceId != Guid.Empty
            ? requestedWorkspaceId
            : null;

        if (sanitizedWorkspaceId.HasValue)
        {
            var workspace = await _workspaceRepository.GetByIdAsync(sanitizedWorkspaceId.Value);
            if (workspace is null || !workspace.IsActive)
            {
                return Option.None<Guid, Error>(
                    Error.NotFound("Workspace.NotFound", "Workspace not found"));
            }

            return Option.Some<Guid, Error>(workspace.WorkspaceId);
        }

        var personalWorkspace = await _workspaceRepository.GetPersonalWorkspaceAsync(userId);
        if (personalWorkspace is not null)
        {
            if (!personalWorkspace.IsActive)
            {
                personalWorkspace.IsActive = true;
                personalWorkspace.UpdatedAt = DateTime.UtcNow;
                await _workspaceRepository.UpdateAsync(personalWorkspace);
            }

            return Option.Some<Guid, Error>(personalWorkspace.WorkspaceId);
        }

        var newWorkspace = new CusomMapOSM_Domain.Entities.Workspaces.Workspace
        {
            WorkspaceId = Guid.NewGuid(),
            OrgId = null,
            CreatedBy = userId,
            WorkspaceName = $"Personal Workspace {userId:N}",
            Description = "Automatically created personal workspace.",
            Icon = null,
            Access = WorkspaceAccessEnum.Private,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Organization = null,
            Creator = null!
        };

        var createdWorkspace = await _workspaceRepository.CreateAsync(newWorkspace);
        return Option.Some<Guid, Error>(createdWorkspace.WorkspaceId);
    }

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
            var mapTemplate = new Map
            {
                MapName = req.TemplateName,
                Description = req.Description,
                Category = req.Category,
                DefaultBounds = req.DataBounds,
                BaseLayer = "OSM",
                IsPublic = req.IsPublic,
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
                LayerType = LayerType.GEOJSON,
                SourceType = LayerSource.UserUploaded,
                LayerStyle = req.LayerStyle,
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

            var hasAccess = map.UserId == currentUserId.Value;
            if (!hasAccess)
            {
                hasAccess = await _organizationPermissionService.HasOrganizationAccess(currentUserId.Value, map.UserId);
            }
            
            if (!hasAccess)
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
                IsVisible = true, // Layer entity doesn't have IsVisible property, default to true
                ZIndex = 0 // Layer entity doesn't have ZIndex property, default to 0
            }).ToList();

            return Option.Some<List<LayerInfoResponse>, Error>(layerInfos);
        }
        catch (Exception ex)
        {
            return Option.None<List<LayerInfoResponse>, Error>(
                Error.Failure("Map.GetLayersException", $"Failed to get map layers: {ex.Message}"));
        }
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

            // Get map and verify workspace edit permission
            var map = await _mapRepository.GetMapById(mapId);
            if (map is null)
            {
                return Option.None<CopyFeatureToLayerResponse, Error>(
                    Error.NotFound("Map.NotFound", "Map not found"));
            }

            var canEditMap = await _organizationPermissionService.CanEditMap(map, currentUserId.Value);
            if (!canEditMap)
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
                    LayerType = LayerType.GEOJSON,
                    SourceType = LayerSource.UserUploaded,
                    LayerStyle = JsonSerializer.Serialize(new
                        { color = "#3388ff", weight = 2, fillColor = "#3388ff", fillOpacity = 0.2 }),
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

                // Notify other users via SignalR
                try
                {
                    await _hubContext.Clients.Group($"map:{mapId}")
                        .SendAsync("LayerUpdated", new { MapId = mapId, LayerId = layerId });
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the update
                    // Logger can be added if needed
                }

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
    
    #region Map Publishing Operations
    
    public async Task<Option<bool, Error>> PublishMap(Guid mapId, PublishMapRequest request)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<bool, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        var map = await _mapRepository.GetMapById(mapId);
        if (map is null || !map.IsActive)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Map.NotFound", "Map not found"));
        }

        if (map.UserId != currentUserId.Value)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("Map.NotOwner", "Only the map owner can publish it"));
        }

        if (map.Status != MapStatusEnum.Draft)
        {
            return Option.None<bool, Error>(
                Error.ValidationError("Map.InvalidStatus", 
                    $"Map cannot be published from status {map.Status}. Only Draft maps can be published."));
        }
        map.Status = MapStatusEnum.Published;
        map.IsPublic = true;
        map.IsStoryMap = request.IsStoryMap;
        map.PublishedAt = DateTime.UtcNow;
        map.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _mapRepository.UpdateMap(map);
        if (!updateResult)
        {
            return Option.None<bool, Error>(
                Error.Failure("Map.PublishFailed", "Failed to publish map"));
        }

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<bool, Error>> UnpublishMap(Guid mapId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<bool, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        var map = await _mapRepository.GetMapById(mapId);
        if (map is null || !map.IsActive)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Map.NotFound", "Map not found"));
        }

        if (map.UserId != currentUserId.Value)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("Map.NotOwner", "Only the map owner can unpublish it"));
        }

        if (map.Status != MapStatusEnum.Published)
        {
            return Option.None<bool, Error>(
                Error.ValidationError("Map.InvalidStatus", 
                    $"Only published maps can be unpublished. Current status: {map.Status}"));
        }

        map.Status = MapStatusEnum.Draft;
        map.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _mapRepository.UpdateMap(map);
        if (!updateResult)
        {
            return Option.None<bool, Error>(
                Error.Failure("Map.UnpublishFailed", "Failed to unpublish map"));
        }

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<bool, Error>> ArchiveMap(Guid mapId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<bool, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        var map = await _mapRepository.GetMapById(mapId);
        if (map is null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Map.NotFound", "Map not found"));
        }

        if (map.UserId != currentUserId.Value)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("Map.NotOwner", "Only the map owner can archive it"));
        }

        map.Status = MapStatusEnum.Archived;
        map.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _mapRepository.UpdateMap(map);
        if (!updateResult)
        {
            return Option.None<bool, Error>(
                Error.Failure("Map.ArchiveFailed", "Failed to archive map"));
        }

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<bool, Error>> RestoreMap(Guid mapId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<bool, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        var map = await _mapRepository.GetMapById(mapId);
        if (map is null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Map.NotFound", "Map not found"));
        }

        if (map.UserId != currentUserId.Value)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("Map.NotOwner", "Only the map owner can restore it"));
        }

        // Chỉ cho phép restore khi map đang ở trạng thái Archived
        if (map.Status != MapStatusEnum.Archived)
        {
            return Option.None<bool, Error>(
                Error.ValidationError("Map.InvalidStatus", 
                    $"Only archived maps can be restored. Current status: {map.Status}"));
        }

        map.Status = MapStatusEnum.Draft;
        map.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _mapRepository.UpdateMap(map);
        if (!updateResult)
        {
            return Option.None<bool, Error>(
                Error.Failure("Map.RestoreFailed", "Failed to restore map"));
        }

        return Option.Some<bool, Error>(true);
    }
    
    #endregion

    // Custom listings
    public async Task<Option<GetMyMapsResponse, Error>> GetMyRecentMaps(int limit)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<GetMyMapsResponse, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        var results = await _mapRepository.GetUserRecentMapsWithActivity(currentUserId.Value, limit);
        var dtos = new List<MapListItemDTO>();
        
        foreach (var (map, lastActivity) in results.Where(x => x.Map.IsActive))
        {
            dtos.Add(new MapListItemDTO
            {
                Id = map.MapId,
                Name = map.MapName,
                Description = map.Description ?? "",
                IsPublic = map.IsPublic,
                Status = map.Status,
                PreviewImage = map.PreviewImage,
                CreatedAt = map.CreatedAt,
                UpdatedAt = map.UpdatedAt,
                LastActivityAt = lastActivity,
                OwnerId = map.UserId,
                OwnerName = map.User?.FullName ?? "Unknown",
                IsOwner = true,
                WorkspaceName = map.Workspace?.WorkspaceName
            });
        }

        return Option.Some<GetMyMapsResponse, Error>(new GetMyMapsResponse
        {
            Maps = dtos,
            TotalCount = dtos.Count
        });
    }

    public async Task<Option<GetMyMapsResponse, Error>> GetMyDraftMaps()
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<GetMyMapsResponse, Error>(
                Error.Unauthorized("Map.Unauthorized", "User not authenticated"));
        }

        var maps = await _mapRepository.GetUserDraftMaps(currentUserId.Value);
        var dtos = new List<MapListItemDTO>();
        
        foreach (var m in maps.Where(m => m.IsActive))
        {
            dtos.Add(new MapListItemDTO
            {
                Id = m.MapId,
                Name = m.MapName,
                Description = m.Description ?? "",
                IsPublic = m.IsPublic,
                Status = m.Status,
                PreviewImage = m.PreviewImage,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                LastActivityAt = m.UpdatedAt ?? m.CreatedAt,
                OwnerId = m.UserId,
                OwnerName = m.User?.FullName ?? "Unknown",
                IsOwner = true,
                WorkspaceName = m.Workspace?.WorkspaceName
            });
        }

        return Option.Some<GetMyMapsResponse, Error>(new GetMyMapsResponse
        {
            Maps = dtos,
            TotalCount = dtos.Count
        });
    }
}
