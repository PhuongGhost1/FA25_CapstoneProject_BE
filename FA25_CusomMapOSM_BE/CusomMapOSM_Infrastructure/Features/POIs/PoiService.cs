using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.POIs;
using CusomMapOSM_Application.Interfaces.Services.StoryMaps;
using CusomMapOSM_Application.Models.DTOs.Features.POIs;
using CusomMapOSM_Domain.Entities.Locations;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.StoryMaps;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.POIs;

public class PoiService : IPoiService
{
    private readonly IStoryMapRepository _storyMapRepository;
    private readonly ISegmentLocationStore _locationStore;

    public PoiService(IStoryMapRepository storyMapRepository, ISegmentLocationStore locationStore)
    {
        _storyMapRepository = storyMapRepository;
        _locationStore = locationStore;
    }

    public async Task<Option<IReadOnlyCollection<PoiDto>, Error>> GetMapPoisAsync(Guid mapId, CancellationToken ct = default)
    {
        var map = await _storyMapRepository.GetMapAsync(mapId, ct);
        if (map is null)
        {
            return Option.None<IReadOnlyCollection<PoiDto>, Error>(Error.NotFound("Poi.Map.NotFound", "Map not found"));
        }

        var locations = await _locationStore.GetByMapAsync(mapId, ct);
        var poiDtos = locations.Select(l => new PoiDto(
            l.LocationId,
            l.MapId,
            l.SegmentId,
            l.ZoneId,
            l.Title,
            l.Subtitle,
            l.LocationType,
            l.MarkerGeometry,
            l.StoryContent,
            l.MediaResources,
            l.DisplayOrder,
            l.HighlightOnEnter,
            l.ShowTooltip,
            l.TooltipContent,
            l.EffectType,
            l.OpenSlideOnClick,
            l.SlideContent,
            l.LinkedLocationId,
            l.PlayAudioOnClick,
            l.AudioUrl,
            l.ExternalUrl,
            l.AssociatedLayerId,
            l.AnimationPresetId,
            l.AnimationOverrides,
            l.CreatedAt,
            l.UpdatedAt)).ToList();
        return Option.Some<IReadOnlyCollection<PoiDto>, Error>(poiDtos);
    }

    public async Task<Option<IReadOnlyCollection<PoiDto>, Error>> GetSegmentPoisAsync(Guid mapId, Guid segmentId, CancellationToken ct = default)
    {
        var segment = await _storyMapRepository.GetSegmentAsync(segmentId, ct);
        if (segment is null || segment.MapId != mapId)
        {
            return Option.None<IReadOnlyCollection<PoiDto>, Error>(Error.NotFound("Poi.Segment.NotFound", "Segment not found"));
        }

        var locations = await _locationStore.GetBySegmentAsync(segmentId, ct);
        var poiDtos = locations.Select(l => new PoiDto(
            l.LocationId,
            l.MapId,
            l.SegmentId,
            l.ZoneId,
            l.Title,
            l.Subtitle,
            l.LocationType,
            l.MarkerGeometry,
            l.StoryContent,
            l.MediaResources,
            l.DisplayOrder,
            l.HighlightOnEnter,
            l.ShowTooltip,
            l.TooltipContent,
            l.EffectType,
            l.OpenSlideOnClick,
            l.SlideContent,
            l.LinkedLocationId,
            l.PlayAudioOnClick,
            l.AudioUrl,
            l.ExternalUrl,
            l.AssociatedLayerId,
            l.AnimationPresetId,
            l.AnimationOverrides,
            l.CreatedAt,
            l.UpdatedAt)).ToList();
        return Option.Some<IReadOnlyCollection<PoiDto>, Error>(poiDtos);
    }

    public async Task<Option<PoiDto, Error>> CreatePoiAsync(CreatePoiRequest request, CancellationToken ct = default)
    {
        var map = await _storyMapRepository.GetMapAsync(request.MapId, ct);
        if (map is null)
        {
            return Option.None<PoiDto, Error>(Error.NotFound("Poi.Map.NotFound", "Map not found"));
        }

        Guid? segmentId = NormalizeGuid(request.SegmentId);
        Guid? zoneId = NormalizeGuid(request.ZoneId);
        Guid? linkedPoiId = NormalizeGuid(request.LinkedPoiId);

        if (segmentId.HasValue)
        {
            var segment = await _storyMapRepository.GetSegmentAsync(segmentId.Value, ct);
            if (segment is null || segment.MapId != request.MapId)
            {
                return Option.None<PoiDto, Error>(Error.NotFound("Poi.Segment.NotFound", "Segment not found for this map"));
            }
        }

        if (zoneId.HasValue)
        {
            var zone = await _storyMapRepository.GetSegmentZoneAsync(zoneId.Value, ct);
            if (zone is null)
            {
                return Option.None<PoiDto, Error>(Error.NotFound("Poi.Zone.NotFound", "Zone not found"));
            }

            if (zone.SegmentId.HasValue)
            {
                var zoneSegment = await _storyMapRepository.GetSegmentAsync(zone.SegmentId.Value, ct);
                if (zoneSegment is null || zoneSegment.MapId != request.MapId)
                {
                    return Option.None<PoiDto, Error>(Error.NotFound("Poi.Segment.NotFound", "Segment not found for this map"));
                }

                if (segmentId.HasValue && segmentId.Value != zone.SegmentId.Value)
                {
                    return Option.None<PoiDto, Error>(Error.ValidationError("Poi.Zone.Invalid", "Zone does not belong to the provided segment"));
                }

                segmentId ??= zone.SegmentId;
            }
        }

        if (linkedPoiId.HasValue)
        {
            var linked = await _locationStore.GetAsync(linkedPoiId.Value, ct);
            if (linked is null || linked.MapId != request.MapId)
            {
                return Option.None<PoiDto, Error>(Error.NotFound("Poi.Linked.NotFound", "Linked POI not found in this map"));
            }
        }

        var location = new Location
        {
            LocationId = Guid.NewGuid(),
            MapId = request.MapId,
            SegmentId = segmentId,
            ZoneId = zoneId,
            Title = request.Title,
            Subtitle = request.Subtitle,
            LocationType = request.LocationType,
            MarkerGeometry = request.MarkerGeometry,
            StoryContent = request.StoryContent,
            MediaResources = request.MediaResources,
            DisplayOrder = request.DisplayOrder,
            HighlightOnEnter = request.HighlightOnEnter,
            ShowTooltip = request.ShowTooltip,
            TooltipContent = request.TooltipContent,
            EffectType = request.EffectType,
            OpenSlideOnClick = request.OpenSlideOnClick,
            SlideContent = request.SlideContent,
            LinkedLocationId = linkedPoiId,
            PlayAudioOnClick = request.PlayAudioOnClick,
            AudioUrl = request.AudioUrl,
            ExternalUrl = request.ExternalUrl,
            AssociatedLayerId = request.AssociatedLayerId,
            AnimationPresetId = request.AnimationPresetId,
            AnimationOverrides = request.AnimationOverrides,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        location = await _locationStore.CreateAsync(location, ct);

        var poiDto = new PoiDto(
            location.LocationId,
            location.MapId,
            location.SegmentId,
            location.ZoneId,
            location.Title,
            location.Subtitle,
            location.LocationType,
            location.MarkerGeometry,
            location.StoryContent,
            location.MediaResources,
            location.DisplayOrder,
            location.HighlightOnEnter,
            location.ShowTooltip,
            location.TooltipContent,
            location.EffectType,
            location.OpenSlideOnClick,
            location.SlideContent,
            location.LinkedLocationId,
            location.PlayAudioOnClick,
            location.AudioUrl,
            location.ExternalUrl,
            location.AssociatedLayerId,
            location.AnimationPresetId,
            location.AnimationOverrides,
            location.CreatedAt,
            location.UpdatedAt);

        return Option.Some<PoiDto, Error>(poiDto);
    }

    public async Task<Option<PoiDto, Error>> UpdatePoiAsync(Guid poiId, UpdatePoiRequest request, CancellationToken ct = default)
    {
        var location = await _locationStore.GetAsync(poiId, ct);
        if (location is null)
        {
            return Option.None<PoiDto, Error>(Error.NotFound("Poi.NotFound", "Point of interest not found"));
        }

        var mapId = location.MapId;

        Guid? segmentId = request.SegmentId.HasValue ? NormalizeGuid(request.SegmentId) : location.SegmentId;
        if (segmentId.HasValue)
        {
            var segment = await _storyMapRepository.GetSegmentAsync(segmentId.Value, ct);
            if (segment is null || segment.MapId != mapId)
            {
                return Option.None<PoiDto, Error>(Error.NotFound("Poi.Segment.NotFound", "Segment not found for this map"));
            }
        }

        Guid? zoneId = request.ZoneId.HasValue ? NormalizeGuid(request.ZoneId) : location.ZoneId;
        if (zoneId.HasValue)
        {
            var zone = await _storyMapRepository.GetSegmentZoneAsync(zoneId.Value, ct);
            if (zone is null)
            {
                return Option.None<PoiDto, Error>(Error.NotFound("Poi.Zone.NotFound", "Zone not found"));
            }

            if (segmentId.HasValue && zone.SegmentId.HasValue && zone.SegmentId.Value != segmentId.Value)
            {
                return Option.None<PoiDto, Error>(Error.ValidationError("Poi.Zone.Invalid", "Zone does not belong to the provided segment"));
            }

            segmentId ??= zone.SegmentId;
        }

        Guid? linkedPoiId = request.LinkedPoiId.HasValue ? NormalizeGuid(request.LinkedPoiId) : location.LinkedLocationId;
        if (linkedPoiId.HasValue)
        {
            if (linkedPoiId.Value == poiId)
            {
                return Option.None<PoiDto, Error>(Error.ValidationError("Poi.Linked.Self", "A POI cannot link to itself"));
            }

            var linked = await _locationStore.GetAsync(linkedPoiId.Value, ct);
            if (linked is null || linked.MapId != mapId)
            {
                return Option.None<PoiDto, Error>(Error.NotFound("Poi.Linked.NotFound", "Linked POI not found in this map"));
            }
        }

        location.SegmentId = segmentId;
        location.ZoneId = zoneId;
        location.Title = request.Title;
        location.Subtitle = request.Subtitle;
        location.LocationType = request.LocationType;
        location.MarkerGeometry = request.MarkerGeometry;
        location.StoryContent = request.StoryContent;
        location.MediaResources = request.MediaResources;
        location.DisplayOrder = request.DisplayOrder;
        location.HighlightOnEnter = request.HighlightOnEnter;
        location.ShowTooltip = request.ShowTooltip;
        location.TooltipContent = request.TooltipContent;
        location.EffectType = request.EffectType;
        location.OpenSlideOnClick = request.OpenSlideOnClick;
        location.SlideContent = request.SlideContent;
        location.LinkedLocationId = linkedPoiId;
        location.PlayAudioOnClick = request.PlayAudioOnClick;
        location.AudioUrl = request.AudioUrl;
        location.ExternalUrl = request.ExternalUrl;
        location.AssociatedLayerId = request.AssociatedLayerId;
        location.AnimationPresetId = request.AnimationPresetId;
        location.AnimationOverrides = request.AnimationOverrides;
        location.UpdatedAt = DateTime.UtcNow;

        var updated = await _locationStore.UpdateAsync(location, ct);
        if (updated is not null)
        {
            location = updated;
        }

        var poiDto = new PoiDto(
            location.LocationId,
            location.MapId,
            location.SegmentId,
            location.ZoneId,
            location.Title,
            location.Subtitle,
            location.LocationType,
            location.MarkerGeometry,
            location.StoryContent,
            location.MediaResources,
            location.DisplayOrder,
            location.HighlightOnEnter,
            location.ShowTooltip,
            location.TooltipContent,
            location.EffectType,
            location.OpenSlideOnClick,
            location.SlideContent,
            location.LinkedLocationId,
            location.PlayAudioOnClick,
            location.AudioUrl,
            location.ExternalUrl,
            location.AssociatedLayerId,
            location.AnimationPresetId,
            location.AnimationOverrides,
            location.CreatedAt,
            location.UpdatedAt);

        return Option.Some<PoiDto, Error>(poiDto);
    }

    public async Task<Option<bool, Error>> DeletePoiAsync(Guid poiId, CancellationToken ct = default)
    {
        var location = await _locationStore.GetAsync(poiId, ct);
        if (location is null)
        {
            return Option.None<bool, Error>(Error.NotFound("Poi.NotFound", "Point of interest not found"));
        }

        await _locationStore.DeleteAsync(poiId, ct);

        return Option.Some<bool, Error>(true);
    }

    private static Guid? NormalizeGuid(Guid? value) =>
        value.HasValue && value.Value == Guid.Empty ? null : value;
}
