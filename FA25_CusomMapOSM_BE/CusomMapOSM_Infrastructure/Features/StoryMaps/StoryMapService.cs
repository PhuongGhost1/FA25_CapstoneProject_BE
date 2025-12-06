using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using CusomMapOSM_Application.Common.Mappers;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.StoryMaps;
using CusomMapOSM_Application.Interfaces.Services.LayerData;
using CusomMapOSM_Application.Interfaces.Services.MapFeatures;
using CusomMapOSM_Application.Interfaces.Services.OSM;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.Documents;
using CusomMapOSM_Application.Models.DTOs.Features.Locations;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Domain.Entities.Animations;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.Locations;
using CusomMapOSM_Domain.Entities.Locations.Enums;
using CusomMapOSM_Domain.Entities.Maps.ErrorMessages;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Segments.Enums;
using CusomMapOSM_Domain.Entities.Timeline;
using CusomMapOSM_Domain.Entities.Timeline.Enums;
using CusomMapOSM_Domain.Entities.Zones;
using CusomMapOSM_Domain.Entities.Zones.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.StoryMaps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Locations;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.StoryMaps;

public class StoryMapService : IStoryMapService
{
    private readonly IStoryMapRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILocationRepository _locationRepository;
    private readonly IMapRepository _mapRepository;
    private readonly IOsmService _osmService;
    private readonly ILayerDataStore _layerDataStore;
    private readonly IMapFeatureRepository _mapFeatureRepository;
    private readonly IMapFeatureStore _mapFeatureStore;

    public StoryMapService(IStoryMapRepository repository, ICurrentUserService currentUserService,
        ILocationRepository locationRepository, IMapRepository mapRepository, IOsmService osmService,
        ILayerDataStore layerDataStore, IMapFeatureRepository mapFeatureRepository, IMapFeatureStore mapFeatureStore)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _locationRepository = locationRepository;
        _mapRepository = mapRepository;
        _osmService = osmService;
        _layerDataStore = layerDataStore;
        _mapFeatureRepository = mapFeatureRepository;
        _mapFeatureStore = mapFeatureStore;
    }


    public async Task<Option<IReadOnlyCollection<SegmentDto>, Error>> GetSegmentsAsync(Guid mapId,
        CancellationToken ct = default)
    {
        var map = await _repository.GetMapAsync(mapId, ct);
        if (map is null)
        {
            return Option.None<IReadOnlyCollection<SegmentDto>, Error>(Error.NotFound("StoryMap.Map.NotFound",
                MapErrors.MapNotFound));
        }

        var segments = await _repository.GetSegmentsByMapAsync(mapId, ct);
        if (segments.Count == 0)
        {
            return Option.Some<IReadOnlyCollection<SegmentDto>, Error>(Array.Empty<SegmentDto>());
        }
        
        var segmentIds = segments.Select(s => s.SegmentId).ToList();

        var allZones = await _repository.GetSegmentZonesBySegmentsAsync(segmentIds, ct);
        var allLayers = await _repository.GetSegmentLayersBySegmentsAsync(segmentIds, ct);
        var allLocations = await _locationRepository.GetBySegmentIdsAsync(segmentIds, ct);
        
        var zonesBySegment = allZones
            .GroupBy(sz => sz.SegmentId)
            .ToDictionary(g => g.Key, g => g.Select(sz => sz.ToDto()).ToList());
        var enrichedLayers = await BuildSegmentLayerDtosAsync(mapId, allLayers, ct);
        var layersBySegment = enrichedLayers
            .GroupBy(sl => sl.SegmentId)
            .ToDictionary(g => g.Key, g => (IReadOnlyCollection<SegmentLayerDto>)g.ToList());
        var locationsBySegment = allLocations
            .GroupBy(l => l.SegmentId)
            .ToDictionary(g => g.Key, g => g.Select(loc => loc.ToDto()).ToList());

        var segmentDtos = new List<SegmentDto>(segments.Count);
        foreach (var segment in segments)
        {
            var zones = zonesBySegment.GetValueOrDefault(segment.SegmentId) ?? new List<SegmentZoneDto>();
            var layers = layersBySegment.GetValueOrDefault(segment.SegmentId) ?? Array.Empty<SegmentLayerDto>();
            var locations = locationsBySegment.GetValueOrDefault(segment.SegmentId) ?? new List<LocationDto>();

            var dto = segment.ToSegmentDto(zones, layers, locations);
            segmentDtos.Add(dto);
        }

        return Option.Some<IReadOnlyCollection<SegmentDto>, Error>(segmentDtos);
    }

    public async Task<Option<SegmentDto, Error>> GetSegmentAsync(Guid segmentId, CancellationToken ct = default)
    {
        var segment = await _repository.GetSegmentAsync(segmentId, ct);
        if (segment is null)
        {
            return Option.None<SegmentDto, Error>(Error.NotFound("Segment.NotFound", "Segment not found"));
        }
        
        var segmentZones = await _repository.GetSegmentZonesBySegmentAsync(segmentId, ct);
        var segmentLayers = await _repository.GetSegmentLayersBySegmentAsync(segmentId, ct);
        var locations = await _locationRepository.GetBySegmentIdAsync(segmentId, ct);

        var zones = segmentZones.Select(sz => sz.ToDto()).ToList();
        var layers = await BuildSegmentLayerDtosAsync(segment.MapId, segmentLayers, ct);
        var pois = locations.Select(l => l.ToDto()).ToList();

        return Option.Some<SegmentDto, Error>(segment.ToSegmentDto(zones, layers, pois));
    }

    public async Task<Option<SegmentDto, Error>> CreateSegmentAsync(CreateSegmentRequest request,
        CancellationToken ct = default)
    {
        var map = await _repository.GetMapAsync(request.MapId, ct);
        if (map is null)
        {
            return Option.None<SegmentDto, Error>(Error.NotFound("StoryMap.Map.NotFound", MapErrors.MapNotFound));
        }

        var userId = _currentUserService.GetUserId();

        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var name = string.IsNullOrWhiteSpace(request.Name) ? "Untitled Segment" : request.Name.Trim();
        var displayOrder = request.DisplayOrder < 0 ? 0 : request.DisplayOrder;
        
        var segment = new Segment
        {
            SegmentId = Guid.NewGuid(),
            MapId = request.MapId,
            CreatedBy = userId.Value,
            Name = name,
            Description = request.Description,
            StoryContent = request.StoryContent,
            DisplayOrder = displayOrder,
            CameraState = request.CameraState ?? string.Empty,
            AutoAdvance = request.AutoAdvance,
            DurationMs = 6000,
            RequireUserAction = request.RequireUserAction,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddSegmentAsync(segment, ct);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<SegmentDto, Error>(segment.ToSegmentDto());
    }

    public async Task<Option<SegmentDto, Error>> UpdateSegmentAsync(Guid segmentId, UpdateSegmentRequest request,
        CancellationToken ct = default)
    {
        // Validate request
        var validationResult = SegmentValidator.ValidateUpdateRequest(request);
        if (validationResult.Match(some: _ => false, none: err => true))
        {
            return validationResult.Match<Option<SegmentDto, Error>>(
                some: _ => throw new InvalidOperationException(),
                none: err => Option.None<SegmentDto, Error>(err)
            );
        }

        // Validate segment exists
        var segment = await _repository.GetSegmentAsync(segmentId, ct);
        if (segment is null)
        {
            return Option.None<SegmentDto, Error>(
                Error.NotFound("StoryMap.Segment.NotFound", "Segment not found"));
        }

        segment.Name = request.Name.Trim();
        
        if (request.Description != null)
        {
            segment.Description = request.Description;
        }
        
        if (request.StoryContent != null)
        {
            segment.StoryContent = request.StoryContent;
        }
        
        if (request.DisplayOrder.HasValue)
        {
            segment.DisplayOrder = request.DisplayOrder.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.CameraState))
        {
            segment.CameraState = request.CameraState;
        }

        if (request.AutoAdvance.HasValue)
        {
            segment.AutoAdvance = request.AutoAdvance.Value;
        }

        if (request.DurationMs.HasValue)
        {
            segment.DurationMs = request.DurationMs.Value;
        }

        if (request.RequireUserAction.HasValue)
        {
            segment.RequireUserAction = request.RequireUserAction.Value;
        }

        if (request.PlaybackMode.HasValue)
        {
            var autoAdvance = request.PlaybackMode.Value != 
                SegmentPlaybackMode.Manual;
            var requireUserAction = request.PlaybackMode.Value == 
                SegmentPlaybackMode.Manual;
            
            segment.AutoAdvance = autoAdvance;
            segment.RequireUserAction = requireUserAction;
        }

        // Set updated timestamp
        segment.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateSegment(segment);
        await _repository.SaveChangesAsync(ct);

        var segmentZones = await _repository.GetSegmentZonesBySegmentAsync(segmentId, ct);
        var zoneDtos = segmentZones.Select(sz => sz.ToDto()).ToList();

        var segmentLayers = await _repository.GetSegmentLayersBySegmentAsync(segmentId, ct);
        var layerDtos = segmentLayers.Select(sl => sl.ToDto()).ToList();

        var locations = await _locationRepository.GetBySegmentIdAsync(segmentId, ct);
        var poiDtos = locations.Select(loc => loc.ToDto()).ToList();

        return Option.Some<SegmentDto, Error>(segment.ToSegmentDto(zoneDtos, layerDtos, poiDtos));
    }

    public async Task<Option<bool, Error>> DeleteSegmentAsync(Guid segmentId, CancellationToken ct = default)
    {
        var segment = await _repository.GetSegmentAsync(segmentId, ct);
        if (segment is null)
        {
            return Option.None<bool, Error>(Error.NotFound("StoryMap.Segment.NotFound", "Segment not found"));
        }

        // Check if segment is used in timeline transitions
        var transitions = await _repository.GetTimelineTransitionsByMapAsync(segment.MapId, ct);
        var transitionsUsingSegment = transitions
            .Where(t => t.FromSegmentId == segmentId || t.ToSegmentId == segmentId)
            .ToList();
        
        if (transitionsUsingSegment.Any())
        {
            return Option.None<bool, Error>(
                Error.ValidationError("Segment.HasTransitions", 
                    "Cannot delete segment while it is being used in timeline transitions. Please delete related transitions first."));
        }

        // Check if segment has route animations
        var routeAnimations = await _repository.GetRouteAnimationsBySegmentAsync(segmentId, ct);
        if (routeAnimations.Any())
        {
            return Option.None<bool, Error>(
                Error.ValidationError("Segment.HasRouteAnimations", 
                    "Cannot delete segment while it has route animations. Please delete route animations first."));
        }

        // Check if segment has animated layers
        var animatedLayers = await _repository.GetAnimatedLayersBySegmentAsync(segmentId, ct);
        if (animatedLayers.Any())
        {
            return Option.None<bool, Error>(
                Error.ValidationError("Segment.HasAnimatedLayers", 
                    "Cannot delete segment while it has animated layers. Please delete animated layers first."));
        }

        _repository.RemoveSegment(segment);
        await _repository.SaveChangesAsync(ct);
        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<SegmentDto, Error>> DuplicateSegmentAsync(Guid segmentId, CancellationToken ct = default)
    {
        var originalSegment = await _repository.GetSegmentAsync(segmentId, ct);
        if (originalSegment is null)
        {
            return Option.None<SegmentDto, Error>(Error.NotFound("Segment.NotFound", "Segment not found"));
        }

        var userId = _currentUserService.GetUserId();
        if (!userId.HasValue)
        {
            return Option.None<SegmentDto, Error>(Error.Unauthorized("User.NotAuthenticated",
                "User is not authenticated"));
        }

        var newSegment = new Segment
        {
            SegmentId = Guid.NewGuid(),
            MapId = originalSegment.MapId,
            CreatedBy = userId.Value,
            Name = $"{originalSegment.Name} (Copy)",
            Description = originalSegment.Description,
            StoryContent = originalSegment.StoryContent,
            DisplayOrder = originalSegment.DisplayOrder + 1,
            CameraState = originalSegment.CameraState,
            AutoAdvance = originalSegment.AutoAdvance,
            DurationMs = originalSegment.DurationMs,
            RequireUserAction = originalSegment.RequireUserAction,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddSegmentAsync(newSegment, ct);

        var originalZones = await _repository.GetSegmentZonesBySegmentAsync(segmentId, ct);
        foreach (var originalZone in originalZones)
        {
            var newZone = new SegmentZone
            {
                SegmentZoneId = Guid.NewGuid(),
                SegmentId = newSegment.SegmentId,
                ZoneId = originalZone.ZoneId,
                DisplayOrder = originalZone.DisplayOrder,
                IsVisible = originalZone.IsVisible,
                ZIndex = originalZone.ZIndex,
                HighlightBoundary = originalZone.HighlightBoundary,
                BoundaryColor = originalZone.BoundaryColor,
                BoundaryWidth = originalZone.BoundaryWidth,
                FillZone = originalZone.FillZone,
                FillColor = originalZone.FillColor,
                FillOpacity = originalZone.FillOpacity,
                ShowLabel = originalZone.ShowLabel,
                LabelOverride = originalZone.LabelOverride,
                LabelStyle = originalZone.LabelStyle,
                EntryDelayMs = originalZone.EntryDelayMs,
                EntryDurationMs = originalZone.EntryDurationMs,
                ExitDelayMs = originalZone.ExitDelayMs,
                ExitDurationMs = originalZone.ExitDurationMs,
                EntryEffect = originalZone.EntryEffect,
                ExitEffect = originalZone.ExitEffect,
                FitBoundsOnEntry = originalZone.FitBoundsOnEntry,
                CameraOverride = originalZone.CameraOverride,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddSegmentZoneAsync(newZone, ct);
        }

        var originalLayers = await _repository.GetSegmentLayersBySegmentAsync(segmentId, ct);
        foreach (var originalLayer in originalLayers)
        {
            var newLayer = new SegmentLayer
            {
                SegmentLayerId = Guid.NewGuid(),
                SegmentId = newSegment.SegmentId,
                LayerId = originalLayer.LayerId,
                DisplayOrder = originalLayer.DisplayOrder,
                IsVisible = originalLayer.IsVisible,
                Opacity = originalLayer.Opacity,
                ZIndex = originalLayer.ZIndex,
                EntryDelayMs = originalLayer.EntryDelayMs,
                EntryDurationMs = originalLayer.EntryDurationMs,
                ExitDelayMs = originalLayer.ExitDelayMs,
                ExitDurationMs = originalLayer.ExitDurationMs,
                EntryEffect = originalLayer.EntryEffect,
                ExitEffect = originalLayer.ExitEffect,
                StyleOverride = originalLayer.StyleOverride,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddSegmentLayerAsync(newLayer, ct);
        }

        var originalLocations = await _locationRepository.GetBySegmentIdAsync(segmentId, ct);
        foreach (var originalLocation in originalLocations)
        {
            var newLocation = new Location
            {
                LocationId = Guid.NewGuid(),
                SegmentId = newSegment.SegmentId,
                Title = originalLocation.Title,
                Subtitle = originalLocation.Subtitle,
                Description = originalLocation.Description,
                LocationType = originalLocation.LocationType,
                MarkerGeometry = originalLocation.MarkerGeometry,
                IconType = originalLocation.IconType,
                IconUrl = originalLocation.IconUrl,
                IconColor = originalLocation.IconColor,
                IconSize = originalLocation.IconSize,
                DisplayOrder = originalLocation.DisplayOrder,
                ShowTooltip = originalLocation.ShowTooltip,
                TooltipContent = originalLocation.TooltipContent,
                OpenPopupOnClick = originalLocation.OpenPopupOnClick,
                PopupContent = originalLocation.PopupContent,
                MediaUrls = originalLocation.MediaUrls,
                PlayAudioOnClick = originalLocation.PlayAudioOnClick,
                AudioUrl = originalLocation.AudioUrl,
                EntryDelayMs = originalLocation.EntryDelayMs,
                EntryDurationMs = originalLocation.EntryDurationMs,
                ExitDelayMs = originalLocation.ExitDelayMs,
                ExitDurationMs = originalLocation.ExitDurationMs,
                EntryEffect = originalLocation.EntryEffect,
                ExitEffect = originalLocation.ExitEffect,
                LinkedLocationId = originalLocation.LinkedLocationId,
                ExternalUrl = originalLocation.ExternalUrl,
                IsVisible = originalLocation.IsVisible,
                ZIndex = originalLocation.ZIndex,
                CreatedBy = userId.Value,
                CreatedAt = DateTime.UtcNow
            };
            await _locationRepository.CreateAsync(newLocation, ct);
        }

        var originalAnimatedLayers = await _repository.GetAnimatedLayersBySegmentAsync(segmentId, ct);
        foreach (var originalAnimatedLayer in originalAnimatedLayers)
        {
            var newAnimatedLayer = new AnimatedLayer
            {
                AnimatedLayerId = Guid.NewGuid(),
                CreatedBy = userId.Value,
                LayerId = originalAnimatedLayer.LayerId,
                SegmentId = newSegment.SegmentId, // Point to new segment
                Name = originalAnimatedLayer.Name,
                Description = originalAnimatedLayer.Description,
                DisplayOrder = originalAnimatedLayer.DisplayOrder,
                MediaType = originalAnimatedLayer.MediaType,
                SourceUrl = originalAnimatedLayer.SourceUrl,
                ThumbnailUrl = originalAnimatedLayer.ThumbnailUrl,
                Coordinates = originalAnimatedLayer.Coordinates,
                IsScreenOverlay = originalAnimatedLayer.IsScreenOverlay,
                ScreenPosition = originalAnimatedLayer.ScreenPosition,
                RotationDeg = originalAnimatedLayer.RotationDeg,
                Scale = originalAnimatedLayer.Scale,
                Opacity = originalAnimatedLayer.Opacity,
                ZIndex = originalAnimatedLayer.ZIndex,
                IsVisible = originalAnimatedLayer.IsVisible,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAnimatedLayerAsync(newAnimatedLayer, ct);
        }

        await _repository.SaveChangesAsync(ct);

        return await GetSegmentAsync(newSegment.SegmentId, ct);
    }

    public async Task<Option<bool, Error>> ReorderSegmentsAsync(Guid mapId, List<Guid> segmentIds,
        CancellationToken ct = default)
    {
        var map = await _repository.GetMapAsync(mapId, ct);
        if (map is null)
        {
            return Option.None<bool, Error>(Error.NotFound("Map.NotFound", "Map not found"));
        }

        var allSegments = await _repository.GetSegmentsByMapAsync(mapId, ct);
        var existingIds = allSegments.Select(s => s.SegmentId).ToHashSet();
        var invalidIds = segmentIds.Where(id => !existingIds.Contains(id)).ToList();
        if (invalidIds.Any())
        {
            return Option.None<bool, Error>(Error.ValidationError("Segment.InvalidIds",
                "Some segment IDs are invalid"));
        }

        for (int i = 0; i < segmentIds.Count; i++)
        {
            var segmentId = segmentIds[i];
            var segment = await _repository.GetSegmentForUpdateAsync(segmentId, ct);
            if (segment is null)
            {
                return Option.None<bool, Error>(Error.NotFound("Segment.NotFound", 
                    $"Segment {segmentId} not found"));
            }

            segment.DisplayOrder = i;
            segment.UpdatedAt = DateTime.UtcNow;
            _repository.UpdateSegment(segment);
        }

        await _repository.SaveChangesAsync(ct);

        return Option.Some<bool, Error>(true);
    }


    public async Task<Option<IReadOnlyCollection<SegmentZoneDto>, Error>> GetSegmentZonesAsync(Guid segmentId,
        CancellationToken ct = default)
    {
        var segment = await _repository.GetSegmentAsync(segmentId, ct);
        if (segment is null)
        {
            return Option.None<IReadOnlyCollection<SegmentZoneDto>, Error>(Error.NotFound("StoryMap.Segment.NotFound",
                "Segment not found"));
        }

        var zones = await _repository.GetSegmentZonesBySegmentAsync(segmentId, ct);
        var zoneDtos = zones.Select(z => z.ToDto()).ToList();
        return Option.Some<IReadOnlyCollection<SegmentZoneDto>, Error>(zoneDtos);
    }

    public async Task<Option<SegmentZoneDto, Error>> CreateSegmentZoneAsync(CreateSegmentZoneV2Request request,
        CancellationToken ct = default)
    {
        var segment = await _repository.GetSegmentAsync(request.SegmentId, ct);
        if (segment is null)
        {
            return Option.None<SegmentZoneDto, Error>(Error.NotFound("Segment.NotFound", "Segment not found"));
        }

        var zone = await _repository.GetZoneAsync(request.ZoneId, ct);
        if (zone is null)
        {
            return Option.None<SegmentZoneDto, Error>(Error.NotFound("Zone.NotFound", "Zone not found"));
        }

        var segmentZone = new SegmentZone
        {
            SegmentZoneId = Guid.NewGuid(),
            SegmentId = request.SegmentId,
            ZoneId = request.ZoneId,
            DisplayOrder = request.DisplayOrder,
            IsVisible = request.IsVisible,
            ZIndex = request.ZIndex,
            HighlightBoundary = request.HighlightBoundary,
            BoundaryColor = request.BoundaryColor,
            BoundaryWidth = request.BoundaryWidth,
            FillZone = request.FillZone,
            FillColor = request.FillColor,
            FillOpacity = request.FillOpacity,
            ShowLabel = request.ShowLabel,
            LabelOverride = request.LabelOverride,
            LabelStyle = request.LabelStyle,
            EntryDelayMs = request.EntryDelayMs,
            EntryDurationMs = request.EntryDurationMs,
            ExitDelayMs = request.ExitDelayMs,
            ExitDurationMs = request.ExitDurationMs,
            EntryEffect = request.EntryEffect,
            ExitEffect = request.ExitEffect,
            FitBoundsOnEntry = request.FitBoundsOnEntry,
            CameraOverride = request.CameraOverride,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddSegmentZoneAsync(segmentZone, ct);
        await _repository.SaveChangesAsync(ct);

        segmentZone = await _repository.GetSegmentZoneAsync(segmentZone.SegmentZoneId, ct);

        return Option.Some<SegmentZoneDto, Error>(segmentZone!.ToDto());
    }

    public async Task<Option<SegmentZoneDto, Error>> UpdateSegmentZoneAsync(Guid segmentZoneId,
        UpdateSegmentZoneV2Request request, CancellationToken ct = default)
    {
        var segmentZone = await _repository.GetSegmentZoneAsync(segmentZoneId, ct);
        if (segmentZone is null)
        {
            return Option.None<SegmentZoneDto, Error>(Error.NotFound("SegmentZone.NotFound", "Segment zone not found"));
        }

        segmentZone.DisplayOrder = request.DisplayOrder;
        segmentZone.IsVisible = request.IsVisible;
        segmentZone.ZIndex = request.ZIndex;
        segmentZone.HighlightBoundary = request.HighlightBoundary;
        segmentZone.BoundaryColor = request.BoundaryColor;
        segmentZone.BoundaryWidth = request.BoundaryWidth;
        segmentZone.FillZone = request.FillZone;
        segmentZone.FillColor = request.FillColor;
        segmentZone.FillOpacity = request.FillOpacity;
        segmentZone.ShowLabel = request.ShowLabel;
        segmentZone.LabelOverride = request.LabelOverride;
        segmentZone.LabelStyle = request.LabelStyle;
        segmentZone.EntryDelayMs = request.EntryDelayMs;
        segmentZone.EntryDurationMs = request.EntryDurationMs;
        segmentZone.ExitDelayMs = request.ExitDelayMs;
        segmentZone.ExitDurationMs = request.ExitDurationMs;
        segmentZone.EntryEffect = request.EntryEffect;
        segmentZone.ExitEffect = request.ExitEffect;
        segmentZone.FitBoundsOnEntry = request.FitBoundsOnEntry;
        segmentZone.CameraOverride = request.CameraOverride;
        segmentZone.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateSegmentZone(segmentZone);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<SegmentZoneDto, Error>(segmentZone.ToDto());
    }

    public async Task<Option<bool, Error>> DeleteSegmentZoneAsync(Guid segmentZoneId, CancellationToken ct = default)
    {
        var zone = await _repository.GetSegmentZoneAsync(segmentZoneId, ct);
        if (zone is null)
        {
            return Option.None<bool, Error>(Error.NotFound("StoryMap.Zone.NotFound", "Segment zone not found"));
        }

        _repository.RemoveSegmentZone(zone);
        await _repository.SaveChangesAsync(ct);
        return Option.Some<bool, Error>(true);
    }


    public async Task<Option<IReadOnlyCollection<SegmentLayerDto>, Error>> GetSegmentLayersAsync(Guid segmentId,
        CancellationToken ct = default)
    {
        var segment = await _repository.GetSegmentAsync(segmentId, ct);
        if (segment is null)
        {
            return Option.None<IReadOnlyCollection<SegmentLayerDto>, Error>(Error.NotFound("StoryMap.Segment.NotFound",
                "Segment not found"));
        }

        var segmentLayers = await _repository.GetSegmentLayersBySegmentAsync(segmentId, ct);
        var layerDtos = await BuildSegmentLayerDtosAsync(segment.MapId, segmentLayers, ct);
        return Option.Some<IReadOnlyCollection<SegmentLayerDto>, Error>(layerDtos);
    }

    public async Task<Option<SegmentLayerDto, Error>> CreateSegmentLayerAsync(CreateSegmentLayerRequest request,
        CancellationToken ct = default)
    {
        var segment = await _repository.GetSegmentAsync(request.SegmentId, ct);
        if (segment is null)
        {
            return Option.None<SegmentLayerDto, Error>(Error.NotFound("Segment.NotFound", "Segment not found"));
        }

        // Validate layer exists
        var layer = await _mapRepository.GetLayerById(request.LayerId);
        if (layer is null)
        {
            return Option.None<SegmentLayerDto, Error>(Error.NotFound("Layer.NotFound", "Layer not found"));
        }

        var segmentLayer = new SegmentLayer
        {
            SegmentLayerId = Guid.NewGuid(),
            SegmentId = request.SegmentId,
            LayerId = request.LayerId,
            DisplayOrder = request.DisplayOrder,
            IsVisible = request.IsVisible,
            Opacity = request.Opacity,
            ZIndex = request.ZIndex,
            EntryDelayMs = request.EntryDelayMs,
            EntryDurationMs = request.EntryDurationMs,
            ExitDelayMs = request.ExitDelayMs,
            ExitDurationMs = request.ExitDurationMs,
            EntryEffect = request.EntryEffect,
            ExitEffect = request.ExitEffect,
            StyleOverride = request.StyleOverride,
            CreatedAt = DateTime.UtcNow
        };
        segmentLayer.Layer = layer;

        await _repository.AddSegmentLayerAsync(segmentLayer, ct);
        await _repository.SaveChangesAsync(ct);

        var dto = await BuildSegmentLayerDtosAsync(segment.MapId, new List<SegmentLayer> { segmentLayer }, ct);
        return Option.Some<SegmentLayerDto, Error>(dto.First());
    }

    public async Task<Option<SegmentLayerDto, Error>> UpdateSegmentLayerAsync(Guid segmentLayerId,
        UpdateSegmentLayerRequest request, CancellationToken ct = default)
    {
        var segmentLayer = await _repository.GetSegmentLayerAsync(segmentLayerId, ct);
        if (segmentLayer is null)
        {
            return Option.None<SegmentLayerDto, Error>(Error.NotFound("SegmentLayer.NotFound",
                "Segment layer not found"));
        }

        segmentLayer.DisplayOrder = request.DisplayOrder;
        segmentLayer.IsVisible = request.IsVisible;
        segmentLayer.Opacity = request.Opacity;
        segmentLayer.ZIndex = request.ZIndex;
        segmentLayer.EntryDelayMs = request.EntryDelayMs;
        segmentLayer.EntryDurationMs = request.EntryDurationMs;
        segmentLayer.ExitDelayMs = request.ExitDelayMs;
        segmentLayer.ExitDurationMs = request.ExitDurationMs;
        segmentLayer.EntryEffect = request.EntryEffect;
        segmentLayer.ExitEffect = request.ExitEffect;
        segmentLayer.StyleOverride = request.StyleOverride;
        segmentLayer.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateSegmentLayer(segmentLayer);
        await _repository.SaveChangesAsync(ct);

        var mapId = segmentLayer.Segment?.MapId ?? segmentLayer.Layer?.MapId ?? Guid.Empty;
        var dto = await BuildSegmentLayerDtosAsync(mapId, new List<SegmentLayer> { segmentLayer }, ct);
        return Option.Some<SegmentLayerDto, Error>(dto.First());
    }

    public async Task<Option<bool, Error>> DeleteSegmentLayerAsync(Guid segmentLayerId, CancellationToken ct = default)
    {
        var segmentLayer = await _repository.GetSegmentLayerAsync(segmentLayerId, ct);
        if (segmentLayer is null)
        {
            return Option.None<bool, Error>(Error.NotFound("SegmentLayer.NotFound", "Segment layer not found"));
        }

        _repository.RemoveSegmentLayer(segmentLayer);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<bool, Error>(true);
    }

    private async Task<List<SegmentLayerDto>> BuildSegmentLayerDtosAsync(Guid mapId, List<SegmentLayer> segmentLayers, CancellationToken ct)
    {
        if (segmentLayers == null || segmentLayers.Count == 0)
        {
            return new List<SegmentLayerDto>();
        }

        var layerDataMap = await LoadLayerDataAsync(segmentLayers, ct);
        var layerDtoMap = BuildLayerDtoMap(segmentLayers, layerDataMap);
        var featuresByLayer = await LoadLayerFeaturesAsync(mapId, segmentLayers, ct);

        var result = new List<SegmentLayerDto>(segmentLayers.Count);
        foreach (var segmentLayer in segmentLayers)
        {
            var layerDto = layerDtoMap.GetValueOrDefault(segmentLayer.LayerId);
            var features = featuresByLayer.GetValueOrDefault(segmentLayer.LayerId) ?? Array.Empty<MapFeatureResponse>();
            result.Add(segmentLayer.ToDto(layerDto, features));
        }

        return result;
    }

    private async Task<Dictionary<Guid, string>> LoadLayerDataAsync(IEnumerable<SegmentLayer> segmentLayers, CancellationToken ct)
    {
        var result = new Dictionary<Guid, string>();
        var uniqueLayers = segmentLayers
            .Select(sl => sl.Layer)
            .OfType<Layer>()
            .GroupBy(layer => layer.LayerId)
            .Select(group => group.First());

        foreach (var layer in uniqueLayers)
        {
            var data = await _layerDataStore.GetDataAsync(layer, ct);
            if (!string.IsNullOrEmpty(data))
            {
                result[layer.LayerId] = data;
            }
        }

        return result;
    }

    private static Dictionary<Guid, LayerDTO> BuildLayerDtoMap(IEnumerable<SegmentLayer> segmentLayers, Dictionary<Guid, string> layerDataMap)
    {
        var result = new Dictionary<Guid, LayerDTO>();
        foreach (var layer in segmentLayers.Select(sl => sl.Layer).OfType<Layer>())
        {
            if (result.ContainsKey(layer.LayerId))
            {
                continue;
            }

            var layerData = layerDataMap.GetValueOrDefault(layer.LayerId);
            result[layer.LayerId] = layer.ToLayerDto(layerData);
        }

        return result;
    }

    private async Task<Dictionary<Guid, IReadOnlyCollection<MapFeatureResponse>>> LoadLayerFeaturesAsync(Guid mapId,
        IEnumerable<SegmentLayer> segmentLayers, CancellationToken ct)
    {
        var layerIds = segmentLayers
            .Select(sl => sl.LayerId)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var result = new Dictionary<Guid, IReadOnlyCollection<MapFeatureResponse>>();
        if (layerIds.Count == 0 || mapId == Guid.Empty)
        {
            return result;
        }

        var featureMetadata = await _mapFeatureRepository.GetByMap(mapId);
        var relevantMetadata = featureMetadata
            .Where(f => f.LayerId.HasValue && layerIds.Contains(f.LayerId.Value))
            .ToList();

        if (relevantMetadata.Count == 0)
        {
            return result;
        }

        var documents = await _mapFeatureStore.GetByMapAsync(mapId, ct);
        var documentMap = new Dictionary<Guid, MapFeatureDocument>();
        foreach (var document in documents)
        {
            if (Guid.TryParse(document.Id, out var featureId))
            {
                documentMap[featureId] = document;
            }
        }

        var temp = new Dictionary<Guid, List<MapFeatureResponse>>();
        foreach (var feature in relevantMetadata)
        {
            var layerId = feature.LayerId!.Value;
            documentMap.TryGetValue(feature.FeatureId, out var document);
            var response = feature.ToResponse(document);

            if (!temp.TryGetValue(layerId, out var list))
            {
                list = new List<MapFeatureResponse>();
                temp[layerId] = list;
            }

            list.Add(response);
        }

        foreach (var (key, value) in temp)
        {
            result[key] = value;
        }

        return result;
    }


    public async Task<Option<IReadOnlyCollection<ZoneDto>, Error>> GetZonesAsync(CancellationToken ct = default)
    {
        var zones = await _repository.GetZonesByParentAsync(null, ct);
        var zoneDtos = zones.Select(z => z.ToDto()).ToList();
        return Option.Some<IReadOnlyCollection<ZoneDto>, Error>(zoneDtos);
    }

    public async Task<Option<ZoneDto, Error>> GetZoneAsync(Guid zoneId, CancellationToken ct = default)
    {
        var zone = await _repository.GetZoneAsync(zoneId, ct);
        if (zone is null)
        {
            return Option.None<ZoneDto, Error>(Error.NotFound("Zone.NotFound", "Zone not found"));
        }

        return Option.Some<ZoneDto, Error>(zone.ToDto());
    }

    public async Task<Option<IReadOnlyCollection<ZoneDto>, Error>> GetZonesByParentAsync(Guid? parentZoneId,
        CancellationToken ct = default)
    {
        var zones = await _repository.GetZonesByParentAsync(parentZoneId, ct);
        var zoneDtos = zones.Select(z => z.ToDto()).ToList();
        return Option.Some<IReadOnlyCollection<ZoneDto>, Error>(zoneDtos);
    }

    public async Task<Option<IReadOnlyCollection<ZoneDto>, Error>> SearchZonesAsync(string searchTerm,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetZonesAsync(ct);
        }

        // First, search in database
        var zones = await _repository.SearchZonesAsync(searchTerm, ct);
        var zoneDtos = zones.Select(z => z.ToDto()).ToList();

        // If found results in DB, return them
        if (zoneDtos.Count > 0)
        {
        return Option.Some<IReadOnlyCollection<ZoneDto>, Error>(zoneDtos);
        }

        // If not found in DB, search OSM
        try
        {
            var osmResults = await _osmService.SearchByNameAsync(searchTerm, null, null, null, 10);
            var osmZoneDtos = new List<ZoneDto>();

            foreach (var osmResult in osmResults)
            {
                var externalId = $"osm:{osmResult.OsmType}:{osmResult.OsmId}";

                // Check if zone already exists in DB by externalId
                var existingZone = await _repository.GetZoneByExternalIdAsync(externalId, ct);
                if (existingZone is not null)
                {
                    // Zone already exists in DB, return it
                    osmZoneDtos.Add(existingZone.ToDto());
                    continue;
                }

                // Zone doesn't exist in DB, create it from OSM data
                var boundingBoxStr = osmResult.BoundingBox != null && osmResult.BoundingBox.Length == 4
                    ? $"{osmResult.BoundingBox[0]},{osmResult.BoundingBox[2]},{osmResult.BoundingBox[1]},{osmResult.BoundingBox[3]}"
                    : null;

                var createRequest = new CreateZoneFromOsmRequest(
                    OsmId: osmResult.OsmId,
                    OsmType: osmResult.OsmType,
                    DisplayName: osmResult.DisplayName,
                    Lat: osmResult.Lat,
                    Lon: osmResult.Lon,
                    GeoJson: osmResult.GeoJson,
                    Category: osmResult.Category,
                    Type: osmResult.Type,
                    AdminLevel: osmResult.AdminLevel,
                    ParentZoneId: null,
                    BoundingBox: boundingBoxStr
                );

                var createResult = await CreateZoneFromOsmAsync(createRequest, ct);
                if (createResult.Match(some: _ => true, none: _ => false))
                {
                    createResult.Match(
                        some: zoneDto => osmZoneDtos.Add(zoneDto),
                        none: _ => { } // Skip if creation failed
                    );
                }
            }

            return Option.Some<IReadOnlyCollection<ZoneDto>, Error>(osmZoneDtos);
        }
        catch (Exception ex)
        {
            // If OSM search fails, return empty list
            return Option.Some<IReadOnlyCollection<ZoneDto>, Error>(new List<ZoneDto>());
        }
    }

    public async Task<Option<ZoneDto, Error>> CreateZoneAsync(CreateZoneRequest request, CancellationToken ct = default)
    {
        if (request.ParentZoneId.HasValue)
        {
            var parentZone = await _repository.GetZoneAsync(request.ParentZoneId.Value, ct);
            if (parentZone is null)
            {
                return Option.None<ZoneDto, Error>(Error.NotFound("Zone.ParentNotFound", "Parent zone not found"));
            }
        }

        if (!string.IsNullOrEmpty(request.ExternalId))
        {
            var existing = await _repository.GetZoneByExternalIdAsync(request.ExternalId, ct);
            if (existing is not null)
            {
                return Option.None<ZoneDto, Error>(Error.Conflict("Zone.ExternalIdExists",
                    "Zone with this external ID already exists"));
            }
        }

        var zone = new Zone
        {
            ZoneId = Guid.NewGuid(),
            ExternalId = request.ExternalId,
            ZoneCode = request.ZoneCode,
            Name = request.Name,
            ZoneType = request.ZoneType,
            AdminLevel = request.AdminLevel,
            ParentZoneId = request.ParentZoneId,
            Geometry = request.Geometry,
            SimplifiedGeometry = request.SimplifiedGeometry,
            Centroid = request.Centroid,
            BoundingBox = request.BoundingBox,
            Description = request.Description,
            IsActive = true,
            LastSyncedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddZoneAsync(zone, ct);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<ZoneDto, Error>(zone.ToDto());
    }

    public async Task<Option<ZoneDto, Error>> CreateZoneFromOsmAsync(CreateZoneFromOsmRequest request,
        CancellationToken ct = default)
    {
        // Validate request
        if (request.OsmId <= 0)
        {
            return Option.None<ZoneDto, Error>(Error.ValidationError("Zone.InvalidOsmId",
                "OSM ID must be greater than 0"));
        }
        
        if (string.IsNullOrWhiteSpace(request.OsmType))
        {
            return Option.None<ZoneDto, Error>(Error.ValidationError("Zone.InvalidOsmType", "OSM Type is required"));
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return Option.None<ZoneDto, Error>(Error.ValidationError("Zone.InvalidDisplayName",
                "Display name is required"));
        }
        
        if (request.ParentZoneId.HasValue)
        {
            var parentZone = await _repository.GetZoneAsync(request.ParentZoneId.Value, ct);
            if (parentZone is null)
            {
                return Option.None<ZoneDto, Error>(Error.NotFound("Zone.ParentNotFound", "Parent zone not found"));
            }
        }

        var externalId = $"osm:{request.OsmType}:{request.OsmId}";

        var existingZone = await _repository.GetZoneByExternalIdAsync(externalId, ct);
        if (existingZone is not null)
        {
            return Option.Some<ZoneDto, Error>(existingZone.ToDto());
        }

        string centroid = JsonSerializer.Serialize(new
        {
            type = "Point",
            coordinates = new[] { request.Lon, request.Lat }
        });
        
        // Use boundingBox from request if provided, otherwise calculate it
        string boundingBox = request.BoundingBox;
        if (string.IsNullOrWhiteSpace(boundingBox))
        {
        var offset = 0.01;
            boundingBox =
                $"{request.Lat - offset},{request.Lon - offset},{request.Lat + offset},{request.Lon + offset}";
        
        try
        {
            if (!string.IsNullOrWhiteSpace(request.GeoJson))
            {
                var geoJson = JsonDocument.Parse(request.GeoJson);
                if (geoJson.RootElement.TryGetProperty("type", out var typeProperty))
                {
                    var geometryType = typeProperty.GetString();

                        if (geometryType == "Point" &&
                            geoJson.RootElement.TryGetProperty("coordinates", out var coords))
                    {
                        var lon = coords[0].GetDouble();
                        var lat = coords[1].GetDouble();
                        
                        centroid = JsonSerializer.Serialize(new
                        {
                            type = "Point",
                            coordinates = new[] { lon, lat }
                        });

                        var pointOffset = 0.001;
                            boundingBox =
                                $"{lat - pointOffset},{lon - pointOffset},{lat + pointOffset},{lon + pointOffset}";
                    }
                    else if ((geometryType == "Polygon" || geometryType == "MultiPolygon"))
                    {
                        centroid = JsonSerializer.Serialize(new
                        {
                            type = "Point",
                            coordinates = new[] { request.Lon, request.Lat }
                        });
                        
                        var polyOffset = 0.1;
                            boundingBox =
                                $"{request.Lat - polyOffset},{request.Lon - polyOffset},{request.Lat + polyOffset},{request.Lon + polyOffset}";
                    }
                }
            }
        }
        catch (Exception)
        {
            }
        }

        var zoneType = DetermineZoneType(request.Category, request.Type, request.AdminLevel);
        var adminLevel = DetermineAdminLevel(request.AdminLevel, request.Category, request.Type);

        var zoneCode = $"OSM_{request.OsmType.ToUpperInvariant()}_{request.OsmId}";

        // Ensure Geometry is not null or empty - create Point geometry if GeoJson is missing
        string geometry = request.GeoJson;
        if (string.IsNullOrWhiteSpace(geometry))
        {
            // Create a Point geometry from lat/lon if GeoJson is not available
            geometry = JsonSerializer.Serialize(new
            {
                type = "Point",
                coordinates = new[] { request.Lon, request.Lat }
            });
        }

        var zone = new Zone
        {
            ZoneId = Guid.NewGuid(),
            ExternalId = externalId,
            ZoneCode = zoneCode,
            Name = request.DisplayName,
            ZoneType = zoneType,
            AdminLevel = adminLevel,
            ParentZoneId = request.ParentZoneId,
            Geometry = geometry,
            SimplifiedGeometry = null,
            Centroid = centroid,
            BoundingBox = boundingBox,
            Description = $"Imported from OpenStreetMap: {request.Category}/{request.Type}",
            IsActive = true,
            LastSyncedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddZoneAsync(zone, ct);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<ZoneDto, Error>(zone.ToDto());
    }

    private ZoneType DetermineZoneType(
        string? category, 
        string? type, 
        int? adminLevel)
    {
        if (category == "boundary" && type == "administrative")
        {
            return ZoneType.Area;
        }

        if (category == "highway" || type == "route")
        {
            return ZoneType.Route;
        }

        return ZoneType.Custom;
    }

    private ZoneAdminLevel DetermineAdminLevel(
        int? adminLevel,
        string? category,
        string? type)
    {
        if (adminLevel.HasValue)
        {
            return adminLevel.Value switch
            {
                2 => ZoneAdminLevel.Country,
                4 => ZoneAdminLevel.Province,
                6 => ZoneAdminLevel.District,
                8 or 9 or 10 => ZoneAdminLevel.Commune,
                _ => ZoneAdminLevel.Custom
            };
        }

        return ZoneAdminLevel.Custom;
    }

    public async Task<Option<ZoneDto, Error>> UpdateZoneAsync(Guid zoneId, UpdateZoneRequest request,
        CancellationToken ct = default)
    {
        var zone = await _repository.GetZoneAsync(zoneId, ct);
        if (zone is null)
        {
            return Option.None<ZoneDto, Error>(Error.NotFound("Zone.NotFound", "Zone not found"));
        }

        zone.Name = request.Name;
        zone.Description = request.Description;
        zone.SimplifiedGeometry = request.SimplifiedGeometry;
        zone.Centroid = request.Centroid;
        zone.BoundingBox = request.BoundingBox;
        zone.IsActive = request.IsActive;
        zone.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateZone(zone);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<ZoneDto, Error>(zone.ToDto());
    }

    public async Task<Option<bool, Error>> DeleteZoneAsync(Guid zoneId, CancellationToken ct = default)
    {
        var zone = await _repository.GetZoneAsync(zoneId, ct);
        if (zone is null)
        {
            return Option.None<bool, Error>(Error.NotFound("Zone.NotFound", "Zone not found"));
        }

        zone.IsActive = false;
        zone.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateZone(zone);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<int, Error>> SyncZonesFromOSMAsync(SyncZonesFromOSMRequest request,
        CancellationToken ct = default)
    {
        try
        {
            if (!int.TryParse(request.AdminLevel, out var adminLevel))
            {
                return Option.None<int, Error>(
                    Error.ValidationError("Zone.InvalidAdminLevel", "Admin level must be a valid integer"));
            }

            var overpassQuery = new StringBuilder();
            overpassQuery.AppendLine("[out:json][timeout:300];");
            overpassQuery.AppendLine("(");
            
            if (!string.IsNullOrWhiteSpace(request.CountryCode))
            {
                var countryCode = request.CountryCode.ToUpperInvariant();
                overpassQuery.AppendLine($"  relation[\"boundary\"=\"administrative\"][\"admin_level\"=\"{adminLevel}\"][\"ISO3166-1\"=\"{countryCode}\"];");
                overpassQuery.AppendLine($"  relation[\"boundary\"=\"administrative\"][\"admin_level\"=\"{adminLevel}\"][\"ISO3166-1:alpha2\"=\"{countryCode}\"];");
            }
            else
            {
                overpassQuery.AppendLine($"  relation[\"boundary\"=\"administrative\"][\"admin_level\"=\"{adminLevel}\"];");
            }
            
            overpassQuery.AppendLine(");");
            overpassQuery.AppendLine("out body;");
            overpassQuery.AppendLine(">;");
            overpassQuery.AppendLine("out skel qt;");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "CustomMapOSM_Application/1.0");
            httpClient.Timeout = TimeSpan.FromMinutes(5);

            var content = new StringContent(overpassQuery.ToString(), Encoding.UTF8, "text/plain");
            var response = await httpClient.PostAsync("https://overpass-api.de/api/interpreter", content, ct);
            
            if (!response.IsSuccessStatusCode)
            {
                return Option.None<int, Error>(
                    Error.Failure("Zone.OverpassApiError", 
                        $"Overpass API returned status code: {response.StatusCode}"));
            }

            var responseContent = await response.Content.ReadAsStringAsync(ct);
            var overpassResult = JsonSerializer.Deserialize<JsonDocument>(responseContent);

            if (overpassResult == null || !overpassResult.RootElement.TryGetProperty("elements", out var elements))
            {
                return Option.Some<int, Error>(0);
            }

            var syncedCount = 0;
            var errors = new List<string>();

            foreach (var element in elements.EnumerateArray())
            {
                try
                {
                    if (!element.TryGetProperty("type", out var typeElement) || 
                        !element.TryGetProperty("id", out var idElement))
                    {
                        continue;
                    }

                    var osmType = typeElement.GetString();
                    var osmId = idElement.GetInt64();

                    if (string.IsNullOrWhiteSpace(osmType) || osmId <= 0)
                    {
                        continue;
                    }

                    var externalId = $"osm:{osmType}:{osmId}";
                    var existingZone = await _repository.GetZoneByExternalIdAsync(externalId, ct);

                    if (existingZone != null)
                    {
                        if (request.UpdateExisting)
                        {
                            existingZone.LastSyncedAt = DateTime.UtcNow;
                            _repository.UpdateZone(existingZone);
                            syncedCount++;
                        }
                        continue;
                    }

                    if (!element.TryGetProperty("tags", out var tagsElement))
                    {
                        continue;
                    }

                    var tags = tagsElement;
                    var name = tags.TryGetProperty("name", out var nameElement) 
                        ? nameElement.GetString() 
                        : tags.TryGetProperty("name:en", out var nameEnElement)
                            ? nameEnElement.GetString()
                            : null;

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    double? lat = null;
                    double? lon = null;
                    if (element.TryGetProperty("lat", out var latElement))
                    {
                        lat = latElement.GetDouble();
                    }
                    if (element.TryGetProperty("lon", out var lonElement))
                    {
                        lon = lonElement.GetDouble();
                    }

                    if (!lat.HasValue || !lon.HasValue)
                    {
                        if (element.TryGetProperty("bounds", out var boundsElement))
                        {
                            if (boundsElement.TryGetProperty("minlat", out var minLat) &&
                                boundsElement.TryGetProperty("maxlat", out var maxLat) &&
                                boundsElement.TryGetProperty("minlon", out var minLon) &&
                                boundsElement.TryGetProperty("maxlon", out var maxLon))
                            {
                                lat = (minLat.GetDouble() + maxLat.GetDouble()) / 2;
                                lon = (minLon.GetDouble() + maxLon.GetDouble()) / 2;
                            }
                        }
                    }

                    if (!lat.HasValue || !lon.HasValue)
                    {
                        continue;
                    }

                    var category = tags.TryGetProperty("boundary", out var boundaryElement)
                        ? boundaryElement.GetString()
                        : null;
                    var type = tags.TryGetProperty("admin_level", out var adminLevelElement)
                        ? adminLevelElement.GetString()
                        : null;

                    var createRequest = new CreateZoneFromOsmRequest(
                        OsmType: osmType,
                        OsmId: osmId,
                        DisplayName: name,
                        Lat: lat.Value,
                        Lon: lon.Value,
                        GeoJson: null,
                        Category: category ?? "boundary",
                        Type: type ?? "administrative",
                        AdminLevel: adminLevel,
                        ParentZoneId: null,
                        BoundingBox: null
                    );

                    var createResult = await CreateZoneFromOsmAsync(createRequest, ct);
                    if (createResult.Match(some: _ => true, none: _ => false))
                    {
                        syncedCount++;
                    }
                    else
                    {
                        createResult.Match(
                            some: _ => { },
                            none: err => errors.Add($"Failed to create zone {name}")
                        );
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error processing OSM element: {ex.Message}");
                }
            }

            await _repository.SaveChangesAsync(ct);

            if (errors.Count > 0 && syncedCount == 0)
            {
                return Option.None<int, Error>(
                    Error.Failure("Zone.SyncPartialFailure", 
                        $"Failed to sync zones. Errors: {string.Join("; ", errors.Take(5))}"));
            }

            return Option.Some<int, Error>(syncedCount);
        }
        catch (TaskCanceledException)
        {
            return Option.None<int, Error>(
                Error.Failure("Zone.SyncTimeout", "Sync operation timed out"));
        }
        catch (Exception ex)
        {
            return Option.None<int, Error>(
                Error.Failure("Zone.SyncError", $"Failed to sync zones from OSM: {ex.Message}"));
        }
    }
    
    public async Task<Option<IReadOnlyCollection<TimelineTransitionDto>, Error>> GetTimelineTransitionsAsync(
        Guid mapId, CancellationToken ct = default)
    {
        var map = await _repository.GetMapAsync(mapId, ct);
        if (map is null)
        {
            return Option.None<IReadOnlyCollection<TimelineTransitionDto>, Error>(
                Error.NotFound("Map.NotFound", "Map not found"));
        }

        var transitions = await _repository.GetTimelineTransitionsByMapAsync(mapId, ct);
        var transitionDtos = transitions.Select(t => t.ToDto()).ToList();

        return Option.Some<IReadOnlyCollection<TimelineTransitionDto>, Error>(transitionDtos);
    }

    public async Task<Option<TimelineTransitionDto, Error>> GetTimelineTransitionAsync(Guid transitionId,
        CancellationToken ct = default)
    {
        var transition = await _repository.GetTimelineTransitionAsync(transitionId, ct);
        if (transition is null)
        {
            return Option.None<TimelineTransitionDto, Error>(
                Error.NotFound("Transition.NotFound", "Timeline transition not found"));
        }

        return Option.Some<TimelineTransitionDto, Error>(transition.ToDto());
    }

    public async Task<Option<TimelineTransitionDto, Error>> CreateTimelineTransitionAsync(
        CreateTimelineTransitionRequest request, CancellationToken ct = default)
    {
        var map = await _repository.GetMapAsync(request.MapId, ct);
        if (map is null)
        {
            return Option.None<TimelineTransitionDto, Error>(Error.NotFound("Map.NotFound", "Map not found"));
        }

        var fromSegment = await _repository.GetSegmentAsync(request.FromSegmentId, ct);
        if (fromSegment is null)
        {
            return Option.None<TimelineTransitionDto, Error>(
                Error.NotFound("Segment.FromNotFound", "From segment not found"));
        }

        var toSegment = await _repository.GetSegmentAsync(request.ToSegmentId, ct);
        if (toSegment is null)
        {
            return Option.None<TimelineTransitionDto, Error>(
                Error.NotFound("Segment.ToNotFound", "To segment not found"));
        }

        var existingTransition =
            await _repository.GetTransitionBetweenSegmentsAsync(request.FromSegmentId, request.ToSegmentId, ct);
        if (existingTransition is not null)
        {
            return Option.None<TimelineTransitionDto, Error>(Error.Conflict("Transition.AlreadyExists",
                "Transition between these segments already exists"));
        }

        var transition = new TimelineTransition
        {
            TimelineTransitionId = Guid.NewGuid(),
            MapId = request.MapId,
            FromSegmentId = request.FromSegmentId,
            ToSegmentId = request.ToSegmentId,
            TransitionName = request.TransitionName,
            DurationMs = request.DurationMs,
            TransitionType = request.TransitionType,
            AnimateCamera = request.AnimateCamera,
            CameraAnimationType = request.CameraAnimationType,
            CameraAnimationDurationMs = request.CameraAnimationDurationMs,
            ShowOverlay = request.ShowOverlay,
            OverlayContent = request.OverlayContent,
            AutoTrigger = request.AutoTrigger,
            RequireUserAction = request.RequireUserAction,
            TriggerButtonText = request.TriggerButtonText ?? "Next",
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddTimelineTransitionAsync(transition, ct);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<TimelineTransitionDto, Error>(transition.ToDto());
    }

    public async Task<Option<TimelineTransitionDto, Error>> UpdateTimelineTransitionAsync(Guid transitionId,
        UpdateTimelineTransitionRequest request, CancellationToken ct = default)
    {
        var transition = await _repository.GetTimelineTransitionAsync(transitionId, ct);
        if (transition is null)
        {
            return Option.None<TimelineTransitionDto, Error>(
                Error.NotFound("Transition.NotFound", "Timeline transition not found"));
        }

        transition.TransitionName = request.TransitionName;
        transition.DurationMs = request.DurationMs;
        transition.TransitionType = request.TransitionType;
        transition.AnimateCamera = request.AnimateCamera;
        transition.CameraAnimationType = request.CameraAnimationType;
        transition.CameraAnimationDurationMs = request.CameraAnimationDurationMs;
        transition.ShowOverlay = request.ShowOverlay;
        transition.OverlayContent = request.OverlayContent;
        transition.AutoTrigger = request.AutoTrigger;
        transition.RequireUserAction = request.RequireUserAction;
        transition.TriggerButtonText = request.TriggerButtonText;
        transition.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateTimelineTransition(transition);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<TimelineTransitionDto, Error>(transition.ToDto());
    }

    public async Task<Option<bool, Error>> DeleteTimelineTransitionAsync(Guid transitionId,
        CancellationToken ct = default)
    {
        var transition = await _repository.GetTimelineTransitionAsync(transitionId, ct);
        if (transition is null)
        {
            return Option.None<bool, Error>(Error.NotFound("Transition.NotFound", "Timeline transition not found"));
        }

        _repository.RemoveTimelineTransition(transition);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<TimelineTransitionDto, Error>> GenerateTimelineTransitionAsync(Guid mapId,
        GenerateTimelineTransitionRequest request, CancellationToken ct = default)
    {
        var map = await _repository.GetMapAsync(mapId, ct);
        if (map is null)
        {
            return Option.None<TimelineTransitionDto, Error>(Error.NotFound("Map.NotFound", "Map not found"));
        }

        var fromSegment = await _repository.GetSegmentAsync(request.FromSegmentId, ct);
        if (fromSegment is null)
        {
            return Option.None<TimelineTransitionDto, Error>(
                Error.NotFound("Segment.FromNotFound", "From segment not found"));
        }

        var toSegment = await _repository.GetSegmentAsync(request.ToSegmentId, ct);
        if (toSegment is null)
        {
            return Option.None<TimelineTransitionDto, Error>(
                Error.NotFound("Segment.ToNotFound", "To segment not found"));
        }

        // Check if transition already exists
        var existingTransition =
            await _repository.GetTransitionBetweenSegmentsAsync(request.FromSegmentId, request.ToSegmentId, ct);
        if (existingTransition is not null)
        {
            return Option.None<TimelineTransitionDto, Error>(
                Error.Conflict("Transition.AlreadyExists", "Transition between these segments already exists"));
        }

        // Smart transition generation based on camera states
        var (animationType, animationDuration) =
            CalculateOptimalTransition(fromSegment.CameraState, toSegment.CameraState);

        // Generate smart transition with sensible defaults
        var transition = new TimelineTransition
        {
            TimelineTransitionId = Guid.NewGuid(),
            MapId = mapId,
            FromSegmentId = request.FromSegmentId,
            ToSegmentId = request.ToSegmentId,
            TransitionName = $"Transition: {fromSegment.Name} → {toSegment.Name}",
            DurationMs = 1000,
            TransitionType = TransitionType.Ease,
            AnimateCamera = true,
            CameraAnimationType = animationType,
            CameraAnimationDurationMs = animationDuration,
            ShowOverlay = false,
            AutoTrigger = true,
            RequireUserAction = false,
            TriggerButtonText = "Next",
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddTimelineTransitionAsync(transition, ct);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<TimelineTransitionDto, Error>(transition.ToDto());
    }
    
    public async Task<Option<IReadOnlyCollection<AnimatedLayerDto>, Error>> GetAnimatedLayersAsync(
        Guid mapId, CancellationToken ct = default)
    {
        var map = await _repository.GetMapAsync(mapId, ct);
        if (map is null)
        {
            return Option.None<IReadOnlyCollection<AnimatedLayerDto>, Error>(
                Error.NotFound("Map.NotFound", "Map not found"));
        }

        var animatedLayers = await _repository.GetAnimatedLayersByMapAsync(mapId, ct);
        var dtos = animatedLayers.Select(al => al.ToDto()).ToList();
        
        return Option.Some<IReadOnlyCollection<AnimatedLayerDto>, Error>(dtos);
    }

    public async Task<Option<IReadOnlyCollection<AnimatedLayerDto>, Error>> GetAnimatedLayersBySegmentAsync(
        Guid segmentId, CancellationToken ct = default)
    {
        var segment = await _repository.GetSegmentAsync(segmentId, ct);
        if (segment is null)
        {
            return Option.None<IReadOnlyCollection<AnimatedLayerDto>, Error>(
                Error.NotFound("Segment.NotFound", "Segment not found"));
        }

        var animatedLayers = await _repository.GetAnimatedLayersBySegmentAsync(segmentId, ct);
        var dtos = animatedLayers.Select(al => al.ToDto()).ToList();
        
        return Option.Some<IReadOnlyCollection<AnimatedLayerDto>, Error>(dtos);
    }

    public async Task<Option<IReadOnlyCollection<AnimatedLayerDto>, Error>> GetAnimatedLayersByLayerAsync(Guid layerId,
        CancellationToken ct = default)
    {
        var animatedLayers = await _repository.GetAnimatedLayersByLayerAsync(layerId, ct);
        var dtos = animatedLayers.Select(al => al.ToDto()).ToList();
        
        return Option.Some<IReadOnlyCollection<AnimatedLayerDto>, Error>(dtos);
    }

    public async Task<Option<AnimatedLayerDto, Error>> GetAnimatedLayerAsync(Guid animatedLayerId,
        CancellationToken ct = default)
    {
        var animatedLayer = await _repository.GetAnimatedLayerAsync(animatedLayerId, ct);
        if (animatedLayer is null)
        {
            return Option.None<AnimatedLayerDto, Error>(
                Error.NotFound("AnimatedLayer.NotFound", "Animated layer not found"));
        }

        return Option.Some<AnimatedLayerDto, Error>(animatedLayer.ToDto());
    }

    public async Task<Option<AnimatedLayerDto, Error>> CreateAnimatedLayerAsync(CreateAnimatedLayerRequest request,
        Guid userId, CancellationToken ct = default)
    {
        if (!request.LayerId.HasValue && !request.SegmentId.HasValue)
        {
            return Option.None<AnimatedLayerDto, Error>(
                Error.ValidationError("AnimatedLayer.NoParent",
                    "AnimatedLayer must belong to either a Layer or a Segment"));
        }

        if (request.LayerId.HasValue)
        {
            var layer = await _mapRepository.GetLayerById(request.LayerId.Value);
            if (layer is null)
            {
                return Option.None<AnimatedLayerDto, Error>(
                    Error.NotFound("Layer.NotFound", "Layer not found"));
            }
        }

        if (request.SegmentId.HasValue)
        {
            var segment = await _repository.GetSegmentAsync(request.SegmentId.Value, ct);
            if (segment is null)
            {
                return Option.None<AnimatedLayerDto, Error>(
                    Error.NotFound("Segment.NotFound", "Segment not found"));
            }
        }

        var animatedLayer = new AnimatedLayer
        {
            AnimatedLayerId = Guid.NewGuid(),
            CreatedBy = userId,
            LayerId = request.LayerId,
            SegmentId = request.SegmentId,
            Name = request.Name,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            MediaType = request.MediaType,
            SourceUrl = request.SourceUrl,
            ThumbnailUrl = request.ThumbnailUrl,
            Coordinates = request.Coordinates,
            IsScreenOverlay = request.IsScreenOverlay,
            ScreenPosition = request.ScreenPosition,
            RotationDeg = request.RotationDeg,
            Scale = request.Scale,
            Opacity = request.Opacity,
            ZIndex = request.ZIndex,
            IsVisible = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAnimatedLayerAsync(animatedLayer, ct);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<AnimatedLayerDto, Error>(animatedLayer.ToDto());
    }

    public async Task<Option<AnimatedLayerDto, Error>> UpdateAnimatedLayerAsync(Guid animatedLayerId,
        UpdateAnimatedLayerRequest request, CancellationToken ct = default)
    {
        var animatedLayer = await _repository.GetAnimatedLayerAsync(animatedLayerId, ct);
        if (animatedLayer is null)
        {
            return Option.None<AnimatedLayerDto, Error>(
                Error.NotFound("AnimatedLayer.NotFound", "Animated layer not found"));
        }

        animatedLayer.Name = request.Name;
        animatedLayer.Description = request.Description;
        animatedLayer.DisplayOrder = request.DisplayOrder;
        animatedLayer.Coordinates = request.Coordinates;
        animatedLayer.IsScreenOverlay = request.IsScreenOverlay;
        animatedLayer.ScreenPosition = request.ScreenPosition;
        animatedLayer.RotationDeg = request.RotationDeg;
        animatedLayer.Scale = request.Scale;
        animatedLayer.Opacity = request.Opacity;
        animatedLayer.ZIndex = request.ZIndex;
        animatedLayer.IsVisible = request.IsVisible;
        animatedLayer.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateAnimatedLayer(animatedLayer);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<AnimatedLayerDto, Error>(animatedLayer.ToDto());
    }

    public async Task<Option<bool, Error>> DeleteAnimatedLayerAsync(Guid animatedLayerId,
        CancellationToken ct = default)
    {
        var animatedLayer = await _repository.GetAnimatedLayerAsync(animatedLayerId, ct);
        if (animatedLayer is null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("AnimatedLayer.NotFound", "Animated layer not found"));
        }

        _repository.RemoveAnimatedLayer(animatedLayer);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<IReadOnlyCollection<AnimatedLayerPresetDto>, Error>> GetAnimatedLayerPresetsAsync(
        CancellationToken ct = default)
    {
        var presets = await _repository.GetAnimatedLayerPresetsAsync(ct);
        var dtos = presets.Select(p => p.ToDto()).ToList();
        
        return Option.Some<IReadOnlyCollection<AnimatedLayerPresetDto>, Error>(dtos);
    }

    public async Task<Option<IReadOnlyCollection<AnimatedLayerPresetDto>, Error>>
        GetAnimatedLayerPresetsByCategoryAsync(string category, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return await GetAnimatedLayerPresetsAsync(ct);
        }

        var presets = await _repository.GetAnimatedLayerPresetsByCategoryAsync(category, ct);
        var dtos = presets.Select(p => p.ToDto()).ToList();
        
        return Option.Some<IReadOnlyCollection<AnimatedLayerPresetDto>, Error>(dtos);
    }

    public async Task<Option<IReadOnlyCollection<AnimatedLayerPresetDto>, Error>> SearchAnimatedLayerPresetsAsync(
        string searchTerm, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAnimatedLayerPresetsAsync(ct);
        }

        var presets = await _repository.SearchAnimatedLayerPresetsAsync(searchTerm, ct);
        var dtos = presets.Select(p => p.ToDto()).ToList();
        
        return Option.Some<IReadOnlyCollection<AnimatedLayerPresetDto>, Error>(dtos);
    }

    public async Task<Option<AnimatedLayerPresetDto, Error>> GetAnimatedLayerPresetAsync(Guid presetId,
        CancellationToken ct = default)
    {
        var preset = await _repository.GetAnimatedLayerPresetAsync(presetId, ct);
        if (preset is null)
        {
            return Option.None<AnimatedLayerPresetDto, Error>(
                Error.NotFound("Preset.NotFound", "Animated layer preset not found"));
        }

        return Option.Some<AnimatedLayerPresetDto, Error>(preset.ToDto());
    }

    public async Task<Option<AnimatedLayerPresetDto, Error>> CreateAnimatedLayerPresetAsync(
        CreateAnimatedLayerPresetRequest request, Guid? userId,
        CancellationToken ct = default)
    {
        var preset = new AnimatedLayerPreset
        {
            AnimatedLayerPresetId = Guid.NewGuid(),
            CreatedBy = userId,
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Tags = request.Tags,
            MediaType = request.MediaType,
            SourceUrl = request.SourceUrl,
            ThumbnailUrl = request.ThumbnailUrl,
            DefaultCoordinates = request.DefaultCoordinates,
            DefaultIsScreenOverlay = request.DefaultIsScreenOverlay,
            DefaultScreenPosition = request.DefaultScreenPosition,
            DefaultScale = request.DefaultScale,
            DefaultOpacity = request.DefaultOpacity,
            DefaultAutoPlay = request.DefaultAutoPlay,
            DefaultLoop = request.DefaultLoop,
            IsSystemPreset = request.IsSystemPreset,
            IsPublic = request.IsPublic,
            UsageCount = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAnimatedLayerPresetAsync(preset, ct);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<AnimatedLayerPresetDto, Error>(preset.ToDto());
    }

    public async Task<Option<AnimatedLayerPresetDto, Error>> UpdateAnimatedLayerPresetAsync(Guid presetId,
        UpdateAnimatedLayerPresetRequest request,
        CancellationToken ct = default)
    {
        var preset = await _repository.GetAnimatedLayerPresetAsync(presetId, ct);
        if (preset is null)
        {
            return Option.None<AnimatedLayerPresetDto, Error>(
                Error.NotFound("Preset.NotFound", "Animated layer preset not found"));
        }

        preset.Name = request.Name;
        preset.Description = request.Description;
        preset.Category = request.Category;
        preset.Tags = request.Tags;
        preset.DefaultCoordinates = request.DefaultCoordinates;
        preset.DefaultIsScreenOverlay = request.DefaultIsScreenOverlay;
        preset.DefaultScreenPosition = request.DefaultScreenPosition;
        preset.DefaultScale = request.DefaultScale;
        preset.DefaultOpacity = request.DefaultOpacity;
        preset.DefaultAutoPlay = request.DefaultAutoPlay;
        preset.DefaultLoop = request.DefaultLoop;
        preset.IsPublic = request.IsPublic;
        preset.IsActive = request.IsActive;
        preset.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateAnimatedLayerPreset(preset);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<AnimatedLayerPresetDto, Error>(preset.ToDto());
    }

    public async Task<Option<bool, Error>> DeleteAnimatedLayerPresetAsync(Guid presetId, CancellationToken ct = default)
    {
        var preset = await _repository.GetAnimatedLayerPresetAsync(presetId, ct);
        if (preset is null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Preset.NotFound", "Animated layer preset not found"));
        }

        // Soft delete
        preset.IsActive = false;
        preset.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateAnimatedLayerPreset(preset);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<AnimatedLayerDto, Error>> CreateAnimatedLayerFromPresetAsync(Guid presetId, Guid? layerId,
        Guid? segmentId, CancellationToken ct = default)
    {
        var preset = await _repository.GetAnimatedLayerPresetAsync(presetId, ct);
        if (preset is null)
        {
            return Option.None<AnimatedLayerDto, Error>(
                Error.NotFound("Preset.NotFound", "Animated layer preset not found"));
        }

        // Validate that at least one of LayerId or SegmentId is provided
        if (!layerId.HasValue && !segmentId.HasValue)
        {
            return Option.None<AnimatedLayerDto, Error>(
                Error.ValidationError("AnimatedLayer.NoParent",
                    "AnimatedLayer must belong to either a Layer or a Segment"));
        }

        // Validate segment if provided
        if (segmentId.HasValue)
        {
            var segment = await _repository.GetSegmentAsync(segmentId.Value, ct);
            if (segment is null)
            {
                return Option.None<AnimatedLayerDto, Error>(
                    Error.NotFound("Segment.NotFound", "Segment not found"));
            }
        }

        var userId = _currentUserService.GetUserId();
        if (!userId.HasValue)
        {
            return Option.None<AnimatedLayerDto, Error>(
                Error.Unauthorized("User.NotAuthenticated", "User is not authenticated"));
        }

        // Create AnimatedLayer from preset settings
        var animatedLayer = new AnimatedLayer
        {
            AnimatedLayerId = Guid.NewGuid(),
            CreatedBy = userId.Value,
            LayerId = layerId,
            SegmentId = segmentId,
            Name = preset.Name,
            Description = preset.Description,
            DisplayOrder = 0,
            MediaType = preset.MediaType,
            SourceUrl = preset.SourceUrl,
            ThumbnailUrl = preset.ThumbnailUrl,
            Coordinates = preset.DefaultCoordinates,
            IsScreenOverlay = preset.DefaultIsScreenOverlay,
            ScreenPosition = preset.DefaultScreenPosition,
            RotationDeg = 0.0,
            Scale = preset.DefaultScale,
            Opacity = preset.DefaultOpacity,
            ZIndex = 1000,
            IsVisible = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAnimatedLayerAsync(animatedLayer, ct);

        // Increment preset usage count
        preset.UsageCount++;
        _repository.UpdateAnimatedLayerPreset(preset);

        await _repository.SaveChangesAsync(ct);

        return Option.Some<AnimatedLayerDto, Error>(animatedLayer.ToDto());
    }

    // Helper method for smart transition calculation
    private (CameraAnimationType animationType, int durationMs) CalculateOptimalTransition(
        string? fromCameraState, 
        string? toCameraState)
    {
        // Default values
        var animationType = CameraAnimationType.Fly;
        var durationMs = 1500;

        // If camera states are empty or invalid, use defaults
        if (string.IsNullOrEmpty(fromCameraState) || string.IsNullOrEmpty(toCameraState))
        {
            return (animationType, durationMs);
        }

        try
        {
            // Try to parse camera states as JSON (assuming format: {"lat": x, "lng": y, "zoom": z})
            // This is a simplified implementation - adjust based on your actual CameraState format
            var fromState = System.Text.Json.JsonDocument.Parse(fromCameraState);
            var toState = System.Text.Json.JsonDocument.Parse(toCameraState);

            // Extract coordinates and zoom
            var fromLat = fromState.RootElement.TryGetProperty("lat", out var fLat) ? fLat.GetDouble() : 0;
            var fromLng = fromState.RootElement.TryGetProperty("lng", out var fLng) ? fLng.GetDouble() : 0;
            var fromZoom = fromState.RootElement.TryGetProperty("zoom", out var fZoom) ? fZoom.GetDouble() : 10;

            var toLat = toState.RootElement.TryGetProperty("lat", out var tLat) ? tLat.GetDouble() : 0;
            var toLng = toState.RootElement.TryGetProperty("lng", out var tLng) ? tLng.GetDouble() : 0;
            var toZoom = toState.RootElement.TryGetProperty("zoom", out var tZoom) ? tZoom.GetDouble() : 10;

            // Calculate distance using Haversine formula (approximate)
            var distance = CalculateDistance(fromLat, fromLng, toLat, toLng);
            var zoomDiff = Math.Abs(toZoom - fromZoom);

            // Smart animation type selection
            if (distance < 1.0 && zoomDiff < 2) // Very close, small zoom change
            {
                animationType = CameraAnimationType.Ease;
                durationMs = 800;
            }
            else if (distance < 5.0) // Moderate distance
            {
                animationType = CameraAnimationType.Fly;
                durationMs = 1200;
            }
            else if (distance < 50.0) // Large distance
            {
                animationType = CameraAnimationType.Fly;
                durationMs = 2000;
            }
            else // Very large distance - use jump for better UX
            {
                animationType = CameraAnimationType.Jump;
                durationMs = 500;
            }

            // Adjust duration based on zoom change
            if (zoomDiff > 5)
            {
                durationMs = (int)(durationMs * 1.5);
            }
        }
        catch
        {
            // If parsing fails, use defaults
            return (CameraAnimationType.Fly, 1500);
        }

        return (animationType, durationMs);
    }

    // Haversine formula for distance calculation (in km)
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth radius in km
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    public async Task<Option<IReadOnlyCollection<LocationDto>, Error>> SearchLocationsAsync(string searchTerm,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Option.Some<IReadOnlyCollection<LocationDto>, Error>(new List<LocationDto>());
        }
        
        try
        {
            var osmResults = await _osmService.SearchByNameAsync(searchTerm, null, null, null, 10);
            var osmPoiDtos = new List<LocationDto>();

            foreach (var osmResult in osmResults)
            {
                // Convert OSM result to PoiDto format
                var poiDto = new LocationDto(
                    LocationId: Guid.Empty, // Not in DB yet
                    MapId: Guid.Empty,
                    SegmentId: null,
                    ZoneId: null,
                    Title: osmResult.DisplayName,
                    Subtitle: osmResult.AddressDetails != null
                        ? $"{osmResult.AddressDetails.City}, {osmResult.AddressDetails.Country}"
                        : null,
                    LocationType: LocationType.PointOfInterest,
                    MarkerGeometry: osmResult.GeoJson ?? JsonSerializer.Serialize(new
                    {
                        type = "Point",
                        coordinates = new[] { osmResult.Lon, osmResult.Lat }
                    }),
                    StoryContent: null,
                    MediaResources: null,
                    DisplayOrder: 0,
                    IconType: null,
                    IconUrl: null,
                    IconColor: null,
                    IconSize: 32,
                    HighlightOnEnter: false,
                    ShowTooltip: true,
                    TooltipContent: osmResult.DisplayName,
                    EffectType: null,
                    OpenSlideOnClick: false,
                    SlideContent: null,
                    LinkedLocationId: null,
                    PlayAudioOnClick: false,
                    AudioUrl: null,
                    ExternalUrl: null,
                    AssociatedLayerId: null,
                    AnimationPresetId: null,
                    AnimationOverrides: null,
                    EntryDelayMs: 0,
                    EntryDurationMs: 400,
                    ExitDelayMs: 0,
                    ExitDurationMs: 400,
                    EntryEffect: "fade",
                    ExitEffect: "fade",
                    IsVisible: true,
                    ZIndex: 100,
                    CreatedBy: Guid.Empty,
                    CreatedAt: DateTime.UtcNow,
                    UpdatedAt: null
                );
                osmPoiDtos.Add(poiDto);
            }

            return Option.Some<IReadOnlyCollection<LocationDto>, Error>(osmPoiDtos);
        }
        catch (Exception ex)
        {
            // If OSM search fails, return empty list
            return Option.Some<IReadOnlyCollection<LocationDto>, Error>(new List<LocationDto>());
        }
    }

    public async Task<Option<IReadOnlyCollection<ZoneDto>, Error>> SearchRoutesAsync(string from, string to,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
        {
            return Option.Some<IReadOnlyCollection<ZoneDto>, Error>(new List<ZoneDto>());
        }

        // First, search in database for routes (zones with ZoneType.Route)
        var allZones = await _repository.GetZonesByTypeAsync(ZoneType.Route.ToString(), ct);
        var dbRoutes = allZones
            .Where(z => (z.Name.Contains(from, StringComparison.OrdinalIgnoreCase) ||
                         z.Name.Contains(to, StringComparison.OrdinalIgnoreCase)) &&
                        z.ZoneType == ZoneType.Route)
            .Take(20)
            .ToList();

        var dbRouteDtos = dbRoutes.Select(z => z.ToDto()).ToList();

        // If found results in DB, return them
        if (dbRouteDtos.Count > 0)
        {
            return Option.Some<IReadOnlyCollection<ZoneDto>, Error>(dbRouteDtos);
        }

        // If not found in DB, search OSM for route between two points
        try
        {
            // Geocode both locations
            var fromGeocode = await _osmService.GeocodeAddressAsync(from);
            var toGeocode = await _osmService.GeocodeAddressAsync(to);

            if (fromGeocode == null || toGeocode == null)
            {
                return Option.Some<IReadOnlyCollection<ZoneDto>, Error>(new List<ZoneDto>());
            }

            // Search for routes/highways between the two points
            var searchQuery = $"{from} to {to}";
            var osmResults = await _osmService.SearchByNameAsync(searchQuery,
                (fromGeocode.Lat + toGeocode.Lat) / 2,
                (fromGeocode.Lon + toGeocode.Lon) / 2,
                50000, // 50km radius
                10);

            var osmRouteDtos = new List<ZoneDto>();

            foreach (var osmResult in osmResults)
            {
                // Filter for route-related results
                if (osmResult.Category == "highway" || osmResult.Category == "route" ||
                    osmResult.Type == "highway" || osmResult.Type == "route")
                {
                    var routeDto = new ZoneDto(
                        ZoneId: Guid.Empty, // Not in DB yet
                        ExternalId: $"osm:{osmResult.OsmType}:{osmResult.OsmId}",
                        ZoneCode: $"OSM_ROUTE_{osmResult.OsmId}",
                        Name: $"{from} → {to}",
                        ZoneType: ZoneType.Route,
                        AdminLevel: ZoneAdminLevel.Custom,
                        ParentZoneId: null,
                        Geometry: osmResult.GeoJson,
                        SimplifiedGeometry: null,
                        Centroid: JsonSerializer.Serialize(new
                        {
                            type = "Point",
                            coordinates = new[] { osmResult.Lon, osmResult.Lat }
                        }),
                        BoundingBox: osmResult.BoundingBox != null && osmResult.BoundingBox.Length == 4
                            ? $"{osmResult.BoundingBox[0]},{osmResult.BoundingBox[2]},{osmResult.BoundingBox[1]},{osmResult.BoundingBox[3]}"
                            : null,
                        Description: $"Route from {from} to {to}",
                        IsActive: true,
                        LastSyncedAt: DateTime.UtcNow,
                        CreatedAt: DateTime.UtcNow,
                        UpdatedAt: null
                    );
                    osmRouteDtos.Add(routeDto);
                }
            }

            return Option.Some<IReadOnlyCollection<ZoneDto>, Error>(osmRouteDtos);
        }
        catch (Exception ex)
        {
            // If OSM search fails, return empty list
            return Option.Some<IReadOnlyCollection<ZoneDto>, Error>(new List<ZoneDto>());
        }
    }

    public async Task<Option<string, Error>> SearchRouteBetweenLocationsAsync(
        Guid fromLocationId, Guid toLocationId, string routeType = "road", CancellationToken ct = default)
    {
        var fromLocation = await _repository.GetLocationAsync(fromLocationId, ct);
        var toLocation = await _repository.GetLocationAsync(toLocationId, ct);

        if (fromLocation == null)
        {
            return Option.None<string, Error>(
                Error.NotFound("Location.NotFound", $"From location {fromLocationId} not found"));
        }

        if (toLocation == null)
        {
            return Option.None<string, Error>(
                Error.NotFound("Location.NotFound", $"To location {toLocationId} not found"));
        }

        // Parse coordinates from markerGeometry (GeoJSON Point)
        double? fromLat = null, fromLon = null, toLat = null, toLon = null;

        // Parse from location coordinates
        if (!string.IsNullOrEmpty(fromLocation.MarkerGeometry))
        {
            var fromGeoJson = JsonSerializer.Deserialize<JsonElement>(fromLocation.MarkerGeometry);
            if (fromGeoJson.TryGetProperty("coordinates", out var fromCoords) &&
                fromCoords.ValueKind == JsonValueKind.Array)
            {
                if (fromCoords.GetArrayLength() >= 2)
                {
                    fromLon = fromCoords[0].GetDouble();
                    fromLat = fromCoords[1].GetDouble();
                }
            }
        }

        // Parse to location coordinates
        if (!string.IsNullOrEmpty(toLocation.MarkerGeometry))
        {
            var toGeoJson = JsonSerializer.Deserialize<JsonElement>(toLocation.MarkerGeometry);
            if (toGeoJson.TryGetProperty("coordinates", out var toCoords) &&
                toCoords.ValueKind == JsonValueKind.Array)
            {
                if (toCoords.GetArrayLength() >= 2)
                {
                    toLon = toCoords[0].GetDouble();
                    toLat = toCoords[1].GetDouble();
                }
            }
        }

        // Validate coordinates
        if (!fromLat.HasValue || !fromLon.HasValue || !toLat.HasValue || !toLon.HasValue)
        {
            return Option.None<string, Error>(
                Error.Failure("Location.InvalidGeometry", "One or both locations have invalid geometry"));
        }

            // Get route from OSM service
            var routeGeoJson = await _osmService.GetRouteBetweenPointsAsync(
                fromLat.Value, fromLon.Value, toLat.Value, toLon.Value, routeType);

            return Option.Some<string, Error>(routeGeoJson);
    }

    public async Task<Option<string, Error>> SearchRouteWithMultipleLocationsAsync(
        List<Guid> locationIds, string routeType = "road", CancellationToken ct = default)
    {
        if (locationIds == null || locationIds.Count < 2)
        {
            return Option.None<string, Error>(
                Error.ValidationError("Route.InvalidWaypoints", "At least 2 locations are required"));
        }

        var waypoints = new List<(double lat, double lon)>();

        foreach (var locationId in locationIds)
        {
            var location = await _repository.GetLocationAsync(locationId, ct);
            if (location == null)
            {
                return Option.None<string, Error>(
                    Error.NotFound("Location.NotFound", $"Location {locationId} not found"));
            }

            // Parse coordinates from markerGeometry
            if (string.IsNullOrEmpty(location.MarkerGeometry))
            {
                return Option.None<string, Error>(
                    Error.Failure("Location.InvalidGeometry", $"Location {locationId} has invalid geometry"));
            }

            try
            {
                var geoJson = JsonSerializer.Deserialize<JsonElement>(location.MarkerGeometry);
                if (geoJson.TryGetProperty("coordinates", out var coords) &&
                    coords.ValueKind == JsonValueKind.Array &&
                    coords.GetArrayLength() >= 2)
                {
                    var lon = coords[0].GetDouble();
                    var lat = coords[1].GetDouble();
                    waypoints.Add((lat, lon));
                }
                else
                {
                    return Option.None<string, Error>(
                        Error.Failure("Location.InvalidGeometry", $"Location {locationId} has invalid coordinates"));
                }
            }
            catch (Exception ex)
            {
                return Option.None<string, Error>(
                    Error.Failure("Location.ParseError", $"Failed to parse location {locationId}: {ex.Message}"));
            }
        }

        // Get route from OSM service with waypoints
        var routeGeoJson = await _osmService.GetRouteWithWaypointsAsync(waypoints, routeType);

        return Option.Some<string, Error>(routeGeoJson);
    }

    // ================== ROUTE ANIMATION ==================
    public async Task<Option<IReadOnlyCollection<RouteAnimationDto>, Error>> GetRouteAnimationsBySegmentAsync(
        Guid segmentId, CancellationToken ct = default)
    {
        var segment = await _repository.GetSegmentAsync(segmentId, ct);
        if (segment is null)
        {
            return Option.None<IReadOnlyCollection<RouteAnimationDto>, Error>(
                Error.NotFound("Segment.NotFound", "Segment not found"));
        }

        var routeAnimations = await _repository.GetRouteAnimationsBySegmentAsync(segmentId, ct);
        var dtos = routeAnimations.Select(ra => ra.ToDto()).ToList();

        return Option.Some<IReadOnlyCollection<RouteAnimationDto>, Error>(dtos);
    }

    public async Task<Option<RouteAnimationDto, Error>> GetRouteAnimationAsync(
        Guid routeAnimationId, CancellationToken ct = default)
    {
        var routeAnimation = await _repository.GetRouteAnimationAsync(routeAnimationId, ct);
        if (routeAnimation is null)
        {
            return Option.None<RouteAnimationDto, Error>(
                Error.NotFound("RouteAnimation.NotFound", "Route animation not found"));
        }

        return Option.Some<RouteAnimationDto, Error>(routeAnimation.ToDto());
    }

    public async Task<Option<RouteAnimationDto, Error>> CreateRouteAnimationAsync(
        CreateRouteAnimationRequest request, CancellationToken ct = default)
    {
        var segment = await _repository.GetSegmentAsync(request.SegmentId, ct);
        if (segment is null)
        {
            return Option.None<RouteAnimationDto, Error>(
                Error.NotFound("Segment.NotFound", "Segment not found"));
        }

        var routeAnimation = new RouteAnimation
        {
            RouteAnimationId = Guid.NewGuid(),
            SegmentId = request.SegmentId,
            MapId = segment.MapId,
            FromLat = request.FromLat,
            FromLng = request.FromLng,
            FromName = request.FromName,
            ToLat = request.ToLat,
            ToLng = request.ToLng,
            ToName = request.ToName,
            ToLocationId = request.ToLocationId,
            RoutePath = request.RoutePath,
            Waypoints = request.Waypoints,
            IconType = request.IconType ?? "car",
            IconUrl = request.IconUrl,
            IconWidth = request.IconWidth ?? 32,
            IconHeight = request.IconHeight ?? 32,
            RouteColor = request.RouteColor ?? "#666666",
            VisitedColor = request.VisitedColor ?? "#3b82f6",
            RouteWidth = request.RouteWidth ?? 4,
            DurationMs = request.DurationMs,
            StartDelayMs = request.StartDelayMs,
            Easing = request.Easing ?? "linear",
            AutoPlay = request.AutoPlay ?? true,
            Loop = request.Loop ?? false,
            IsVisible = request.IsVisible ?? true,
            ZIndex = request.ZIndex ?? 1000,
            DisplayOrder = request.DisplayOrder ?? 0,
            StartTimeMs = request.StartTimeMs,
            EndTimeMs = request.EndTimeMs,
            CameraStateBefore = request.CameraStateBefore,
            CameraStateAfter = request.CameraStateAfter,
            ShowLocationInfoOnArrival = request.ShowLocationInfoOnArrival ?? true,
            LocationInfoDisplayDurationMs = request.LocationInfoDisplayDurationMs,
            FollowCamera = request.FollowCamera ?? true,
            FollowCameraZoom = request.FollowCameraZoom,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddRouteAnimationAsync(routeAnimation, ct);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<RouteAnimationDto, Error>(routeAnimation.ToDto());
    }

    public async Task<Option<RouteAnimationDto, Error>> UpdateRouteAnimationAsync(
        Guid routeAnimationId, UpdateRouteAnimationRequest request, CancellationToken ct = default)
    {
        var routeAnimation = await _repository.GetRouteAnimationAsync(routeAnimationId, ct);
        if (routeAnimation is null)
        {
            return Option.None<RouteAnimationDto, Error>(
                Error.NotFound("RouteAnimation.NotFound", "Route animation not found"));
        }

        if (request.FromLat.HasValue) routeAnimation.FromLat = request.FromLat.Value;
        if (request.FromLng.HasValue) routeAnimation.FromLng = request.FromLng.Value;
        if (request.FromName is not null) routeAnimation.FromName = request.FromName;
        if (request.ToLat.HasValue) routeAnimation.ToLat = request.ToLat.Value;
        if (request.ToLng.HasValue) routeAnimation.ToLng = request.ToLng.Value;
        if (request.ToName is not null) routeAnimation.ToName = request.ToName;
        if (request.ToLocationId.HasValue) routeAnimation.ToLocationId = request.ToLocationId;
        if (request.RoutePath is not null) routeAnimation.RoutePath = request.RoutePath;
        if (request.Waypoints is not null) routeAnimation.Waypoints = request.Waypoints;
        if (request.IconType is not null) routeAnimation.IconType = request.IconType;
        if (request.IconUrl is not null) routeAnimation.IconUrl = request.IconUrl;
        if (request.IconWidth.HasValue) routeAnimation.IconWidth = request.IconWidth.Value;
        if (request.IconHeight.HasValue) routeAnimation.IconHeight = request.IconHeight.Value;
        if (request.RouteColor is not null) routeAnimation.RouteColor = request.RouteColor;
        if (request.VisitedColor is not null) routeAnimation.VisitedColor = request.VisitedColor;
        if (request.RouteWidth.HasValue) routeAnimation.RouteWidth = request.RouteWidth.Value;
        if (request.DurationMs.HasValue) routeAnimation.DurationMs = request.DurationMs.Value;
        if (request.StartDelayMs.HasValue) routeAnimation.StartDelayMs = request.StartDelayMs;
        if (request.Easing is not null) routeAnimation.Easing = request.Easing;
        if (request.AutoPlay.HasValue) routeAnimation.AutoPlay = request.AutoPlay.Value;
        if (request.Loop.HasValue) routeAnimation.Loop = request.Loop.Value;
        if (request.IsVisible.HasValue) routeAnimation.IsVisible = request.IsVisible.Value;
        if (request.ZIndex.HasValue) routeAnimation.ZIndex = request.ZIndex.Value;
        if (request.DisplayOrder.HasValue) routeAnimation.DisplayOrder = request.DisplayOrder.Value;
        if (request.StartTimeMs.HasValue) routeAnimation.StartTimeMs = request.StartTimeMs;
        if (request.EndTimeMs.HasValue) routeAnimation.EndTimeMs = request.EndTimeMs;
        if (request.CameraStateBefore is not null) routeAnimation.CameraStateBefore = request.CameraStateBefore;
        if (request.CameraStateAfter is not null) routeAnimation.CameraStateAfter = request.CameraStateAfter;
        if (request.ShowLocationInfoOnArrival.HasValue) routeAnimation.ShowLocationInfoOnArrival = request.ShowLocationInfoOnArrival.Value;
        if (request.LocationInfoDisplayDurationMs.HasValue) routeAnimation.LocationInfoDisplayDurationMs = request.LocationInfoDisplayDurationMs;
        if (request.FollowCamera.HasValue) routeAnimation.FollowCamera = request.FollowCamera.Value;
        if (request.FollowCameraZoom.HasValue) routeAnimation.FollowCameraZoom = request.FollowCameraZoom;

        routeAnimation.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateRouteAnimation(routeAnimation);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<RouteAnimationDto, Error>(routeAnimation.ToDto());
    }

    public async Task<Option<bool, Error>> DeleteRouteAnimationAsync(
        Guid routeAnimationId, CancellationToken ct = default)
    {
        var routeAnimation = await _repository.GetRouteAnimationAsync(routeAnimationId, ct);
        if (routeAnimation is null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("RouteAnimation.NotFound", "Route animation not found"));
        }

        _repository.RemoveRouteAnimation(routeAnimation);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<bool, Error>> MoveZoneToSegmentAsync(
        Guid segmentZoneId, Guid fromSegmentId, Guid toSegmentId, CancellationToken ct = default)
    {
        // Validate segments exist and belong to same map
        var fromSegment = await _repository.GetSegmentAsync(fromSegmentId, ct);
        if (fromSegment is null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Segment.NotFound", $"Source segment {fromSegmentId} not found"));
        }

        var toSegment = await _repository.GetSegmentAsync(toSegmentId, ct);
        if (toSegment is null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Segment.NotFound", $"Target segment {toSegmentId} not found"));
        }

        if (fromSegment.MapId != toSegment.MapId)
        {
            return Option.None<bool, Error>(
                Error.Failure("Segment.MoveInvalid", "Cannot move zone between segments from different maps"));
        }

        // Get the segment zone
        var segmentZone = await _repository.GetSegmentZoneAsync(segmentZoneId, ct);
        if (segmentZone is null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("SegmentZone.NotFound", "Segment zone not found"));
        }

        if (segmentZone.SegmentId != fromSegmentId)
        {
            return Option.None<bool, Error>(
                Error.Failure("SegmentZone.MoveInvalid", "Segment zone does not belong to source segment"));
        }

        // Update segment zone
        segmentZone.SegmentId = toSegmentId;
        segmentZone.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateSegmentZone(segmentZone);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<bool, Error>> MoveLayerToSegmentAsync(
        Guid segmentLayerId, Guid fromSegmentId, Guid toSegmentId, CancellationToken ct = default)
    {
        // Validate segments exist and belong to same map
        var fromSegment = await _repository.GetSegmentAsync(fromSegmentId, ct);
        if (fromSegment is null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Segment.NotFound", $"Source segment {fromSegmentId} not found"));
        }

        var toSegment = await _repository.GetSegmentAsync(toSegmentId, ct);
        if (toSegment is null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Segment.NotFound", $"Target segment {toSegmentId} not found"));
        }

        if (fromSegment.MapId != toSegment.MapId)
        {
            return Option.None<bool, Error>(
                Error.Failure("Segment.MoveInvalid", "Cannot move layer between segments from different maps"));
        }

        // Get the segment layer
        var segmentLayer = await _repository.GetSegmentLayerAsync(segmentLayerId, ct);
        if (segmentLayer is null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("SegmentLayer.NotFound", "Segment layer not found"));
        }

        if (segmentLayer.SegmentId != fromSegmentId)
        {
            return Option.None<bool, Error>(
                Error.Failure("SegmentLayer.MoveInvalid", "Segment layer does not belong to source segment"));
        }

        // Update segment layer
        segmentLayer.SegmentId = toSegmentId;
        segmentLayer.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateSegmentLayer(segmentLayer);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<bool, Error>> MoveRouteToSegmentAsync(
        Guid routeAnimationId, Guid fromSegmentId, Guid toSegmentId, CancellationToken ct = default)
    {
        // Validate segments exist and belong to same map
        var fromSegment = await _repository.GetSegmentAsync(fromSegmentId, ct);
        if (fromSegment is null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Segment.NotFound", $"Source segment {fromSegmentId} not found"));
        }

        var toSegment = await _repository.GetSegmentAsync(toSegmentId, ct);
        if (toSegment is null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Segment.NotFound", $"Target segment {toSegmentId} not found"));
        }

        if (fromSegment.MapId != toSegment.MapId)
        {
            return Option.None<bool, Error>(
                Error.Failure("Segment.MoveInvalid", "Cannot move route between segments from different maps"));
        }

        // Get the route animation
        var routeAnimation = await _repository.GetRouteAnimationAsync(routeAnimationId, ct);
        if (routeAnimation is null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("RouteAnimation.NotFound", "Route animation not found"));
        }

        if (routeAnimation.SegmentId != fromSegmentId)
        {
            return Option.None<bool, Error>(
                Error.Failure("RouteAnimation.MoveInvalid", "Route animation does not belong to source segment"));
        }

        // Update route animation
        routeAnimation.SegmentId = toSegmentId;
        routeAnimation.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateRouteAnimation(routeAnimation);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<bool, Error>(true);
    }
}