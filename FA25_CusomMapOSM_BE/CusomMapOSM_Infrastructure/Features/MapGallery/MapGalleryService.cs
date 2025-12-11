using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Common.Mappers;
using CusomMapOSM_Application.Interfaces.Features.MapGallery;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.Documents;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using CusomMapOSM_Application.Models.DTOs.Features.MapGallery;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.User;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Workspaces;
using CusomMapOSM_Application.Interfaces.Services.LayerData;
using CusomMapOSM_Application.Interfaces.Services.MapFeatures;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.Workspaces;
using CusomMapOSM_Domain.Entities.Workspaces.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.StoryMaps;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Locations;
using CusomMapOSM_Domain.Entities.Timeline;
using CusomMapOSM_Domain.Entities.Animations;
using CusomMapOSM_Domain.Entities.Zones;
using CusomMapOSM_Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.MapGallery;

public class MapGalleryService : IMapGalleryService
{
    private readonly IMongoCollection<MapGalleryDocument> _collection;
    private readonly IMapService _mapService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IMapRepository _mapRepository;
    private readonly ILayerDataStore _layerDataStore;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IMapFeatureRepository _mapFeatureRepository;
    private readonly IMapFeatureStore _mapFeatureStore;
    private readonly IStoryMapRepository _storyMapRepository;
    private readonly IHubContext<NotificationHub> _hubContext;

    public MapGalleryService(
        IMongoDatabase database,
        IMapService mapService,
        ICurrentUserService currentUserService,
        IMapRepository mapRepository,
        IUserRepository userRepository,
        ILayerDataStore layerDataStore,
        IWorkspaceRepository workspaceRepository,
        IMapFeatureRepository mapFeatureRepository,
        IMapFeatureStore mapFeatureStore,
        IStoryMapRepository storyMapRepository,
        IHubContext<NotificationHub> hubContext)
    {
        _collection = database.GetCollection<MapGalleryDocument>("map_gallery");
        _mapService = mapService;
        _currentUserService = currentUserService;
        _mapRepository = mapRepository;
        _userRepository = userRepository;
        _layerDataStore = layerDataStore;
        _workspaceRepository = workspaceRepository;
        _mapFeatureRepository = mapFeatureRepository;
        _mapFeatureStore = mapFeatureStore;
        _storyMapRepository = storyMapRepository;
        _hubContext = hubContext;
    }

    public async Task<List<MapGallerySummaryResponse>> GetPublishedMapsAsync(
        MapTemplateCategoryEnum? category,
        string? searchTerm,
        bool? featuredOnly,
        CancellationToken ct = default)
    {
        var filter = Builders<MapGalleryDocument>.Filter.Eq(x => x.Status, MapGalleryStatusEnum.Approved);

        if (category.HasValue)
        {
            filter &= Builders<MapGalleryDocument>.Filter.Eq(x => x.Category, category.Value);
        }

        if (featuredOnly == true)
        {
            filter &= Builders<MapGalleryDocument>.Filter.Eq(x => x.IsFeatured, true);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchFilter = Builders<MapGalleryDocument>.Filter.Or(
                Builders<MapGalleryDocument>.Filter.Regex(x => x.MapName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<MapGalleryDocument>.Filter.Regex(x => x.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<MapGalleryDocument>.Filter.AnyIn(x => x.Tags, new[] { searchTerm })
            );
            filter &= searchFilter;
        }

        var docs = await _collection
            .Find(filter)
            .ToListAsync(ct);

        return docs
            .OrderByDescending(x => x.IsFeatured)
            .ThenByDescending(x => x.PublishedAt ?? x.CreatedAt)
            .Select(doc => doc.ToSummary())
            .ToList();
    }

    public async Task<Option<MapGalleryDetailResponse, Error>> GetPublishedMapByIdAsync(
        string id,
        CancellationToken ct = default)
    {
        var doc = await _collection
            .Find(x => x.Id == id && x.Status == MapGalleryStatusEnum.Approved)
            .FirstOrDefaultAsync(ct);

        if (doc == null)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.NotFound("MapGallery.NotFound", "Bản đồ không tồn tại hoặc chưa được duyệt"));
        }

        // Increment view count
        await IncrementViewCountAsync(id, ct);

        return Option.Some<MapGalleryDetailResponse, Error>(doc.ToDetail());
    }

    public async Task<Option<MapGalleryDetailResponse, Error>> GetPublishedMapByMapIdAsync(
        Guid mapId,
        CancellationToken ct = default)
    {
        var doc = await _collection
            .Find(x => x.MapId == mapId && x.Status == MapGalleryStatusEnum.Approved)
            .FirstOrDefaultAsync(ct);

        if (doc == null)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.NotFound("MapGallery.NotFound", "Bản đồ không tồn tại hoặc chưa được duyệt"));
        }

        await IncrementViewCountAsync(doc.Id, ct);

        return Option.Some<MapGalleryDetailResponse, Error>(doc.ToDetail());
    }

    public async Task<Option<MapGalleryDetailResponse, Error>> SubmitMapAsync(
        Guid userId,
        MapGallerySubmitRequest request,
        CancellationToken ct = default)
    {
        // Check if map exists and belongs to user
        var map = await _mapRepository.GetMapById(request.MapId);
        if (map == null || !map.IsActive)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.NotFound("Map.NotFound", "Bản đồ không tồn tại"));
        }

        if (map.UserId != userId)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.Forbidden("MapGallery.Unauthorized", "Bạn không có quyền submit bản đồ này"));
        }

        // Check if already submitted
        var existing = await _collection
            .Find(x => x.MapId == request.MapId)
            .FirstOrDefaultAsync(ct);

        if (existing != null)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.Conflict("MapGallery.AlreadySubmitted", "Bản đồ này đã được submit"));
        }
        
        var userSubmit = await _userRepository.GetUserByIdAsync(userId, ct);

        var doc = new MapGalleryDocument
        {
            Id = Guid.NewGuid().ToString("N"),
            MapId = request.MapId,
            UserId = userId,
            MapName = request.MapName,
            Description = request.Description,
            PreviewImage = request.PreviewImage,
            Category = request.Category,
            Tags = request.Tags,
            AuthorName = userSubmit.FullName,
            AuthorEmail = userSubmit.Email,
            Status = MapGalleryStatusEnum.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _collection.InsertOneAsync(doc, cancellationToken: ct);

        await _hubContext.Clients.Group("admin").SendAsync("AdminNotification", new
        {
            type = "gallery_submission",
            title = "Gallery submission mới",
            message = $"Có submission gallery mới: {doc.MapName}",
            submissionId = doc.Id,
            mapId = doc.MapId.ToString(),
            authorName = doc.AuthorName,
            createdAt = doc.CreatedAt
        });

        return Option.Some<MapGalleryDetailResponse, Error>(doc.ToDetail());
    }

    public async Task<Option<MapGalleryDetailResponse, Error>> GetMySubmissionAsync(
        Guid userId,
        Guid mapId,
        CancellationToken ct = default)
    {
        var doc = await _collection
            .Find(x => x.MapId == mapId && x.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (doc == null)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.NotFound("MapGallery.NotFound", "Bản đồ chưa được submit"));
        }

        return Option.Some<MapGalleryDetailResponse, Error>(doc.ToDetail());
    }

    public async Task<Option<MapGalleryDetailResponse, Error>> UpdateMySubmissionAsync(
        Guid userId,
        string submissionId,
        MapGalleryUpdateRequest request,
        CancellationToken ct = default)
    {
        var doc = await _collection.Find(x => x.Id == submissionId).FirstOrDefaultAsync(ct);
        if (doc == null)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.NotFound("MapGallery.NotFound", "Submission không tồn tại"));
        }

        if (doc.UserId != userId)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.Forbidden("MapGallery.Unauthorized", "Bạn không có quyền chỉnh sửa submission này"));
        }

        if (doc.Status != MapGalleryStatusEnum.Pending)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.Failure("MapGallery.CannotUpdate", "Chỉ có thể chỉnh sửa submission đang ở trạng thái Pending"));
        }

        if (request.MapName != null) doc.MapName = request.MapName;
        if (request.Description != null) doc.Description = request.Description;
        if (request.PreviewImage != null) doc.PreviewImage = request.PreviewImage;
        if (request.Category.HasValue) doc.Category = request.Category.Value;
        if (request.Tags != null) doc.Tags = request.Tags;
        doc.UpdatedAt = DateTime.UtcNow;

        await _collection.ReplaceOneAsync(x => x.Id == submissionId, doc, cancellationToken: ct);
        return Option.Some<MapGalleryDetailResponse, Error>(doc.ToDetail());
    }

    public async Task<List<MapGallerySummaryResponse>> AdminGetAllSubmissionsAsync(
        MapGalleryStatusEnum? status,
        CancellationToken ct = default)
    {
        var filter = Builders<MapGalleryDocument>.Filter.Empty;
        if (status.HasValue)
        {
            filter = Builders<MapGalleryDocument>.Filter.Eq(x => x.Status, status.Value);
        }

        var docs = await _collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        return docs.Select(doc => doc.ToSummary()).ToList();
    }

    public async Task<Option<MapGalleryDetailResponse, Error>> AdminGetSubmissionByIdAsync(
        string id,
        CancellationToken ct = default)
    {
        var doc = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        if (doc == null)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.NotFound("MapGallery.NotFound", "Submission không tồn tại"));
        }

        return Option.Some<MapGalleryDetailResponse, Error>(doc.ToDetail());
    }

    public async Task<Option<MapGalleryDetailResponse, Error>> AdminApproveOrRejectAsync(
        string id,
        Guid reviewerId,
        MapGalleryApprovalRequest request,
        CancellationToken ct = default)
    {
        var doc = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        if (doc == null)
        {
            return Option.None<MapGalleryDetailResponse, Error>(
                Error.NotFound("MapGallery.NotFound", "Submission không tồn tại"));
        }

        doc.Status = request.Status;
        doc.ReviewedBy = reviewerId;
        doc.ReviewedAt = DateTime.UtcNow;
        doc.IsFeatured = request.IsFeatured;

        if (request.Status == MapGalleryStatusEnum.Rejected)
        {
            doc.RejectionReason = request.RejectionReason;
        }
        else if (request.Status == MapGalleryStatusEnum.Approved)
        {
            doc.PublishedAt = DateTime.UtcNow;
            doc.RejectionReason = null;
        }

        doc.UpdatedAt = DateTime.UtcNow;

        await _collection.ReplaceOneAsync(x => x.Id == id, doc, cancellationToken: ct);

        // Update the actual Map entity if approved
        if (request.Status == MapGalleryStatusEnum.Approved)
        {
            var map = await _mapRepository.GetMapById(doc.MapId);
            if (map != null && map.IsActive)
            {
                map.IsPublic = true;
                
                if (doc.Category.HasValue)
                {
                    map.Category = doc.Category.Value;
                }
                
                map.PublishedAt = DateTime.UtcNow;
                
                map.IsFeatured = doc.IsFeatured;
                
                if (!string.IsNullOrEmpty(doc.PreviewImage))
                {
                    map.PreviewImage = doc.PreviewImage;
                }
                
                map.UpdatedAt = DateTime.UtcNow;
                
                await _mapRepository.UpdateMap(map);
            }
        }

        return Option.Some<MapGalleryDetailResponse, Error>(doc.ToDetail());
    }

    public async Task<Option<bool, Error>> AdminDeleteSubmissionAsync(
        string id,
        CancellationToken ct = default)
    {
        var result = await _collection.DeleteOneAsync(x => x.Id == id, ct);
        if (result.DeletedCount == 0)
        {
            return Option.None<bool, Error>(
                Error.NotFound("MapGallery.NotFound", "Submission không tồn tại"));
        }

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<bool, Error>> IncrementViewCountAsync(
        string id,
        CancellationToken ct = default)
    {
        var update = Builders<MapGalleryDocument>.Update.Inc(x => x.ViewCount, 1);
        var result = await _collection.UpdateOneAsync(
            x => x.Id == id,
            update,
            cancellationToken: ct);

        if (result.MatchedCount == 0)
        {
            return Option.None<bool, Error>(
                Error.NotFound("MapGallery.NotFound", "Submission không tồn tại"));
        }

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<MapGalleryDuplicateResponse, Error>> DuplicateMapFromGalleryAsync(
        Guid userId,
        string galleryId,
        MapGalleryDuplicateRequest request,
        CancellationToken ct = default)
    {
        var galleryDoc = await _collection
            .Find(x => x.Id == galleryId && x.Status == MapGalleryStatusEnum.Approved)
            .FirstOrDefaultAsync(ct);

        if (galleryDoc == null)
        {
            return Option.None<MapGalleryDuplicateResponse, Error>(
                Error.NotFound("MapGallery.NotFound", "Bản đồ không tồn tại hoặc chưa được duyệt"));
        }

        var sourceMap = await _mapRepository.GetMapById(galleryDoc.MapId);
        if (sourceMap == null || !sourceMap.IsActive)
        {
            return Option.None<MapGalleryDuplicateResponse, Error>(
                Error.NotFound("Map.NotFound", "Bản đồ nguồn không tồn tại"));
        }

        var workspaceResult = await ResolveWorkspaceIdAsync(userId, request.WorkspaceId);
        if (!workspaceResult.HasValue)
        {
            return Option.None<MapGalleryDuplicateResponse, Error>(Error.Failure("Workspace.NotFound", "Workspace not found"));
        }

        var workspaceId = workspaceResult.ValueOr(Guid.Empty);

        var newMap = new Map
        {
            MapName = request.CustomName ?? $"{sourceMap.MapName} (Copy)",
            Description = request.CustomDescription ?? sourceMap.Description,
            IsPublic = request.IsPublic,
            UserId = userId,
            WorkspaceId = workspaceId,
            DefaultBounds = sourceMap.DefaultBounds,
            ViewState = sourceMap.ViewState,
            BaseLayer = sourceMap.BaseLayer,
            ParentMapId = sourceMap.MapId,
            CreatedAt = DateTime.UtcNow
        };

        if (request.CustomInitialLatitude.HasValue && request.CustomInitialLongitude.HasValue)
        {
            newMap.DefaultBounds = $"{request.CustomInitialLatitude},{request.CustomInitialLongitude}";
            newMap.ViewState = request.CustomInitialZoom.HasValue
                ? $"{{\"center\":[{request.CustomInitialLatitude},{request.CustomInitialLongitude}],\"zoom\":{request.CustomInitialZoom}}}"
                : $"{{\"center\":[{request.CustomInitialLatitude},{request.CustomInitialLongitude}],\"zoom\":10}}";
        }

        var createResult = await _mapRepository.CreateMap(newMap);
        if (!createResult)
        {
            return Option.None<MapGalleryDuplicateResponse, Error>(
                Error.Failure("Map.CreateFailed", "Failed to create map from gallery"));
        }

        var (layersCreated, layerIdMapping) = await CopyMapLayersToMap(sourceMap.MapId, newMap.MapId, userId);
        
        var imagesCreated = await CopyMapImagesToMap(sourceMap.MapId, newMap.MapId);
        
        var featuresCreated = await CopyMapFeaturesToMap(sourceMap.MapId, newMap.MapId, userId, layerIdMapping);

        var (segmentsCreated, segmentIdMapping, locationIdMapping) = await CopyStoryMapEntities(
            sourceMap.MapId, 
            newMap.MapId, 
            userId, 
            layerIdMapping, 
            ct);

        return Option.Some<MapGalleryDuplicateResponse, Error>(new MapGalleryDuplicateResponse
        {
            MapId = newMap.MapId,
            MapName = newMap.MapName,
            SourceMapName = sourceMap.MapName,
            LayersCreated = layersCreated,
            ImagesCreated = imagesCreated,
            CreatedAt = newMap.CreatedAt
        });
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

        var newWorkspace = new Workspace
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

    private async Task<(int Count, Dictionary<Guid, Guid> LayerIdMapping)> CopyMapLayersToMap(Guid sourceMapId, Guid newMapId, Guid userId)
    {
        var sourceLayers = await _mapRepository.GetMapLayers(sourceMapId);
        int count = 0;
        var layerIdMapping = new Dictionary<Guid, Guid>();
        
        foreach (var sourceLayer in sourceLayers)
        {
            var sourceLayerData = await _layerDataStore.GetDataAsync(sourceLayer);
            var newLayerId = Guid.NewGuid();
            var newLayer = new Layer
            {
                LayerId = newLayerId,
                MapId = newMapId,
                UserId = userId,
                LayerName = sourceLayer.LayerName,
                LayerType = sourceLayer.LayerType,
                SourceType = sourceLayer.SourceType,
                LayerStyle = sourceLayer.LayerStyle,
                FeatureCount = sourceLayer.FeatureCount,
                DataSizeKB = sourceLayer.DataSizeKB,
                DataBounds = sourceLayer.DataBounds,
                CreatedAt = DateTime.UtcNow
            };

            // Create layer in database first
            await _mapRepository.CreateLayer(newLayer);
            
            // Then copy layer data from MongoDB (this will set DataStoreKey)
            if (!string.IsNullOrEmpty(sourceLayerData))
            {
                await _layerDataStore.SetDataAsync(newLayer, sourceLayerData);
                // Update layer to save DataStoreKey that was set by SetDataAsync
                await _mapRepository.UpdateLayer(newLayer);
            }
            
            layerIdMapping[sourceLayer.LayerId] = newLayerId;
            count++;
        }

        return (count, layerIdMapping);
    }

    private async Task<int> CopyMapImagesToMap(Guid sourceMapId, Guid newMapId)
    {
        var sourceImages = await _mapRepository.GetTemplateImages(sourceMapId);
        var imagesCreated = 0;

        foreach (var sourceImage in sourceImages)
        {
            var newImage = new MapImage
            {
                MapId = newMapId,
                ImageName = sourceImage.ImageName,
                ImageUrl = sourceImage.ImageUrl,
                ImageData = sourceImage.ImageData,
                Latitude = sourceImage.Latitude,
                Longitude = sourceImage.Longitude,
                Width = sourceImage.Width,
                Height = sourceImage.Height,
                Rotation = sourceImage.Rotation,
                ZIndex = sourceImage.ZIndex,
                IsVisible = sourceImage.IsVisible,
                Description = sourceImage.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _mapRepository.CreateMapImage(newImage);
            imagesCreated++;
        }

        return imagesCreated;
    }

    private async Task<int> CopyMapFeaturesToMap(Guid sourceMapId, Guid newMapId, Guid userId, Dictionary<Guid, Guid> layerIdMapping)
    {
        // Get source features from SQL
        var sourceFeatures = await _mapRepository.GetMapFeatures(sourceMapId);
        
        // Get source feature documents from MongoDB (contains geometry, style, properties)
        var sourceFeatureDocs = await _mapFeatureStore.GetByMapAsync(sourceMapId);
        
        // Create a dictionary to map old feature IDs to new feature IDs
        var featureIdMapping = new Dictionary<Guid, Guid>();
        
        var featuresCreated = 0;
        var featuresToAdd = new List<MapFeature>();
        var mongoDocsToAdd = new List<MapFeatureDocument>();

        foreach (var sourceFeature in sourceFeatures)
        {
            var newFeatureId = Guid.NewGuid();
            featureIdMapping[sourceFeature.FeatureId] = newFeatureId;

            var newFeature = new MapFeature
            {
                FeatureId = newFeatureId,
                MapId = newMapId,
                LayerId = sourceFeature.LayerId.HasValue && layerIdMapping.ContainsKey(sourceFeature.LayerId.Value)
                    ? layerIdMapping[sourceFeature.LayerId.Value]
                    : null,
                Name = sourceFeature.Name,
                Description = sourceFeature.Description,
                FeatureCategory = sourceFeature.FeatureCategory,
                AnnotationType = sourceFeature.AnnotationType,
                GeometryType = sourceFeature.GeometryType,
                MongoDocumentId = null, // Will be set after creating MongoDB document
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                IsVisible = sourceFeature.IsVisible,
                ZIndex = sourceFeature.ZIndex
            };

            featuresToAdd.Add(newFeature);
            featuresCreated++;
        }

        // Copy MongoDB documents with geometry, style, and properties
        foreach (var sourceDoc in sourceFeatureDocs)
        {
            // Find the corresponding SQL feature to get the new feature ID
            // sourceDoc.Id is the FeatureId as string (from MongoDB document)
            var sourceFeature = sourceFeatures.FirstOrDefault(f => 
                f.FeatureId.ToString() == sourceDoc.Id || 
                f.MongoDocumentId == sourceDoc.Id);
            if (sourceFeature == null) continue;

            var newFeatureId = featureIdMapping[sourceFeature.FeatureId];
            var newLayerId = sourceFeature.LayerId.HasValue && layerIdMapping.ContainsKey(sourceFeature.LayerId.Value)
                ? layerIdMapping[sourceFeature.LayerId.Value]
                : sourceDoc.LayerId;

            var newMongoDoc = new MapFeatureDocument
            {
                Id = newFeatureId.ToString(),
                MapId = newMapId,
                LayerId = newLayerId,
                Name = sourceDoc.Name,
                FeatureCategory = sourceDoc.FeatureCategory,
                AnnotationType = sourceDoc.AnnotationType,
                GeometryType = sourceDoc.GeometryType,
                Geometry = sourceDoc.Geometry, // Copy coordinates/geometry (already as object, will be converted to BsonValue)
                Properties = sourceDoc.Properties != null 
                    ? new Dictionary<string, object>(sourceDoc.Properties) 
                    : null, // Copy properties
                Style = sourceDoc.Style != null 
                    ? new Dictionary<string, object>(sourceDoc.Style) 
                    : null, // Copy style
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                IsVisible = sourceDoc.IsVisible,
                ZIndex = sourceDoc.ZIndex
            };

            mongoDocsToAdd.Add(newMongoDoc);
        }

        // Save SQL features first
        if (featuresToAdd.Any())
        {
            await _mapFeatureRepository.AddRange(featuresToAdd);
        }

        // Save MongoDB documents and update MongoDocumentId in SQL
        foreach (var mongoDoc in mongoDocsToAdd)
        {
            var mongoDocId = await _mapFeatureStore.CreateAsync(mongoDoc);
            
            // Update the MongoDocumentId in the SQL feature
            var feature = featuresToAdd.FirstOrDefault(f => f.FeatureId.ToString() == mongoDoc.Id);
            if (feature != null)
            {
                feature.MongoDocumentId = mongoDocId;
                await _mapFeatureRepository.Update(feature);
            }
        }

        return featuresCreated;
    }

    private async Task<(int SegmentsCreated, Dictionary<Guid, Guid> SegmentIdMapping, Dictionary<Guid, Guid> LocationIdMapping)> CopyStoryMapEntities(
        Guid sourceMapId,
        Guid newMapId,
        Guid userId,
        Dictionary<Guid, Guid> layerIdMapping,
        CancellationToken ct)
    {
        var segmentIdMapping = new Dictionary<Guid, Guid>();
        var locationIdMapping = new Dictionary<Guid, Guid>();
        var zoneIdMapping = new Dictionary<Guid, Guid>();
        int segmentsCreated = 0;

        var sourceSegments = await _storyMapRepository.GetSegmentsByMapAsync(sourceMapId, ct);
        foreach (var sourceSegment in sourceSegments.OrderBy(s => s.DisplayOrder))
        {
            var newSegmentId = Guid.NewGuid();
            var newSegment = new Segment
            {
                SegmentId = newSegmentId,
                MapId = newMapId,
                CreatedBy = userId,
                Name = sourceSegment.Name,
                Description = sourceSegment.Description,
                StoryContent = sourceSegment.StoryContent,
                DisplayOrder = sourceSegment.DisplayOrder,
                CameraState = sourceSegment.CameraState,
                AutoAdvance = sourceSegment.AutoAdvance,
                DurationMs = sourceSegment.DurationMs,
                RequireUserAction = sourceSegment.RequireUserAction,
                CreatedAt = DateTime.UtcNow
            };

            await _storyMapRepository.AddSegmentAsync(newSegment, ct);
            segmentIdMapping[sourceSegment.SegmentId] = newSegmentId;
            segmentsCreated++;
        }

        foreach (var sourceSegment in sourceSegments)
        {
            if (!segmentIdMapping.ContainsKey(sourceSegment.SegmentId)) continue;

            var newSegmentId = segmentIdMapping[sourceSegment.SegmentId];
            var sourceSegmentZones = await _storyMapRepository.GetSegmentZonesBySegmentAsync(sourceSegment.SegmentId, ct);

            foreach (var sourceSegmentZone in sourceSegmentZones)
            {
                var newSegmentZone = new SegmentZone
                {
                    SegmentZoneId = Guid.NewGuid(),
                    SegmentId = newSegmentId,
                    ZoneId = sourceSegmentZone.ZoneId,
                    DisplayOrder = sourceSegmentZone.DisplayOrder,
                    IsVisible = sourceSegmentZone.IsVisible,
                    ZIndex = sourceSegmentZone.ZIndex,
                    HighlightBoundary = sourceSegmentZone.HighlightBoundary,
                    BoundaryColor = sourceSegmentZone.BoundaryColor,
                    BoundaryWidth = sourceSegmentZone.BoundaryWidth,
                    FillZone = sourceSegmentZone.FillZone,
                    FillColor = sourceSegmentZone.FillColor,
                    FillOpacity = sourceSegmentZone.FillOpacity,
                    ShowLabel = sourceSegmentZone.ShowLabel,
                    LabelOverride = sourceSegmentZone.LabelOverride,
                    LabelStyle = sourceSegmentZone.LabelStyle,
                    EntryDelayMs = sourceSegmentZone.EntryDelayMs,
                    EntryDurationMs = sourceSegmentZone.EntryDurationMs,
                    ExitDelayMs = sourceSegmentZone.ExitDelayMs,
                    ExitDurationMs = sourceSegmentZone.ExitDurationMs,
                    EntryEffect = sourceSegmentZone.EntryEffect,
                    ExitEffect = sourceSegmentZone.ExitEffect,
                    FitBoundsOnEntry = sourceSegmentZone.FitBoundsOnEntry,
                    CameraOverride = sourceSegmentZone.CameraOverride,
                    CreatedAt = DateTime.UtcNow
                };

                await _storyMapRepository.AddSegmentZoneAsync(newSegmentZone, ct);
            }
        }

        foreach (var sourceSegment in sourceSegments)
        {
            if (!segmentIdMapping.ContainsKey(sourceSegment.SegmentId)) continue;

            var newSegmentId = segmentIdMapping[sourceSegment.SegmentId];
            var sourceSegmentLayers = await _storyMapRepository.GetSegmentLayersBySegmentAsync(sourceSegment.SegmentId, ct);

            foreach (var sourceSegmentLayer in sourceSegmentLayers)
            {
                if (!layerIdMapping.ContainsKey(sourceSegmentLayer.LayerId)) continue;

                var newSegmentLayer = new SegmentLayer
                {
                    SegmentLayerId = Guid.NewGuid(),
                    SegmentId = newSegmentId,
                    LayerId = layerIdMapping[sourceSegmentLayer.LayerId],
                    DisplayOrder = sourceSegmentLayer.DisplayOrder,
                    IsVisible = sourceSegmentLayer.IsVisible,
                    Opacity = sourceSegmentLayer.Opacity,
                    ZIndex = sourceSegmentLayer.ZIndex,
                    EntryDelayMs = sourceSegmentLayer.EntryDelayMs,
                    EntryDurationMs = sourceSegmentLayer.EntryDurationMs,
                    ExitDelayMs = sourceSegmentLayer.ExitDelayMs,
                    ExitDurationMs = sourceSegmentLayer.ExitDurationMs,
                    EntryEffect = sourceSegmentLayer.EntryEffect,
                    ExitEffect = sourceSegmentLayer.ExitEffect,
                    StyleOverride = sourceSegmentLayer.StyleOverride,
                    CreatedAt = DateTime.UtcNow
                };

                await _storyMapRepository.AddSegmentLayerAsync(newSegmentLayer, ct);
            }
        }

        var sourceLocations = await _storyMapRepository.GetLocationsByMapAsync(sourceMapId, ct);
        var locationsToUpdate = new List<(Location location, Guid? linkedLocationId)>();
        
        foreach (var sourceLocation in sourceLocations)
        {
            var newLocationId = Guid.NewGuid();
            var newLocation = new Location
            {
                LocationId = newLocationId,
                MapId = newMapId,
                SegmentId = sourceLocation.SegmentId.HasValue && segmentIdMapping.ContainsKey(sourceLocation.SegmentId.Value)
                    ? segmentIdMapping[sourceLocation.SegmentId.Value]
                    : null,
                ZoneId = sourceLocation.ZoneId,
                CreatedBy = userId,
                Title = sourceLocation.Title,
                Subtitle = sourceLocation.Subtitle,
                Description = sourceLocation.Description,
                LocationType = sourceLocation.LocationType,
                DisplayOrder = sourceLocation.DisplayOrder,
                MarkerGeometry = sourceLocation.MarkerGeometry,
                IconType = sourceLocation.IconType,
                IconUrl = sourceLocation.IconUrl,
                IconColor = sourceLocation.IconColor,
                IconSize = sourceLocation.IconSize,
                ZIndex = sourceLocation.ZIndex,
                ShowTooltip = sourceLocation.ShowTooltip,
                TooltipContent = sourceLocation.TooltipContent,
                OpenPopupOnClick = sourceLocation.OpenPopupOnClick,
                PopupContent = sourceLocation.PopupContent,
                MediaUrls = sourceLocation.MediaUrls,
                PlayAudioOnClick = sourceLocation.PlayAudioOnClick,
                AudioUrl = sourceLocation.AudioUrl,
                EntryDelayMs = sourceLocation.EntryDelayMs,
                EntryDurationMs = sourceLocation.EntryDurationMs,
                ExitDelayMs = sourceLocation.ExitDelayMs,
                ExitDurationMs = sourceLocation.ExitDurationMs,
                EntryEffect = sourceLocation.EntryEffect,
                ExitEffect = sourceLocation.ExitEffect,
                LinkedLocationId = null,
                ExternalUrl = sourceLocation.ExternalUrl,
                IsVisible = sourceLocation.IsVisible,
                CreatedAt = DateTime.UtcNow
            };

            await _storyMapRepository.AddLocationAsync(newLocation, ct);
            locationIdMapping[sourceLocation.LocationId] = newLocationId;
            
            if (sourceLocation.LinkedLocationId.HasValue)
            {
                locationsToUpdate.Add((newLocation, sourceLocation.LinkedLocationId));
            }
        }

        foreach (var (location, sourceLinkedLocationId) in locationsToUpdate)
        {
            if (sourceLinkedLocationId.HasValue && locationIdMapping.ContainsKey(sourceLinkedLocationId.Value))
            {
                location.LinkedLocationId = locationIdMapping[sourceLinkedLocationId.Value];
                _storyMapRepository.UpdateLocation(location);
            }
        }

        var sourceTransitions = await _storyMapRepository.GetTimelineTransitionsByMapAsync(sourceMapId, ct);
        foreach (var sourceTransition in sourceTransitions)
        {
            if (!segmentIdMapping.ContainsKey(sourceTransition.FromSegmentId) ||
                !segmentIdMapping.ContainsKey(sourceTransition.ToSegmentId))
                continue;

            var newTransition = new TimelineTransition
            {
                TimelineTransitionId = Guid.NewGuid(),
                MapId = newMapId,
                FromSegmentId = segmentIdMapping[sourceTransition.FromSegmentId],
                ToSegmentId = segmentIdMapping[sourceTransition.ToSegmentId],
                TransitionName = sourceTransition.TransitionName,
                DurationMs = sourceTransition.DurationMs,
                TransitionType = sourceTransition.TransitionType,
                AnimateCamera = sourceTransition.AnimateCamera,
                CameraAnimationType = sourceTransition.CameraAnimationType,
                CameraAnimationDurationMs = sourceTransition.CameraAnimationDurationMs,
                ShowOverlay = sourceTransition.ShowOverlay,
                OverlayContent = sourceTransition.OverlayContent,
                AutoTrigger = sourceTransition.AutoTrigger,
                RequireUserAction = sourceTransition.RequireUserAction,
                TriggerButtonText = sourceTransition.TriggerButtonText,
                CreatedAt = DateTime.UtcNow
            };

            await _storyMapRepository.AddTimelineTransitionAsync(newTransition, ct);
        }

        var sourceRouteAnimations = await _storyMapRepository.GetRouteAnimationsByMapAsync(sourceMapId, ct);
        foreach (var sourceRouteAnimation in sourceRouteAnimations)
        {
            if (!segmentIdMapping.ContainsKey(sourceRouteAnimation.SegmentId))
                continue;

            var newRouteAnimation = new RouteAnimation
            {
                RouteAnimationId = Guid.NewGuid(),
                SegmentId = segmentIdMapping[sourceRouteAnimation.SegmentId],
                MapId = newMapId,
                FromLat = sourceRouteAnimation.FromLat,
                FromLng = sourceRouteAnimation.FromLng,
                FromName = sourceRouteAnimation.FromName,
                ToLat = sourceRouteAnimation.ToLat,
                ToLng = sourceRouteAnimation.ToLng,
                ToName = sourceRouteAnimation.ToName,
                ToLocationId = sourceRouteAnimation.ToLocationId.HasValue && locationIdMapping.ContainsKey(sourceRouteAnimation.ToLocationId.Value)
                    ? locationIdMapping[sourceRouteAnimation.ToLocationId.Value]
                    : null,
                RoutePath = sourceRouteAnimation.RoutePath,
                Waypoints = sourceRouteAnimation.Waypoints,
                IconType = sourceRouteAnimation.IconType,
                IconUrl = sourceRouteAnimation.IconUrl,
                IconWidth = sourceRouteAnimation.IconWidth,
                IconHeight = sourceRouteAnimation.IconHeight,
                RouteColor = sourceRouteAnimation.RouteColor,
                VisitedColor = sourceRouteAnimation.VisitedColor,
                RouteWidth = sourceRouteAnimation.RouteWidth,
                DurationMs = sourceRouteAnimation.DurationMs,
                StartDelayMs = sourceRouteAnimation.StartDelayMs,
                Easing = sourceRouteAnimation.Easing,
                AutoPlay = sourceRouteAnimation.AutoPlay,
                Loop = sourceRouteAnimation.Loop,
                IsVisible = sourceRouteAnimation.IsVisible,
                ZIndex = sourceRouteAnimation.ZIndex,
                DisplayOrder = sourceRouteAnimation.DisplayOrder,
                StartTimeMs = sourceRouteAnimation.StartTimeMs,
                EndTimeMs = sourceRouteAnimation.EndTimeMs,
                CameraStateBefore = sourceRouteAnimation.CameraStateBefore,
                CameraStateAfter = sourceRouteAnimation.CameraStateAfter,
                ShowLocationInfoOnArrival = sourceRouteAnimation.ShowLocationInfoOnArrival,
                LocationInfoDisplayDurationMs = sourceRouteAnimation.LocationInfoDisplayDurationMs,
                FollowCamera = sourceRouteAnimation.FollowCamera,
                FollowCameraZoom = sourceRouteAnimation.FollowCameraZoom,
                CreatedAt = DateTime.UtcNow
            };

            await _storyMapRepository.AddRouteAnimationAsync(newRouteAnimation, ct);
        }

        // 7. Copy AnimatedLayers (need to map layerId and segmentId)
        var sourceAnimatedLayers = await _storyMapRepository.GetAnimatedLayersByMapAsync(sourceMapId, ct);
        foreach (var sourceAnimatedLayer in sourceAnimatedLayers)
        {
            var newAnimatedLayer = new AnimatedLayer
            {
                AnimatedLayerId = Guid.NewGuid(),
                LayerId = sourceAnimatedLayer.LayerId.HasValue && layerIdMapping.ContainsKey(sourceAnimatedLayer.LayerId.Value)
                    ? layerIdMapping[sourceAnimatedLayer.LayerId.Value]
                    : null,
                SegmentId = sourceAnimatedLayer.SegmentId.HasValue && segmentIdMapping.ContainsKey(sourceAnimatedLayer.SegmentId.Value)
                    ? segmentIdMapping[sourceAnimatedLayer.SegmentId.Value]
                    : null,
                CreatedBy = userId,
                Name = sourceAnimatedLayer.Name,
                Description = sourceAnimatedLayer.Description,
                DisplayOrder = sourceAnimatedLayer.DisplayOrder,
                MediaType = sourceAnimatedLayer.MediaType,
                SourceUrl = sourceAnimatedLayer.SourceUrl,
                ThumbnailUrl = sourceAnimatedLayer.ThumbnailUrl,
                Coordinates = sourceAnimatedLayer.Coordinates,
                IsScreenOverlay = sourceAnimatedLayer.IsScreenOverlay,
                ScreenPosition = sourceAnimatedLayer.ScreenPosition,
                RotationDeg = sourceAnimatedLayer.RotationDeg,
                Scale = sourceAnimatedLayer.Scale,
                Opacity = sourceAnimatedLayer.Opacity,
                ZIndex = sourceAnimatedLayer.ZIndex,
                IsVisible = sourceAnimatedLayer.IsVisible,
                CreatedAt = DateTime.UtcNow
            };

            await _storyMapRepository.AddAnimatedLayerAsync(newAnimatedLayer, ct);
        }

        // Save all changes
        await _storyMapRepository.SaveChangesAsync(ct);

        return (segmentsCreated, segmentIdMapping, locationIdMapping);
    }
}

