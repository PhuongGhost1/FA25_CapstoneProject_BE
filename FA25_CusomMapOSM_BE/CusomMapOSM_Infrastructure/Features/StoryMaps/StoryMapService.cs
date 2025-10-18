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

        var segmentDtos = new List<SegmentDto>(segments.Count);
        foreach (var segment in segments)
        {
            var zonesRaw = await _repository.GetSegmentZonesBySegmentAsync(segment.SegmentId, ct);
            var layersRaw = await _repository.GetSegmentLayersBySegmentAsync(segment.SegmentId, ct);
            var locationsRaw = await _repository.GetLocationsBySegmentAsync(segment.SegmentId, ct);

            var zones = zonesRaw.Select(z => new SegmentZoneDto(
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

            var layers = layersRaw.Select(l => new SegmentLayerDto(
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

            var locations = locationsRaw.Select(loc => new PoiDto(
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

            var dto = new SegmentDto(
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
            segmentDtos.Add(dto);
        }

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

        // validations/defaults
        var name = string.IsNullOrWhiteSpace(request.Name) ? "Untitled Segment" : request.Name.Trim();
        var displayOrder = request.DisplayOrder < 0 ? 0 : request.DisplayOrder;

        var segment = new Segment
        {
            SegmentId = Guid.NewGuid(),
            MapId = request.MapId,
            CreatedBy = userId.Value,
            Name = name,
            Summary = request.Summary,
            StoryContent = request.StoryContent,
            DisplayOrder = displayOrder,
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
        var name = string.IsNullOrWhiteSpace(request.Name) ? segment.Name : request.Name.Trim();
        var displayOrder = request.DisplayOrder < 0 ? segment.DisplayOrder : request.DisplayOrder;

        segment.Name = name;
        segment.Summary = request.Summary;
        segment.StoryContent = request.StoryContent;
        segment.DisplayOrder = displayOrder;
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

    public async Task<Option<TransitionPreviewDto, Error>> PreviewTransitionAsync(PreviewTransitionRequest request, CancellationToken ct = default)
    {
        var from = await _repository.GetSegmentAsync(request.FromSegmentId, ct);
        var to = await _repository.GetSegmentAsync(request.ToSegmentId, ct);
        if (from is null || to is null)
        {
            return Option.None<TransitionPreviewDto, Error>(Error.NotFound("StoryMap.Transition.Segment.NotFound", "One or both segments not found"));
        }

        // naive suggestion: read primary zones' focus camera or fallback
        var fromZones = await _repository.GetSegmentZonesBySegmentAsync(from.SegmentId, ct);
        var toZones = await _repository.GetSegmentZonesBySegmentAsync(to.SegmentId, ct);
        var fromPrimary = fromZones.FirstOrDefault(z => z.IsPrimary);
        var toPrimary = toZones.FirstOrDefault(z => z.IsPrimary);

        var fromState = fromPrimary?.FocusCameraState;
        var toState = toPrimary?.FocusCameraState;

        // default duration suggestion based on distance if camera states exist
        var suggested = 1500; // ms default
        try
        {
            if (!string.IsNullOrWhiteSpace(fromState) && !string.IsNullOrWhiteSpace(toState))
            {
                suggested = 2000;
            }
        }
        catch { /* keep default */ }

        var dto = new TransitionPreviewDto(
            from.SegmentId,
            to.SegmentId,
            fromState,
            toState,
            suggested,
            "easeInOut");
        return Option.Some<TransitionPreviewDto, Error>(dto);
    }

    public async Task<Option<IReadOnlyCollection<SegmentTransitionDto>, Error>> GetSegmentTransitionsAsync(Guid mapId, CancellationToken ct = default)
    {
        // Do not fail if map is missing; return empty list for robustness
        var transitions = await _repository.GetSegmentTransitionsByMapAsync(mapId, ct);
        var transitionDtos = transitions.Select(t => new SegmentTransitionDto(
            t.SegmentTransitionId,
            t.FromSegmentId,
            t.ToSegmentId,
            t.EffectType,
            t.AnimationPresetId,
            t.DurationMs,
            t.DelayMs,
            t.AutoPlay,
            t.IsSkippable,
            t.TransitionConfig,
            t.Metadata)).ToList();

        return Option.Some<IReadOnlyCollection<SegmentTransitionDto>, Error>(transitionDtos);
    }

    public async Task<Option<SegmentTransitionDto, Error>> CreateSegmentTransitionAsync(CreateSegmentTransitionRequest request, CancellationToken ct = default)
    {
        var map = await _repository.GetMapAsync(request.MapId, ct);
        if (map is null)
        {
            return Option.None<SegmentTransitionDto, Error>(Error.NotFound("StoryMap.Map.NotFound", "Map not found"));
        }

        var fromSegment = await _repository.GetSegmentAsync(request.FromSegmentId, ct);
        var toSegment = await _repository.GetSegmentAsync(request.ToSegmentId, ct);
        if (fromSegment is null || toSegment is null)
        {
            return Option.None<SegmentTransitionDto, Error>(Error.NotFound("StoryMap.Transition.Segment.NotFound", "One or both segments not found"));
        }

        var transition = new SegmentTransition
        {
            SegmentTransitionId = Guid.NewGuid(),
            FromSegmentId = request.FromSegmentId,
            ToSegmentId = request.ToSegmentId,
            EffectType = request.EffectType,
            AnimationPresetId = request.AnimationPresetId,
            DurationMs = request.DurationMs,
            DelayMs = request.DelayMs,
            AutoPlay = request.AutoPlay,
            IsSkippable = request.IsSkippable,
            TransitionConfig = request.TransitionConfig,
            Metadata = request.Metadata
        };

        await _repository.AddSegmentTransitionAsync(transition, ct);
        await _repository.SaveChangesAsync(ct);

        var dto = new SegmentTransitionDto(
            transition.SegmentTransitionId,
            transition.FromSegmentId,
            transition.ToSegmentId,
            transition.EffectType,
            transition.AnimationPresetId,
            transition.DurationMs,
            transition.DelayMs,
            transition.AutoPlay,
            transition.IsSkippable,
            transition.TransitionConfig,
            transition.Metadata);

        return Option.Some<SegmentTransitionDto, Error>(dto);
    }

    public async Task<Option<SegmentTransitionDto, Error>> UpdateSegmentTransitionAsync(Guid transitionId, UpdateSegmentTransitionRequest request, CancellationToken ct = default)
    {
        var transition = await _repository.GetSegmentTransitionAsync(transitionId, ct);
        if (transition is null)
        {
            return Option.None<SegmentTransitionDto, Error>(Error.NotFound("StoryMap.Transition.NotFound", "Transition not found"));
        }

        transition.EffectType = request.EffectType;
        transition.AnimationPresetId = request.AnimationPresetId;
        transition.DurationMs = request.DurationMs;
        transition.DelayMs = request.DelayMs;
        transition.AutoPlay = request.AutoPlay;
        transition.IsSkippable = request.IsSkippable;
        transition.TransitionConfig = request.TransitionConfig;
        transition.Metadata = request.Metadata;

        _repository.UpdateSegmentTransition(transition);
        await _repository.SaveChangesAsync(ct);

        var dto = new SegmentTransitionDto(
            transition.SegmentTransitionId,
            transition.FromSegmentId,
            transition.ToSegmentId,
            transition.EffectType,
            transition.AnimationPresetId,
            transition.DurationMs,
            transition.DelayMs,
            transition.AutoPlay,
            transition.IsSkippable,
            transition.TransitionConfig,
            transition.Metadata);

        return Option.Some<SegmentTransitionDto, Error>(dto);
    }

    public async Task<Option<bool, Error>> DeleteSegmentTransitionAsync(Guid transitionId, CancellationToken ct = default)
    {
        var transition = await _repository.GetSegmentTransitionAsync(transitionId, ct);
        if (transition is null)
        {
            return Option.None<bool, Error>(Error.NotFound("StoryMap.Transition.NotFound", "Transition not found"));
        }

        _repository.RemoveSegmentTransition(transition);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<ExportedStoryDto, Error>> ExportAsync(Guid mapId, CancellationToken ct = default)
    {
        var segmentsResult = await GetSegmentsAsync(mapId, ct);
        if (!segmentsResult.HasValue)
        {
            var err = segmentsResult.Match(_ => (Error?)null, e => e);
            return Option.None<ExportedStoryDto, Error>(err!);
        }

        var timelineResult = await GetTimelineAsync(mapId, ct);
        if (!timelineResult.HasValue)
        {
            var err = timelineResult.Match(_ => (Error?)null, e => e);
            return Option.None<ExportedStoryDto, Error>(err!);
        }

        var segments = segmentsResult.Match(s => s, _ => Array.Empty<SegmentDto>());
        var timeline = timelineResult.Match(t => t, _ => Array.Empty<TimelineStepDto>());
        var dto = new ExportedStoryDto(mapId, segments, timeline);
        return Option.Some<ExportedStoryDto, Error>(dto);
    }

    public async Task<Option<bool, Error>> ImportAsync(ImportStoryRequest request, CancellationToken ct = default)
    {
        // naive upsert: clear existing then recreate minimal data
        var existingSegments = await _repository.GetSegmentsByMapAsync(request.MapId, ct);
        foreach (var seg in existingSegments)
        {
            var delSeg = await DeleteSegmentAsync(seg.SegmentId, ct);
            if (!delSeg.HasValue)
            {
                var err = delSeg.Match(_ => (Error?)null, e => e);
                return Option.None<bool, Error>(err!);
            }
        }
        await _repository.SaveChangesAsync(ct);

        foreach (var seg in request.Segments)
        {
            var createSeg = new CreateSegmentRequest(request.MapId, seg.Name, seg.Summary, seg.StoryContent, seg.DisplayOrder, seg.AutoFitBounds, seg.EntryAnimationPresetId, seg.ExitAnimationPresetId, seg.DefaultLayerAnimationPresetId, seg.PlaybackMode);
            var segRes = await CreateSegmentAsync(createSeg, ct);
            if (!segRes.HasValue)
            {
                var err = segRes.Match(_ => (Error?)null, e => e);
                return Option.None<bool, Error>(err!);
            }
            var segCreated = segRes.Match(s => s, _ => default(SegmentDto));
            var newSegmentId = segCreated!.SegmentId;

            // zones
            foreach (var z in seg.Zones)
            {
                var c = new CreateSegmentZoneRequest(newSegmentId, z.Name, z.Description, z.ZoneType, z.ZoneGeometry, z.FocusCameraState, z.DisplayOrder, z.IsPrimary);
                var zr = await CreateSegmentZoneAsync(c, ct);
                if (!zr.HasValue)
                {
                    var err = zr.Match(_ => (Error?)null, e => e);
                    return Option.None<bool, Error>(err!);
                }
            }

            // layers
            foreach (var l in seg.Layers)
            {
                var c = new UpsertSegmentLayerRequest(l.LayerId, l.SegmentZoneId, l.ExpandToZone, l.HighlightZoneBoundary, l.DisplayOrder, l.DelayMs, l.FadeInMs, l.FadeOutMs, l.StartOpacity, l.EndOpacity, l.Easing, l.AnimationPresetId, l.AutoPlayAnimation, l.RepeatCount, l.AnimationOverrides, l.OverrideStyle, l.Metadata);
                var lr = await CreateSegmentLayerAsync(newSegmentId, c, ct);
                if (!lr.HasValue)
                {
                    var err = lr.Match(_ => (Error?)null, e => e);
                    return Option.None<bool, Error>(err!);
                }
            }
        }

        // timeline
        var existingTimeline = await _repository.GetTimelineByMapAsync(request.MapId, ct);
        foreach (var step in existingTimeline)
        {
            var del = await DeleteTimelineStepAsync(step.TimelineStepId, ct);
            if (!del.HasValue)
            {
                var err = del.Match(_ => (Error?)null, e => e);
                return Option.None<bool, Error>(err!);
            }
        }
        await _repository.SaveChangesAsync(ct);

        foreach (var t in request.Timeline)
        {
            var c = new CreateTimelineStepRequest(request.MapId, t.SegmentId, t.Title, t.Subtitle, t.Description, t.DisplayOrder, t.AutoAdvance, t.DurationMs, t.TriggerType, t.CameraState, t.OverlayContent, Array.Empty<CreateTimelineStepLayerRequest>());
            var tr = await CreateTimelineStepAsync(c, ct);
            if (!tr.HasValue)
            {
                var err = tr.Match(_ => (Error?)null, e => e);
                return Option.None<bool, Error>(err!);
            }
        }

        return Option.Some<bool, Error>(true);
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

        var timelineDtos = new List<TimelineStepDto>(steps.Count);
        foreach (var step in steps)
        {
            var layersRaw = await _repository.GetTimelineStepLayersAsync(step.TimelineStepId, ct);
            var layerDtos = layersRaw.Select(l => new TimelineStepLayerDto(
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

            var dto = new TimelineStepDto(
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
            timelineDtos.Add(dto);
        }

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
}
