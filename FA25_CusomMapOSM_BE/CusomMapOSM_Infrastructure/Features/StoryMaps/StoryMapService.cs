using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.StoryMaps;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.POIs;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Timeline;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.StoryMaps;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.StoryMaps;

public class StoryMapService : IStoryMapService
{
    private readonly IStoryMapRepository _repository;
    private readonly ICurrentUserService  _currentUserService;

    public StoryMapService(IStoryMapRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Option<IReadOnlyCollection<SegmentDto>, Error>> GetSegmentsAsync(Guid mapId, CancellationToken ct = default)
    {
        var map = await _repository.GetMapAsync(mapId, ct);
        if (map is null)
        {
            return Option.None<IReadOnlyCollection<SegmentDto>, Error>(Error.NotFound("StoryMap.Map.NotFound", "Map not found"));
        }

        var segments = await _repository.GetSegmentsByMapAsync(mapId, ct);
        if (segments.Count == 0)
        {
            return Option.Some<IReadOnlyCollection<SegmentDto>, Error>(Array.Empty<SegmentDto>());
        }

        var zoneTasks = segments.Select(s => _repository.GetSegmentZonesBySegmentAsync(s.SegmentId, ct)).ToList();
        var layerTasks = segments.Select(s => _repository.GetSegmentLayersBySegmentAsync(s.SegmentId, ct)).ToList();
        var locationTasks = segments.Select(s => _repository.GetLocationsBySegmentAsync(s.SegmentId, ct)).ToList();

        await Task.WhenAll(zoneTasks.Cast<Task>()
            .Concat(layerTasks.Cast<Task>())
            .Concat(locationTasks.Cast<Task>()));

        var segmentDtos = segments.Select((segment, index) =>
        {
            var zones = zoneTasks[index].Result.Select(z => new SegmentZoneDto(
                z.SegmentZoneId,
                z.SegmentId,
                z.Name,
                z.Description,
                z.ZoneType,
                z.ZoneGeometry,
                z.FocusCameraState,
                z.DisplayOrder,
                z.IsPrimary,
                z.CreatedAt,
                z.UpdatedAt)).ToList();

            var layers = layerTasks[index].Result.Select(l => new SegmentLayerDto(
                l.SegmentLayerId,
                l.SegmentId,
                l.LayerId,
                l.SegmentZoneId,
                l.ExpandToZone,
                l.HighlightZoneBoundary,
                l.DisplayOrder,
                l.DelayMs,
                l.FadeInMs,
                l.FadeOutMs,
                l.StartOpacity,
                l.EndOpacity,
                l.Easing,
                l.AnimationPresetId,
                l.AutoPlayAnimation,
                l.RepeatCount,
                l.AnimationOverrides,
                l.OverrideStyle,
                l.Metadata)).ToList();

            var locations = locationTasks[index].Result.Select(loc => new PoiDto(
                loc.LocationId,
                loc.MapId,
                loc.SegmentId,
                loc.SegmentZoneId,
                loc.Title,
                loc.Subtitle,
                loc.LocationType,
                loc.MarkerGeometry,
                loc.StoryContent,
                loc.MediaResources,
                loc.DisplayOrder,
                loc.HighlightOnEnter,
                loc.ShowTooltip,
                loc.TooltipContent,
                loc.EffectType,
                loc.OpenSlideOnClick,
                loc.SlideContent,
                loc.LinkedLocationId,
                loc.PlayAudioOnClick,
                loc.AudioUrl,
                loc.ExternalUrl,
                loc.AssociatedLayerId,
                loc.AnimationPresetId,
                loc.AnimationOverrides,
                loc.CreatedAt,
                loc.UpdatedAt)).ToList();

            return new SegmentDto(
                segment.SegmentId,
                segment.MapId,
                segment.Name,
                segment.Summary,
                segment.StoryContent,
                segment.DisplayOrder,
                segment.AutoFitBounds,
                segment.EntryAnimationPresetId,
                segment.ExitAnimationPresetId,
                segment.DefaultLayerAnimationPresetId,
                segment.PlaybackMode,
                segment.CreatedAt,
                segment.UpdatedAt,
                zones,
                layers,
                locations);
        }).ToList();

        return Option.Some<IReadOnlyCollection<SegmentDto>, Error>(segmentDtos);
    }

    public async Task<Option<SegmentDto, Error>> CreateSegmentAsync(CreateSegmentRequest request, CancellationToken ct = default)
    {
        var map = await _repository.GetMapAsync(request.MapId, ct);
        if (map is null)
        {
            return Option.None<SegmentDto, Error>(Error.NotFound("StoryMap.Map.NotFound", "Map not found"));
        }

        var userId = _currentUserService.GetUserId();
        
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var segment = new Segment
        {
            SegmentId = Guid.NewGuid(),
            MapId = request.MapId,
            CreatedBy = userId.Value,
            Name = request.Name,
            Summary = request.Summary,
            StoryContent = request.StoryContent,
            DisplayOrder = request.DisplayOrder,
            AutoFitBounds = request.AutoFitBounds,
            EntryAnimationPresetId = request.EntryAnimationPresetId,
            ExitAnimationPresetId = request.ExitAnimationPresetId,
            DefaultLayerAnimationPresetId = request.DefaultLayerAnimationPresetId,
            PlaybackMode = request.PlaybackMode,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddSegmentAsync(segment, ct);
        await _repository.SaveChangesAsync(ct);

        var segmentDto = new SegmentDto(
            segment.SegmentId,
            segment.MapId,
            segment.Name,
            segment.Summary,
            segment.StoryContent,
            segment.DisplayOrder,
            segment.AutoFitBounds,
            segment.EntryAnimationPresetId,
            segment.ExitAnimationPresetId,
            segment.DefaultLayerAnimationPresetId,
            segment.PlaybackMode,
            segment.CreatedAt,
            segment.UpdatedAt,
            Array.Empty<SegmentZoneDto>(),
            Array.Empty<SegmentLayerDto>(),
            Array.Empty<PoiDto>());

        return Option.Some<SegmentDto, Error>(segmentDto);
    }

    public async Task<Option<SegmentDto, Error>> UpdateSegmentAsync(Guid segmentId, UpdateSegmentRequest request, CancellationToken ct = default)
    {
        var segment = await _repository.GetSegmentAsync(segmentId, ct);
        if (segment is null)
        {
            return Option.None<SegmentDto, Error>(Error.NotFound("StoryMap.Segment.NotFound", "Segment not found"));
        }

        segment.Name = request.Name;
        segment.Summary = request.Summary;
        segment.StoryContent = request.StoryContent;
        segment.DisplayOrder = request.DisplayOrder;
        segment.AutoFitBounds = request.AutoFitBounds;
        segment.EntryAnimationPresetId = request.EntryAnimationPresetId;
        segment.ExitAnimationPresetId = request.ExitAnimationPresetId;
        segment.DefaultLayerAnimationPresetId = request.DefaultLayerAnimationPresetId;
        segment.PlaybackMode = request.PlaybackMode;
        segment.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateSegment(segment);
        await _repository.SaveChangesAsync(ct);

        var zones = await _repository.GetSegmentZonesBySegmentAsync(segmentId, ct);
        var layers = await _repository.GetSegmentLayersBySegmentAsync(segmentId, ct);
        var locations = await _repository.GetLocationsBySegmentAsync(segmentId, ct);

        var zoneDtos = zones.Select(z => new SegmentZoneDto(
            z.SegmentZoneId,
            z.SegmentId,
            z.Name,
            z.Description,
            z.ZoneType,
            z.ZoneGeometry,
            z.FocusCameraState,
            z.DisplayOrder,
            z.IsPrimary,
            z.CreatedAt,
            z.UpdatedAt)).ToList();

        var layerDtos = layers.Select(l => new SegmentLayerDto(
            l.SegmentLayerId,
            l.SegmentId,
            l.LayerId,
            l.SegmentZoneId,
            l.ExpandToZone,
            l.HighlightZoneBoundary,
            l.DisplayOrder,
            l.DelayMs,
            l.FadeInMs,
            l.FadeOutMs,
            l.StartOpacity,
            l.EndOpacity,
            l.Easing,
            l.AnimationPresetId,
            l.AutoPlayAnimation,
            l.RepeatCount,
            l.AnimationOverrides,
            l.OverrideStyle,
            l.Metadata)).ToList();

        var poiDtos = locations.Select(loc => new PoiDto(
            loc.LocationId,
            loc.MapId,
            loc.SegmentId,
            loc.SegmentZoneId,
            loc.Title,
            loc.Subtitle,
            loc.LocationType,
            loc.MarkerGeometry,
            loc.StoryContent,
            loc.MediaResources,
            loc.DisplayOrder,
            loc.HighlightOnEnter,
            loc.ShowTooltip,
            loc.TooltipContent,
            loc.EffectType,
            loc.OpenSlideOnClick,
            loc.SlideContent,
            loc.LinkedLocationId,
            loc.PlayAudioOnClick,
            loc.AudioUrl,
            loc.ExternalUrl,
            loc.AssociatedLayerId,
            loc.AnimationPresetId,
            loc.AnimationOverrides,
            loc.CreatedAt,
            loc.UpdatedAt)).ToList();

        var segmentDto = new SegmentDto(
            segment.SegmentId,
            segment.MapId,
            segment.Name,
            segment.Summary,
            segment.StoryContent,
            segment.DisplayOrder,
            segment.AutoFitBounds,
            segment.EntryAnimationPresetId,
            segment.ExitAnimationPresetId,
            segment.DefaultLayerAnimationPresetId,
            segment.PlaybackMode,
            segment.CreatedAt,
            segment.UpdatedAt,
            zoneDtos,
            layerDtos,
            poiDtos);

        return Option.Some<SegmentDto, Error>(segmentDto);
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

    public async Task<Option<IReadOnlyCollection<SegmentZoneDto>, Error>> GetSegmentZonesAsync(Guid segmentId, CancellationToken ct = default)
    {
        var segment = await _repository.GetSegmentAsync(segmentId, ct);
        if (segment is null)
        {
            return Option.None<IReadOnlyCollection<SegmentZoneDto>, Error>(Error.NotFound("StoryMap.Segment.NotFound", "Segment not found"));
        }

        var zones = await _repository.GetSegmentZonesBySegmentAsync(segmentId, ct);
        var zoneDtos = zones.Select(z => new SegmentZoneDto(
            z.SegmentZoneId,
            z.SegmentId,
            z.Name,
            z.Description,
            z.ZoneType,
            z.ZoneGeometry,
            z.FocusCameraState,
            z.DisplayOrder,
            z.IsPrimary,
            z.CreatedAt,
            z.UpdatedAt)).ToList();
        return Option.Some<IReadOnlyCollection<SegmentZoneDto>, Error>(zoneDtos);
    }

    public async Task<Option<SegmentZoneDto, Error>> CreateSegmentZoneAsync(CreateSegmentZoneRequest request, CancellationToken ct = default)
    {
        var segment = await _repository.GetSegmentAsync(request.SegmentId, ct);
        if (segment is null)
        {
            return Option.None<SegmentZoneDto, Error>(Error.NotFound("StoryMap.Segment.NotFound", "Segment not found"));
        }

        var zone = new SegmentZone
        {
            SegmentZoneId = Guid.NewGuid(),
            SegmentId = request.SegmentId,
            Name = request.Name,
            Description = request.Description,
            ZoneType = request.ZoneType,
            ZoneGeometry = request.ZoneGeometry,
            FocusCameraState = request.FocusCameraState,
            DisplayOrder = request.DisplayOrder,
            IsPrimary = request.IsPrimary,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddSegmentZoneAsync(zone, ct);
        await _repository.SaveChangesAsync(ct);

        var zoneDto = new SegmentZoneDto(
            zone.SegmentZoneId,
            zone.SegmentId,
            zone.Name,
            zone.Description,
            zone.ZoneType,
            zone.ZoneGeometry,
            zone.FocusCameraState,
            zone.DisplayOrder,
            zone.IsPrimary,
            zone.CreatedAt,
            zone.UpdatedAt);

        return Option.Some<SegmentZoneDto, Error>(zoneDto);
    }

    public async Task<Option<SegmentZoneDto, Error>> UpdateSegmentZoneAsync(Guid segmentZoneId, UpdateSegmentZoneRequest request, CancellationToken ct = default)
    {
        var zone = await _repository.GetSegmentZoneAsync(segmentZoneId, ct);
        if (zone is null)
        {
            return Option.None<SegmentZoneDto, Error>(Error.NotFound("StoryMap.Zone.NotFound", "Segment zone not found"));
        }

        zone.Name = request.Name;
        zone.Description = request.Description;
        zone.ZoneType = request.ZoneType;
        zone.ZoneGeometry = request.ZoneGeometry;
        zone.FocusCameraState = request.FocusCameraState;
        zone.DisplayOrder = request.DisplayOrder;
        zone.IsPrimary = request.IsPrimary;
        zone.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateSegmentZone(zone);
        await _repository.SaveChangesAsync(ct);

        var zoneDto = new SegmentZoneDto(
            zone.SegmentZoneId,
            zone.SegmentId,
            zone.Name,
            zone.Description,
            zone.ZoneType,
            zone.ZoneGeometry,
            zone.FocusCameraState,
            zone.DisplayOrder,
            zone.IsPrimary,
            zone.CreatedAt,
            zone.UpdatedAt);

        return Option.Some<SegmentZoneDto, Error>(zoneDto);
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

    public async Task<Option<IReadOnlyCollection<SegmentLayerDto>, Error>> GetSegmentLayersAsync(Guid segmentId, CancellationToken ct = default)
    {
        var segment = await _repository.GetSegmentAsync(segmentId, ct);
        if (segment is null)
        {
            return Option.None<IReadOnlyCollection<SegmentLayerDto>, Error>(Error.NotFound("StoryMap.Segment.NotFound", "Segment not found"));
        }

        var layers = await _repository.GetSegmentLayersBySegmentAsync(segmentId, ct);
        var layerDtos = layers.Select(l => new SegmentLayerDto(
            l.SegmentLayerId,
            l.SegmentId,
            l.LayerId,
            l.SegmentZoneId,
            l.ExpandToZone,
            l.HighlightZoneBoundary,
            l.DisplayOrder,
            l.DelayMs,
            l.FadeInMs,
            l.FadeOutMs,
            l.StartOpacity,
            l.EndOpacity,
            l.Easing,
            l.AnimationPresetId,
            l.AutoPlayAnimation,
            l.RepeatCount,
            l.AnimationOverrides,
            l.OverrideStyle,
            l.Metadata)).ToList();
        return Option.Some<IReadOnlyCollection<SegmentLayerDto>, Error>(layerDtos);
    }

    public async Task<Option<SegmentLayerDto, Error>> CreateSegmentLayerAsync(Guid segmentId, UpsertSegmentLayerRequest request, CancellationToken ct = default)
    {
        var segment = await _repository.GetSegmentAsync(segmentId, ct);
        if (segment is null)
        {
            return Option.None<SegmentLayerDto, Error>(Error.NotFound("StoryMap.Segment.NotFound", "Segment not found"));
        }

        if (request.SegmentZoneId.HasValue)
        {
            var zone = await _repository.GetSegmentZoneAsync(request.SegmentZoneId.Value, ct);
            if (zone is null)
            {
                return Option.None<SegmentLayerDto, Error>(Error.NotFound("StoryMap.Zone.NotFound", "Segment zone not found"));
            }
        }

        var layer = new SegmentLayer
        {
            SegmentLayerId = Guid.NewGuid(),
            SegmentId = segmentId,
            LayerId = request.LayerId,
            SegmentZoneId = request.SegmentZoneId,
            ExpandToZone = request.ExpandToZone,
            HighlightZoneBoundary = request.HighlightZoneBoundary,
            DisplayOrder = request.DisplayOrder,
            DelayMs = request.DelayMs,
            FadeInMs = request.FadeInMs,
            FadeOutMs = request.FadeOutMs,
            StartOpacity = request.StartOpacity,
            EndOpacity = request.EndOpacity,
            Easing = request.Easing,
            AnimationPresetId = request.AnimationPresetId,
            AutoPlayAnimation = request.AutoPlayAnimation,
            RepeatCount = request.RepeatCount,
            AnimationOverrides = request.AnimationOverrides,
            OverrideStyle = request.OverrideStyle,
            Metadata = request.Metadata
        };

        await _repository.AddSegmentLayerAsync(layer, ct);
        await _repository.SaveChangesAsync(ct);

        var layerDto = new SegmentLayerDto(
            layer.SegmentLayerId,
            layer.SegmentId,
            layer.LayerId,
            layer.SegmentZoneId,
            layer.ExpandToZone,
            layer.HighlightZoneBoundary,
            layer.DisplayOrder,
            layer.DelayMs,
            layer.FadeInMs,
            layer.FadeOutMs,
            layer.StartOpacity,
            layer.EndOpacity,
            layer.Easing,
            layer.AnimationPresetId,
            layer.AutoPlayAnimation,
            layer.RepeatCount,
            layer.AnimationOverrides,
            layer.OverrideStyle,
            layer.Metadata);

        return Option.Some<SegmentLayerDto, Error>(layerDto);
    }

    public async Task<Option<SegmentLayerDto, Error>> UpdateSegmentLayerAsync(Guid segmentLayerId, UpsertSegmentLayerRequest request, CancellationToken ct = default)
    {
        var layer = await _repository.GetSegmentLayerAsync(segmentLayerId, ct);
        if (layer is null)
        {
            return Option.None<SegmentLayerDto, Error>(Error.NotFound("StoryMap.Layer.NotFound", "Segment layer not found"));
        }

        if (request.SegmentZoneId.HasValue)
        {
            var zone = await _repository.GetSegmentZoneAsync(request.SegmentZoneId.Value, ct);
            if (zone is null)
            {
                return Option.None<SegmentLayerDto, Error>(Error.NotFound("StoryMap.Zone.NotFound", "Segment zone not found"));
            }
        }

        layer.LayerId = request.LayerId;
        layer.SegmentZoneId = request.SegmentZoneId;
        layer.ExpandToZone = request.ExpandToZone;
        layer.HighlightZoneBoundary = request.HighlightZoneBoundary;
        layer.DisplayOrder = request.DisplayOrder;
        layer.DelayMs = request.DelayMs;
        layer.FadeInMs = request.FadeInMs;
        layer.FadeOutMs = request.FadeOutMs;
        layer.StartOpacity = request.StartOpacity;
        layer.EndOpacity = request.EndOpacity;
        layer.Easing = request.Easing;
        layer.AnimationPresetId = request.AnimationPresetId;
        layer.AutoPlayAnimation = request.AutoPlayAnimation;
        layer.RepeatCount = request.RepeatCount;
        layer.AnimationOverrides = request.AnimationOverrides;
        layer.OverrideStyle = request.OverrideStyle;
        layer.Metadata = request.Metadata;

        _repository.UpdateSegmentLayer(layer);
        await _repository.SaveChangesAsync(ct);

        var layerDto = new SegmentLayerDto(
            layer.SegmentLayerId,
            layer.SegmentId,
            layer.LayerId,
            layer.SegmentZoneId,
            layer.ExpandToZone,
            layer.HighlightZoneBoundary,
            layer.DisplayOrder,
            layer.DelayMs,
            layer.FadeInMs,
            layer.FadeOutMs,
            layer.StartOpacity,
            layer.EndOpacity,
            layer.Easing,
            layer.AnimationPresetId,
            layer.AutoPlayAnimation,
            layer.RepeatCount,
            layer.AnimationOverrides,
            layer.OverrideStyle,
            layer.Metadata);

        return Option.Some<SegmentLayerDto, Error>(layerDto);
    }

    public async Task<Option<bool, Error>> DeleteSegmentLayerAsync(Guid segmentLayerId, CancellationToken ct = default)
    {
        var layer = await _repository.GetSegmentLayerAsync(segmentLayerId, ct);
        if (layer is null)
        {
            return Option.None<bool, Error>(Error.NotFound("StoryMap.Layer.NotFound", "Segment layer not found"));
        }

        _repository.RemoveSegmentLayer(layer);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<IReadOnlyCollection<TimelineStepDto>, Error>> GetTimelineAsync(Guid mapId, CancellationToken ct = default)
    {
        var map = await _repository.GetMapAsync(mapId, ct);
        if (map is null)
        {
            return Option.None<IReadOnlyCollection<TimelineStepDto>, Error>(Error.NotFound("StoryMap.Map.NotFound", "Map not found"));
        }

        var steps = await _repository.GetTimelineByMapAsync(mapId, ct);
        if (steps.Count == 0)
        {
            return Option.Some<IReadOnlyCollection<TimelineStepDto>, Error>(Array.Empty<TimelineStepDto>());
        }

        var layersTasks = steps.Select(step => _repository.GetTimelineStepLayersAsync(step.TimelineStepId, ct)).ToList();
        await Task.WhenAll(layersTasks);

        var timelineDtos = steps.Select((step, index) =>
        {
            var layerDtos = layersTasks[index].Result.Select(l => new TimelineStepLayerDto(
                l.TimelineStepLayerId,
                l.TimelineStepId,
                l.LayerId,
                l.IsVisible,
                l.Opacity,
                l.FadeInMs,
                l.FadeOutMs,
                l.DelayMs,
                l.DisplayMode,
                l.StyleOverride,
                l.Metadata)).ToList();

            return new TimelineStepDto(
                step.TimelineStepId,
                step.MapId,
                step.SegmentId,
                step.Title,
                step.Subtitle,
                step.Description,
                step.DisplayOrder,
                step.AutoAdvance,
                step.DurationMs,
                step.TriggerType,
                step.CameraState,
                step.OverlayContent,
                step.CreatedAt,
                layerDtos);
        }).ToList();

        return Option.Some<IReadOnlyCollection<TimelineStepDto>, Error>(timelineDtos);
    }

    public async Task<Option<TimelineStepDto, Error>> CreateTimelineStepAsync(CreateTimelineStepRequest request, CancellationToken ct = default)
    {
        var map = await _repository.GetMapAsync(request.MapId, ct);
        if (map is null)
        {
            return Option.None<TimelineStepDto, Error>(Error.NotFound("StoryMap.Map.NotFound", "Map not found"));
        }

        if (request.SegmentId.HasValue)
        {
            var segment = await _repository.GetSegmentAsync(request.SegmentId.Value, ct);
            if (segment is null)
            {
                return Option.None<TimelineStepDto, Error>(Error.NotFound("StoryMap.Segment.NotFound", "Segment not found"));
            }
        }

        var step = new TimelineStep
        {
            TimelineStepId = Guid.NewGuid(),
            MapId = request.MapId,
            SegmentId = request.SegmentId,
            Title = request.Title,
            Subtitle = request.Subtitle,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            AutoAdvance = request.AutoAdvance,
            DurationMs = request.DurationMs,
            TriggerType = request.TriggerType,
            CameraState = request.CameraState,
            OverlayContent = request.OverlayContent,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddTimelineStepAsync(step, ct);

        var layerEntities = request.Layers?.Select(l => new TimelineStepLayer
        {
            TimelineStepLayerId = Guid.NewGuid(),
            TimelineStepId = step.TimelineStepId,
            LayerId = l.LayerId,
            IsVisible = l.IsVisible,
            Opacity = l.Opacity,
            FadeInMs = l.FadeInMs,
            FadeOutMs = l.FadeOutMs,
            DelayMs = l.DelayMs,
            DisplayMode = l.DisplayMode,
            StyleOverride = l.StyleOverride,
            Metadata = l.Metadata
        }).ToList() ?? new List<TimelineStepLayer>();

        if (layerEntities.Count > 0)
        {
            _repository.AddTimelineStepLayers(layerEntities);
        }

        await _repository.SaveChangesAsync(ct);

        var layerDtos = layerEntities.Select(l => new TimelineStepLayerDto(
            l.TimelineStepLayerId,
            l.TimelineStepId,
            l.LayerId,
            l.IsVisible,
            l.Opacity,
            l.FadeInMs,
            l.FadeOutMs,
            l.DelayMs,
            l.DisplayMode,
            l.StyleOverride,
            l.Metadata)).ToList();

        var timelineDto = new TimelineStepDto(
            step.TimelineStepId,
            step.MapId,
            step.SegmentId,
            step.Title,
            step.Subtitle,
            step.Description,
            step.DisplayOrder,
            step.AutoAdvance,
            step.DurationMs,
            step.TriggerType,
            step.CameraState,
            step.OverlayContent,
            step.CreatedAt,
            layerDtos);

        return Option.Some<TimelineStepDto, Error>(timelineDto);
    }

    public async Task<Option<TimelineStepDto, Error>> UpdateTimelineStepAsync(Guid timelineStepId, UpdateTimelineStepRequest request, CancellationToken ct = default)
    {
        var step = await _repository.GetTimelineStepAsync(timelineStepId, ct);
        if (step is null)
        {
            return Option.None<TimelineStepDto, Error>(Error.NotFound("StoryMap.Timeline.NotFound", "Timeline step not found"));
        }

        if (request.SegmentId.HasValue)
        {
            var segment = await _repository.GetSegmentAsync(request.SegmentId.Value, ct);
            if (segment is null)
            {
                return Option.None<TimelineStepDto, Error>(Error.NotFound("StoryMap.Segment.NotFound", "Segment not found"));
            }
        }

        step.SegmentId = request.SegmentId;
        step.Title = request.Title;
        step.Subtitle = request.Subtitle;
        step.Description = request.Description;
        step.DisplayOrder = request.DisplayOrder;
        step.AutoAdvance = request.AutoAdvance;
        step.DurationMs = request.DurationMs;
        step.TriggerType = request.TriggerType;
        step.CameraState = request.CameraState;
        step.OverlayContent = request.OverlayContent;

        _repository.UpdateTimelineStep(step);

        var existingLayers = await _repository.GetTimelineStepLayersAsync(timelineStepId, ct);
        if (existingLayers.Count > 0)
        {
            _repository.RemoveTimelineStepLayers(existingLayers);
        }

        var newLayerEntities = request.Layers?.Select(l => new TimelineStepLayer
        {
            TimelineStepLayerId = Guid.NewGuid(),
            TimelineStepId = timelineStepId,
            LayerId = l.LayerId,
            IsVisible = l.IsVisible,
            Opacity = l.Opacity,
            FadeInMs = l.FadeInMs,
            FadeOutMs = l.FadeOutMs,
            DelayMs = l.DelayMs,
            DisplayMode = l.DisplayMode,
            StyleOverride = l.StyleOverride,
            Metadata = l.Metadata
        }).ToList() ?? new List<TimelineStepLayer>();

        if (newLayerEntities.Count > 0)
        {
            _repository.AddTimelineStepLayers(newLayerEntities);
        }

        await _repository.SaveChangesAsync(ct);

        var layerDtos = newLayerEntities.Select(l => new TimelineStepLayerDto(
            l.TimelineStepLayerId,
            l.TimelineStepId,
            l.LayerId,
            l.IsVisible,
            l.Opacity,
            l.FadeInMs,
            l.FadeOutMs,
            l.DelayMs,
            l.DisplayMode,
            l.StyleOverride,
            l.Metadata)).ToList();

        var timelineDto = new TimelineStepDto(
            step.TimelineStepId,
            step.MapId,
            step.SegmentId,
            step.Title,
            step.Subtitle,
            step.Description,
            step.DisplayOrder,
            step.AutoAdvance,
            step.DurationMs,
            step.TriggerType,
            step.CameraState,
            step.OverlayContent,
            step.CreatedAt,
            layerDtos);

        return Option.Some<TimelineStepDto, Error>(timelineDto);
    }

    public async Task<Option<bool, Error>> DeleteTimelineStepAsync(Guid timelineStepId, CancellationToken ct = default)
    {
        var step = await _repository.GetTimelineStepAsync(timelineStepId, ct);
        if (step is null)
        {
            return Option.None<bool, Error>(Error.NotFound("StoryMap.Timeline.NotFound", "Timeline step not found"));
        }

        _repository.RemoveTimelineStep(step);
        await _repository.SaveChangesAsync(ct);
        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<ZoneAnalyticsResponse, Error>> GetZoneAnalyticsAsync(ZoneAnalyticsRequest request, CancellationToken ct = default)
    {
        if (request.ZoneIds.Count == 0)
        {
            return Option.Some<ZoneAnalyticsResponse, Error>(new ZoneAnalyticsResponse(Array.Empty<ZoneAnalyticsItem>()));
        }

        var zones = await _repository.GetAdministrativeZonesAsync(request.ZoneIds, ct);
        if (zones.Count == 0)
        {
            return Option.None<ZoneAnalyticsResponse, Error>(Error.NotFound("StoryMap.ZoneAnalytics.NotFound", "No zones found for the provided identifiers"));
        }

        var stats = await _repository.GetZoneStatisticsAsync(request.ZoneIds, ct);
        var insights = await _repository.GetZoneInsightsAsync(request.ZoneIds, ct);

        var analyticsItems = new List<ZoneAnalyticsItem>();
        foreach (var zone in zones)
        {
            var zoneStats = stats.Where(s => s.ZoneId == zone.ZoneId)
                .Select(s => new ZoneStatisticItem(
                    s.ZoneStatisticId,
                    s.MetricType,
                    s.NumericValue,
                    s.TextValue,
                    s.Unit,
                    s.Year,
                    s.Quarter,
                    s.Source,
                    s.Metadata,
                    s.CollectedAt))
                .ToList();

            var zoneInsights = insights.Where(i => i.ZoneId == zone.ZoneId)
                .Select(i => new ZoneInsightItem(
                    i.ZoneInsightId,
                    i.InsightType,
                    i.Title,
                    i.Summary,
                    i.Description,
                    i.ImageUrl,
                    i.ExternalUrl,
                    i.Location,
                    i.Metadata,
                    i.CreatedAt,
                    i.UpdatedAt))
                .ToList();

            analyticsItems.Add(new ZoneAnalyticsItem(
                zone.ZoneId,
                zone.Name,
                zone.AdminLevel,
                zone.ZoneCode,
                zone.Geometry,
                zone.SimplifiedGeometry,
                zone.Centroid,
                zone.BoundingBox,
                zoneStats,
                zoneInsights));
        }

        return Option.Some<ZoneAnalyticsResponse, Error>(new ZoneAnalyticsResponse(analyticsItems));
    }

}
