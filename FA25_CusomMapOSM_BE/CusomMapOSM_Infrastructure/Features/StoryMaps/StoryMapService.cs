using CusomMapOSM_Application.Common.Mappers;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.StoryMaps;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Interfaces.Services.StoryMaps;
using CusomMapOSM_Application.Models.DTOs.Features.POIs;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Domain.Entities.Maps.ErrorMessages;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.StoryElement;
using CusomMapOSM_Domain.Entities.StoryElement.Enums;
using CusomMapOSM_Domain.Entities.Timeline;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.StoryMaps;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.StoryMaps;

public class StoryMapService : IStoryMapService
{
    private readonly IStoryMapRepository _repository;
    private readonly ICurrentUserService  _currentUserService;
    private readonly ISegmentLocationStore _locationStore;

    public StoryMapService(IStoryMapRepository repository, ICurrentUserService currentUserService, ISegmentLocationStore locationStore)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _locationStore = locationStore;
    }

    public async Task<Option<IReadOnlyCollection<SegmentDto>, Error>> GetSegmentsAsync(Guid mapId, CancellationToken ct = default)
    {
        var map = await _repository.GetMapAsync(mapId, ct);
        if (map is null)
        {
            return Option.None<IReadOnlyCollection<SegmentDto>, Error>(Error.NotFound("StoryMap.Map.NotFound", MapErrors.MapNotFound));
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
            var storyElementLayersRaw = await _repository.GetStoryElementLayersByElementAsync(segment.SegmentId, ct);
            var layersRaw = storyElementLayersRaw.Where(sel => sel.ElementType == CusomMapOSM_Domain.Entities.StoryElement.Enums.StoryElementType.Segment).ToList();
            var locationsRaw = await _locationStore.GetBySegmentAsync(segment.SegmentId, ct);

            var zones = zonesRaw.Select(z => z.ToSegmentZoneDto()).ToList();

            var layers = layersRaw.Select(l => l.ToSegmentLayerDto()).ToList();

            var locations = locationsRaw.Select(loc => loc.ToPoiDto()).ToList();

            var dto = segment.ToSegmentDto(zones, layers, locations);
            segmentDtos.Add(dto);
        }

        return Option.Some<IReadOnlyCollection<SegmentDto>, Error>(segmentDtos);
    }

    public async Task<Option<SegmentDto, Error>> CreateSegmentAsync(CreateSegmentRequest request, CancellationToken ct = default)
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

        return Option.Some<SegmentDto, Error>(segment.ToSegmentDto());
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
        var storyElementLayersRaw = await _repository.GetStoryElementLayersByElementAsync(segmentId, ct);
        var layers = storyElementLayersRaw.Where(sel => sel.ElementType == CusomMapOSM_Domain.Entities.StoryElement.Enums.StoryElementType.Segment).ToList();
        var locations = await _locationStore.GetBySegmentAsync(segmentId, ct);

        var zoneDtos = zones.Select(z => z.ToSegmentZoneDto()).ToList();

        var layerDtos = layers.Select(l => l.ToSegmentLayerDto()).ToList();

        var poiDtos = locations.Select(loc => loc.ToPoiDto()).ToList();

        return Option.Some<SegmentDto, Error>(segment.ToSegmentDto(zoneDtos, layerDtos, poiDtos));
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
                var c = new UpsertSegmentLayerRequest(l.LayerId, l.ZoneId, l.ExpandToZone, l.HighlightZoneBoundary, l.DisplayOrder, l.DelayMs, l.FadeInMs, l.FadeOutMs, (double)l.StartOpacity, (double)l.EndOpacity, l.Easing, l.AnimationPresetId, l.AutoPlayAnimation, l.RepeatCount, l.AnimationOverrides, l.StyleOverride, l.Metadata);
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
        var zoneDtos = zones.Select(z => z.ToSegmentZoneDto()).ToList();
        return Option.Some<IReadOnlyCollection<SegmentZoneDto>, Error>(zoneDtos);
    }

    public async Task<Option<SegmentZoneDto, Error>> CreateSegmentZoneAsync(CreateSegmentZoneRequest request, CancellationToken ct = default)
    {
        var segment = await _repository.GetSegmentAsync(request.SegmentId, ct);
        if (segment is null)
        {
            return Option.None<SegmentZoneDto, Error>(Error.NotFound("StoryMap.Segment.NotFound", "Segment not found"));
        }

        var zone = new CusomMapOSM_Domain.Entities.Zones.Zone
        {
            ZoneId = Guid.NewGuid(),
            SegmentId = request.SegmentId,
            Name = request.Name,
            Description = request.Description,
            ZoneType = request.ZoneType,
            Geometry = request.ZoneGeometry,
            FocusCameraState = request.FocusCameraState,
            DisplayOrder = request.DisplayOrder,
            IsPrimary = request.IsPrimary,
            CreatedAt = DateTime.UtcNow,
            // Set required fields for Zone entity
            ExternalId = string.Empty,
            ZoneCode = string.Empty,
            AdminLevel = CusomMapOSM_Domain.Entities.Zones.Enums.ZoneAdminLevel.Custom,
            IsActive = true,
            LastSyncedAt = DateTime.UtcNow
        };

        await _repository.AddSegmentZoneAsync(zone, ct);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<SegmentZoneDto, Error>(zone.ToSegmentZoneDto());
    }

    public async Task<Option<SegmentZoneDto, Error>> UpdateSegmentZoneAsync(Guid ZoneId, UpdateSegmentZoneRequest request, CancellationToken ct = default)
    {
        var zone = await _repository.GetSegmentZoneAsync(ZoneId, ct);
        if (zone is null)
        {
            return Option.None<SegmentZoneDto, Error>(Error.NotFound("StoryMap.Zone.NotFound", "Segment zone not found"));
        }

        zone.Name = request.Name;
        zone.Description = request.Description;
        zone.ZoneType = request.ZoneType;
        zone.Geometry = request.ZoneGeometry;
        zone.FocusCameraState = request.FocusCameraState;
        zone.DisplayOrder = request.DisplayOrder;
        zone.IsPrimary = request.IsPrimary;
        zone.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateSegmentZone(zone);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<SegmentZoneDto, Error>(zone.ToSegmentZoneDto());
    }

    public async Task<Option<bool, Error>> DeleteSegmentZoneAsync(Guid ZoneId, CancellationToken ct = default)
    {
        var zone = await _repository.GetSegmentZoneAsync(ZoneId, ct);
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

        var storyElementLayersRaw = await _repository.GetStoryElementLayersByElementAsync(segmentId, ct);
        var layers = storyElementLayersRaw.Where(sel => sel.ElementType == CusomMapOSM_Domain.Entities.StoryElement.Enums.StoryElementType.Segment).ToList();
        var layerDtos = layers.Select(l => new SegmentLayerDto(
            l.StoryElementLayerId,
            l.ElementId,
            l.LayerId,
            l.ZoneId,
            l.ExpandToZone,
            l.HighlightZoneBoundary,
            l.DisplayOrder,
            l.DelayMs,
            l.FadeInMs,
            l.FadeOutMs,
            l.StartOpacity ,
            l.EndOpacity ,
            l.Easing,
            l.AnimationPresetId,
            l.AutoPlayAnimation,
            l.RepeatCount,
            l.AnimationOverrides,
            l.StyleOverride,
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

        if (request.ZoneId.HasValue)
        {
            var zone = await _repository.GetSegmentZoneAsync(request.ZoneId.Value, ct);
            if (zone is null)
            {
                return Option.None<SegmentLayerDto, Error>(Error.NotFound("StoryMap.Zone.NotFound", "Segment zone not found"));
            }
        }

        var layer = new StoryElementLayer
        {
            StoryElementLayerId = Guid.NewGuid(),
            ElementId = segmentId,
            ElementType = StoryElementType.Segment,
            LayerId = request.LayerId,
            ZoneId = request.ZoneId,
            ExpandToZone = request.ExpandToZone,
            HighlightZoneBoundary = request.HighlightZoneBoundary,
            DisplayOrder = request.DisplayOrder,
            DelayMs = request.DelayMs,
            FadeInMs = request.FadeInMs,
            FadeOutMs = request.FadeOutMs,
            StartOpacity = (decimal)request.StartOpacity,
            EndOpacity = (decimal)request.EndOpacity,
            Easing = request.Easing,
            AnimationPresetId = request.AnimationPresetId,
            AutoPlayAnimation = request.AutoPlayAnimation,
            RepeatCount = request.RepeatCount,
            AnimationOverrides = request.AnimationOverrides,
            Metadata = request.Metadata,
            IsVisible = true,
            Opacity = 1.0m,
            DisplayMode = StoryElementDisplayMode.Normal,
            StyleOverride = request.StyleOverride,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddStoryElementLayerAsync(layer, ct);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<SegmentLayerDto, Error>(layer.ToSegmentLayerDto());
    }

    public async Task<Option<SegmentLayerDto, Error>> UpdateSegmentLayerAsync(Guid segmentLayerId, UpsertSegmentLayerRequest request, CancellationToken ct = default)
    {
        var layer = await _repository.GetStoryElementLayerAsync(segmentLayerId, ct);
        if (layer is null)
        {
            return Option.None<SegmentLayerDto, Error>(Error.NotFound("StoryMap.Layer.NotFound", "Segment layer not found"));
        }

        if (request.ZoneId.HasValue)
        {
            var zone = await _repository.GetSegmentZoneAsync(request.ZoneId.Value, ct);
            if (zone is null)
            {
                return Option.None<SegmentLayerDto, Error>(Error.NotFound("StoryMap.Zone.NotFound", "Segment zone not found"));
            }
        }

        layer.LayerId = request.LayerId;
        layer.ZoneId = request.ZoneId;
        layer.ExpandToZone = request.ExpandToZone;
        layer.HighlightZoneBoundary = request.HighlightZoneBoundary;
        layer.DisplayOrder = request.DisplayOrder;
        layer.DelayMs = request.DelayMs;
        layer.FadeInMs = request.FadeInMs;
        layer.FadeOutMs = request.FadeOutMs;
        layer.StartOpacity = (decimal)request.StartOpacity;
        layer.EndOpacity = (decimal)request.EndOpacity;
        layer.Easing = request.Easing;
        layer.AnimationPresetId = request.AnimationPresetId;
        layer.AutoPlayAnimation = request.AutoPlayAnimation;
        layer.RepeatCount = request.RepeatCount;
        layer.AnimationOverrides = request.AnimationOverrides;
        layer.StyleOverride = request.StyleOverride;
        layer.Metadata = request.Metadata;
        layer.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateStoryElementLayer(layer);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<SegmentLayerDto, Error>(layer.ToSegmentLayerDto());
    }

    public async Task<Option<bool, Error>> DeleteSegmentLayerAsync(Guid segmentLayerId, CancellationToken ct = default)
    {
        var layer = await _repository.GetStoryElementLayerAsync(segmentLayerId, ct);
        if (layer is null)
        {
            return Option.None<bool, Error>(Error.NotFound("StoryMap.Layer.NotFound", "Segment layer not found"));
        }

        _repository.RemoveStoryElementLayer(layer);
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
            var storyElementLayers = await _repository.GetStoryElementLayersByElementAsync(step.TimelineStepId, ct);
            var timelineLayersOnly = storyElementLayers.Where(sel => sel.ElementType == CusomMapOSM_Domain.Entities.StoryElement.Enums.StoryElementType.TimelineStep).ToList();
            
            var layerDtos = timelineLayersOnly.Select(sel => new TimelineStepLayerDto(
                sel.StoryElementLayerId,
                step.TimelineStepId,
                sel.LayerId,
                sel.IsVisible,
                (double)sel.Opacity,
                sel.FadeInMs,
                sel.FadeOutMs,
                sel.DelayMs,
                (CusomMapOSM_Domain.Entities.Timeline.Enums.TimelineLayerDisplayMode)(int)sel.DisplayMode,
                sel.StyleOverride,
                sel.Metadata)).ToList();

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

        var storyElementLayers = request.Layers?.Select(l => new CusomMapOSM_Domain.Entities.StoryElement.StoryElementLayer
        {
            StoryElementLayerId = Guid.NewGuid(),
            ElementId = step.TimelineStepId,
            ElementType = CusomMapOSM_Domain.Entities.StoryElement.Enums.StoryElementType.TimelineStep,
            LayerId = l.LayerId,
            IsVisible = l.IsVisible,
            Opacity = l.Opacity,
            FadeInMs = l.FadeInMs,
            FadeOutMs = l.FadeOutMs,
            DelayMs = l.DelayMs,
            DisplayMode = (CusomMapOSM_Domain.Entities.StoryElement.Enums.StoryElementDisplayMode)l.DisplayMode,
            StyleOverride = l.StyleOverride,
            Metadata = l.Metadata,
            CreatedAt = DateTime.UtcNow
        }).ToList() ?? new List<CusomMapOSM_Domain.Entities.StoryElement.StoryElementLayer>();

        if (storyElementLayers.Count > 0)
        {
            foreach (var layer in storyElementLayers)
            {
                await _repository.AddStoryElementLayerAsync(layer, ct);
            }
        }

        await _repository.SaveChangesAsync(ct);

        var layerDtos = storyElementLayers.Select(sel => new TimelineStepLayerDto(
            sel.StoryElementLayerId,
            step.TimelineStepId,
            sel.LayerId,
            sel.IsVisible,
            (double)sel.Opacity,
            sel.FadeInMs,
            sel.FadeOutMs,
            sel.DelayMs,
            (CusomMapOSM_Domain.Entities.Timeline.Enums.TimelineLayerDisplayMode)(int)sel.DisplayMode,
            sel.StyleOverride,
            sel.Metadata)).ToList();

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

        var existingLayers = await _repository.GetStoryElementLayersByElementAsync(timelineStepId, ct);
        var timelineStepLayers = existingLayers.Where(sel => sel.ElementType == CusomMapOSM_Domain.Entities.StoryElement.Enums.StoryElementType.TimelineStep).ToList();
        
        foreach (var layer in timelineStepLayers)
        {
            _repository.RemoveStoryElementLayer(layer);
        }

        var newStoryElementLayers = request.Layers?.Select(l => new CusomMapOSM_Domain.Entities.StoryElement.StoryElementLayer
        {
            StoryElementLayerId = Guid.NewGuid(),
            ElementId = timelineStepId,
            ElementType = CusomMapOSM_Domain.Entities.StoryElement.Enums.StoryElementType.TimelineStep,
            LayerId = l.LayerId,
            IsVisible = l.IsVisible,
            Opacity = l.Opacity,
            FadeInMs = l.FadeInMs,
            FadeOutMs = l.FadeOutMs,
            DelayMs = l.DelayMs,
            DisplayMode = (CusomMapOSM_Domain.Entities.StoryElement.Enums.StoryElementDisplayMode)l.DisplayMode,
            StyleOverride = l.StyleOverride,
            Metadata = l.Metadata,
            CreatedAt = DateTime.UtcNow
        }).ToList() ?? new List<CusomMapOSM_Domain.Entities.StoryElement.StoryElementLayer>();

        if (newStoryElementLayers.Count > 0)
        {
            foreach (var layer in newStoryElementLayers)
            {
                await _repository.AddStoryElementLayerAsync(layer, ct);
            }
        }

        await _repository.SaveChangesAsync(ct);

        var layerDtos = newStoryElementLayers.Select(sel => new TimelineStepLayerDto(
            sel.StoryElementLayerId,
            timelineStepId,
            sel.LayerId,
            sel.IsVisible,
            (double)sel.Opacity,
            sel.FadeInMs,
            sel.FadeOutMs,
            sel.DelayMs,
            (CusomMapOSM_Domain.Entities.Timeline.Enums.TimelineLayerDisplayMode)(int)sel.DisplayMode,
            sel.StyleOverride,
            sel.Metadata)).ToList();

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

    public async Task<Option<IReadOnlyCollection<StoryElementLayerDto>, Error>> GetStoryElementLayersAsync(Guid elementId, CancellationToken ct = default)
    {
        var layers = await _repository.GetStoryElementLayersByElementAsync(elementId, ct);
        var layerDtos = layers.Select(sel => sel.ToStoryElementLayerDto()).ToList();
        return Option.Some<IReadOnlyCollection<StoryElementLayerDto>, Error>(layerDtos);
    }

    public async Task<Option<StoryElementLayerDto, Error>> CreateStoryElementLayerAsync(CreateStoryElementLayerRequest request, CancellationToken ct = default)
    {
        var layer = new StoryElementLayer
        {
            StoryElementLayerId = Guid.NewGuid(),
            ElementId = request.ElementId,
            ElementType = request.ElementType,
            LayerId = request.LayerId,
            ZoneId = request.ZoneId,
            ExpandToZone = request.ExpandToZone,
            HighlightZoneBoundary = request.HighlightZoneBoundary,
            DisplayOrder = request.DisplayOrder,
            DelayMs = request.DelayMs,
            FadeInMs = request.FadeInMs,
            FadeOutMs = request.FadeOutMs,
            StartOpacity = (decimal)request.StartOpacity,
            EndOpacity = (decimal)request.EndOpacity,
            Easing = request.Easing,
            AnimationPresetId = request.AnimationPresetId,
            AutoPlayAnimation = request.AutoPlayAnimation,
            RepeatCount = request.RepeatCount,
            AnimationOverrides = request.AnimationOverrides,
            Metadata = request.Metadata,
            IsVisible = request.IsVisible,
            Opacity = request.Opacity,
            DisplayMode = request.DisplayMode,
            StyleOverride = request.StyleOverride,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddStoryElementLayerAsync(layer, ct);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<StoryElementLayerDto, Error>(layer.ToStoryElementLayerDto());
    }

    public async Task<Option<StoryElementLayerDto, Error>> UpdateStoryElementLayerAsync(Guid storyElementLayerId, UpdateStoryElementLayerRequest request, CancellationToken ct = default)
    {
        var layer = await _repository.GetStoryElementLayerAsync(storyElementLayerId, ct);
        if (layer is null)
        {
            return Option.None<StoryElementLayerDto, Error>(Error.NotFound("StoryElement.Layer.NotFound", "Story element layer not found"));
        }

        layer.ElementType = request.ElementType;
        layer.LayerId = request.LayerId;
        layer.ZoneId = request.ZoneId;
        layer.ExpandToZone = request.ExpandToZone;
        layer.HighlightZoneBoundary = request.HighlightZoneBoundary;
        layer.DisplayOrder = request.DisplayOrder;
        layer.DelayMs = request.DelayMs;
        layer.FadeInMs = request.FadeInMs;
        layer.FadeOutMs = request.FadeOutMs;
        layer.StartOpacity = (decimal)request.StartOpacity;
        layer.EndOpacity = (decimal)request.EndOpacity;
        layer.Easing = request.Easing;
        layer.AnimationPresetId = request.AnimationPresetId;
        layer.AutoPlayAnimation = request.AutoPlayAnimation;
        layer.RepeatCount = request.RepeatCount;
        layer.AnimationOverrides = request.AnimationOverrides;
        layer.Metadata = request.Metadata;
        layer.IsVisible = request.IsVisible;
        layer.Opacity = request.Opacity;
        layer.DisplayMode = request.DisplayMode;
        layer.StyleOverride = request.StyleOverride;
        layer.UpdatedAt = DateTime.UtcNow;

        _repository.UpdateStoryElementLayer(layer);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<StoryElementLayerDto, Error>(layer.ToStoryElementLayerDto());
    }

    public async Task<Option<bool, Error>> DeleteStoryElementLayerAsync(Guid storyElementLayerId, CancellationToken ct = default)
    {
        var layer = await _repository.GetStoryElementLayerAsync(storyElementLayerId, ct);
        if (layer is null)
        {
            return Option.None<bool, Error>(Error.NotFound("StoryElement.Layer.NotFound", "Story element layer not found"));
        }

        _repository.RemoveStoryElementLayer(layer);
        await _repository.SaveChangesAsync(ct);

        return Option.Some<bool, Error>(true);
    }
}

