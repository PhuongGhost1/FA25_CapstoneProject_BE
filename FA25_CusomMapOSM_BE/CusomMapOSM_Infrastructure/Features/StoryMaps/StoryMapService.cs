using System.Text.Json;
using CusomMapOSM_Application.Common.Mappers;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.StoryMaps;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Domain.Entities.Maps.ErrorMessages;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Segments.Enums;
using CusomMapOSM_Domain.Entities.Timeline;
using CusomMapOSM_Domain.Entities.Timeline.Enums;
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

    public StoryMapService(IStoryMapRepository repository, ICurrentUserService currentUserService,
        ILocationRepository locationRepository, IMapRepository mapRepository)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _locationRepository = locationRepository;
        _mapRepository = mapRepository;
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

        var segmentDtos = new List<SegmentDto>(segments.Count);
        foreach (var segment in segments)
        {
            var segmentZones = await _repository.GetSegmentZonesBySegmentAsync(segment.SegmentId, ct);
            var zones = segmentZones.Select(sz => sz.ToDto()).ToList();

            var segmentLayers = await _repository.GetSegmentLayersBySegmentAsync(segment.SegmentId, ct);
            var layers = segmentLayers.Select(sl => sl.ToDto()).ToList();

            var locationsRaw = await _locationRepository.GetBySegmentIdAsync(segment.SegmentId, ct);
            var locations = locationsRaw.Select(loc => loc.ToPoiDto()).ToList();

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
        var layers = segmentLayers.Select(sl => sl.ToDto()).ToList();
        var pois = locations.Select(l => l.ToPoiDto()).ToList();

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
        var poiDtos = locations.Select(loc => loc.ToPoiDto()).ToList();

        return Option.Some<SegmentDto, Error>(segment.ToSegmentDto(zoneDtos, layerDtos, poiDtos));
    }

    public async Task<Option<bool, Error>> DeleteSegmentAsync(Guid segmentId, CancellationToken ct = default)
    {
        var segment = await _repository.GetSegmentAsync(segmentId, ct);
        if (segment is null)
        {
            return Option.None<bool, Error>(Error.NotFound("StoryMap.Segment.NotFound", "Segment not found"));
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

        // Duplicate segment zones
        var originalZones = await _repository.GetSegmentZonesBySegmentAsync(segmentId, ct);
        foreach (var originalZone in originalZones)
        {
            var newZone = new CusomMapOSM_Domain.Entities.Segments.SegmentZone
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

        // Duplicate segment layers
        var originalLayers = await _repository.GetSegmentLayersBySegmentAsync(segmentId, ct);
        foreach (var originalLayer in originalLayers)
        {
            var newLayer = new CusomMapOSM_Domain.Entities.Segments.SegmentLayer
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

        // Duplicate locations
        var originalLocations = await _locationRepository.GetBySegmentIdAsync(segmentId, ct);
        foreach (var originalLocation in originalLocations)
        {
            var newLocation = new CusomMapOSM_Domain.Entities.Locations.Location
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
                LinkedSegmentId = originalLocation.LinkedSegmentId,
                LinkedLocationId = originalLocation.LinkedLocationId,
                ExternalUrl = originalLocation.ExternalUrl,
                IsVisible = originalLocation.IsVisible,
                ZIndex = originalLocation.ZIndex,
                CreatedBy = userId.Value,
                CreatedAt = DateTime.UtcNow
            };
            await _locationRepository.CreateAsync(newLocation, ct);
        }

        // Duplicate animated layers (only those belonging to the segment, not layer-only)
        var originalAnimatedLayers = await _repository.GetAnimatedLayersBySegmentAsync(segmentId, ct);
        foreach (var originalAnimatedLayer in originalAnimatedLayers)
        {
            var newAnimatedLayer = new CusomMapOSM_Domain.Entities.Animations.AnimatedLayer
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
                CssFilter = originalAnimatedLayer.CssFilter,
                AutoPlay = originalAnimatedLayer.AutoPlay,
                Loop = originalAnimatedLayer.Loop,
                PlaybackSpeed = originalAnimatedLayer.PlaybackSpeed,
                StartTimeMs = originalAnimatedLayer.StartTimeMs,
                EndTimeMs = originalAnimatedLayer.EndTimeMs,
                EntryDelayMs = originalAnimatedLayer.EntryDelayMs,
                EntryDurationMs = originalAnimatedLayer.EntryDurationMs,
                EntryEffect = originalAnimatedLayer.EntryEffect,
                ExitDelayMs = originalAnimatedLayer.ExitDelayMs,
                ExitDurationMs = originalAnimatedLayer.ExitDurationMs,
                ExitEffect = originalAnimatedLayer.ExitEffect,
                EnableClick = originalAnimatedLayer.EnableClick,
                OnClickAction = originalAnimatedLayer.OnClickAction,
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

        var segments = await _repository.GetSegmentsByMapAsync(mapId, ct);

        var existingIds = segments.Select(s => s.SegmentId).ToHashSet();
        var invalidIds = segmentIds.Where(id => !existingIds.Contains(id)).ToList();
        if (invalidIds.Any())
        {
            return Option.None<bool, Error>(Error.ValidationError("Segment.InvalidIds",
                "Some segment IDs are invalid"));
        }

        for (int i = 0; i < segmentIds.Count; i++)
        {
            var segment = segments.First(s => s.SegmentId == segmentIds[i]);
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

        var segmentZone = new CusomMapOSM_Domain.Entities.Segments.SegmentZone
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
        var layerDtos = segmentLayers.Select(sl => sl.ToDto()).ToList();
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

        var segmentLayer = new CusomMapOSM_Domain.Entities.Segments.SegmentLayer
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

        await _repository.AddSegmentLayerAsync(segmentLayer, ct);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<SegmentLayerDto, Error>(segmentLayer.ToDto());
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

        return Option.Some<SegmentLayerDto, Error>(segmentLayer.ToDto());
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

        var zones = await _repository.SearchZonesAsync(searchTerm, ct);
        var zoneDtos = zones.Select(z => z.ToDto()).ToList();
        return Option.Some<IReadOnlyCollection<ZoneDto>, Error>(zoneDtos);
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

        var zone = new CusomMapOSM_Domain.Entities.Zones.Zone
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

    public async Task<Option<ZoneDto, Error>> CreateZoneFromOsmAsync(CreateZoneFromOsmRequest request, CancellationToken ct = default)
    {
        // Validate request
        if (request.OsmId <= 0)
        {
            return Option.None<ZoneDto, Error>(Error.ValidationError("Zone.InvalidOsmId", "OSM ID must be greater than 0"));
        }
        
        if (string.IsNullOrWhiteSpace(request.OsmType))
        {
            return Option.None<ZoneDto, Error>(Error.ValidationError("Zone.InvalidOsmType", "OSM Type is required"));
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
        
        var offset = 0.01;
        string boundingBox = $"{request.Lat - offset},{request.Lon - offset},{request.Lat + offset},{request.Lon + offset}";
        
        try
        {
            if (!string.IsNullOrWhiteSpace(request.GeoJson))
            {
                var geoJson = JsonDocument.Parse(request.GeoJson);
                if (geoJson.RootElement.TryGetProperty("type", out var typeProperty))
                {
                    var geometryType = typeProperty.GetString();

                    if (geometryType == "Point" && geoJson.RootElement.TryGetProperty("coordinates", out var coords))
                    {
                        var lon = coords[0].GetDouble();
                        var lat = coords[1].GetDouble();
                        
                        centroid = JsonSerializer.Serialize(new
                        {
                            type = "Point",
                            coordinates = new[] { lon, lat }
                        });

                        var pointOffset = 0.001;
                        boundingBox = $"{lat - pointOffset},{lon - pointOffset},{lat + pointOffset},{lon + pointOffset}";
                    }
                    else if ((geometryType == "Polygon" || geometryType == "MultiPolygon"))
                    {
                        centroid = JsonSerializer.Serialize(new
                        {
                            type = "Point",
                            coordinates = new[] { request.Lon, request.Lat }
                        });
                        
                        var polyOffset = 0.1;
                        boundingBox = $"{request.Lat - polyOffset},{request.Lon - polyOffset},{request.Lat + polyOffset},{request.Lon + polyOffset}";
                    }
                }
            }
        }
        catch (Exception)
        {
        }

        var zoneType = DetermineZoneType(request.Category, request.Type, request.AdminLevel);
        var adminLevel = DetermineAdminLevel(request.AdminLevel, request.Category, request.Type);

        var zoneCode = $"OSM_{request.OsmType.ToUpperInvariant()}_{request.OsmId}";

        var zone = new CusomMapOSM_Domain.Entities.Zones.Zone
        {
            ZoneId = Guid.NewGuid(),
            ExternalId = externalId,
            ZoneCode = zoneCode,
            Name = request.DisplayName,
            ZoneType = zoneType,
            AdminLevel = adminLevel,
            ParentZoneId = request.ParentZoneId,
            Geometry = request.GeoJson,
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
        // TODO: Implement Overpass API integration
        await Task.CompletedTask;
        return Option.None<int, Error>(Error.Problem("Zone.SyncNotImplemented", "OSM sync not yet implemented"));
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

        var transition = new CusomMapOSM_Domain.Entities.Timeline.TimelineTransition
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

    public async Task<Option<TimelineTransitionDto, Error>> GenerateTransitionAsync(Guid fromSegmentId,
        Guid toSegmentId, CancellationToken ct = default)
    {
        var fromSegment = await _repository.GetSegmentAsync(fromSegmentId, ct);
        if (fromSegment is null)
        {
            return Option.None<TimelineTransitionDto, Error>(
                Error.NotFound("Segment.NotFound", "From segment not found"));
        }

        var toSegment = await _repository.GetSegmentAsync(toSegmentId, ct);
        if (toSegment is null)
        {
            return Option.None<TimelineTransitionDto, Error>(
                Error.NotFound("Segment.NotFound", "To segment not found"));
        }

        // Check if transition already exists
        var existingTransition =
            await _repository.GetTransitionBetweenSegmentsAsync(fromSegmentId, toSegmentId, ct);
        if (existingTransition is not null)
        {
            return Option.None<TimelineTransitionDto, Error>(
                Error.Conflict("Transition.AlreadyExists", "Transition between these segments already exists"));
        }

        // Smart transition generation based on camera states
        var (animationType, animationDuration) = CalculateOptimalTransition(fromSegment.CameraState, toSegment.CameraState);

        // Generate smart transition with sensible defaults
        var transition = new TimelineTransition
        {
            TimelineTransitionId = Guid.NewGuid(),
            MapId = fromSegment.MapId,
            FromSegmentId = fromSegmentId,
            ToSegmentId = toSegmentId,
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
                Error.ValidationError("AnimatedLayer.NoParent", "AnimatedLayer must belong to either a Layer or a Segment"));
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

        var animatedLayer = new CusomMapOSM_Domain.Entities.Animations.AnimatedLayer
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
            CssFilter = request.CssFilter,
            AutoPlay = request.AutoPlay,
            Loop = request.Loop,
            PlaybackSpeed = request.PlaybackSpeed,
            StartTimeMs = request.StartTimeMs,
            EndTimeMs = request.EndTimeMs,
            EntryDelayMs = request.EntryDelayMs,
            EntryDurationMs = request.EntryDurationMs,
            EntryEffect = request.EntryEffect,
            ExitDelayMs = request.ExitDelayMs,
            ExitDurationMs = request.ExitDurationMs,
            ExitEffect = request.ExitEffect,
            EnableClick = request.EnableClick,
            OnClickAction = request.OnClickAction,
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
        animatedLayer.CssFilter = request.CssFilter;
        animatedLayer.AutoPlay = request.AutoPlay;
        animatedLayer.Loop = request.Loop;
        animatedLayer.PlaybackSpeed = request.PlaybackSpeed;
        animatedLayer.StartTimeMs = request.StartTimeMs;
        animatedLayer.EndTimeMs = request.EndTimeMs;
        animatedLayer.EntryDelayMs = request.EntryDelayMs;
        animatedLayer.EntryDurationMs = request.EntryDurationMs;
        animatedLayer.EntryEffect = request.EntryEffect;
        animatedLayer.ExitDelayMs = request.ExitDelayMs;
        animatedLayer.ExitDurationMs = request.ExitDurationMs;
        animatedLayer.ExitEffect = request.ExitEffect;
        animatedLayer.EnableClick = request.EnableClick;
        animatedLayer.OnClickAction = request.OnClickAction;
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
        var preset = new CusomMapOSM_Domain.Entities.Animations.AnimatedLayerPreset
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
                Error.ValidationError("AnimatedLayer.NoParent", "AnimatedLayer must belong to either a Layer or a Segment"));
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
        var animatedLayer = new CusomMapOSM_Domain.Entities.Animations.AnimatedLayer
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
            AutoPlay = preset.DefaultAutoPlay,
            Loop = preset.DefaultLoop,
            PlaybackSpeed = 100,
            StartTimeMs = 0,
            EntryDelayMs = 0,
            EntryDurationMs = 400,
            EntryEffect = "fade",
            ExitDelayMs = 0,
            ExitDurationMs = 400,
            ExitEffect = "fade",
            EnableClick = false,
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
}

