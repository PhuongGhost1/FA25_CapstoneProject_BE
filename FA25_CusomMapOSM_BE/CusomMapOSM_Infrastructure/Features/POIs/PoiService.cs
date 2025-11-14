using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.POIs;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.POIs;
using CusomMapOSM_Application.Common.Mappers;
using CusomMapOSM_Domain.Entities.Locations;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.StoryMaps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Locations;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.POIs;

public class PoiService : IPoiService
{
    private readonly IStoryMapRepository _storyMapRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly ICurrentUserService _currentUserService;

    public PoiService(IStoryMapRepository storyMapRepository, ILocationRepository locationRepository,
        ICurrentUserService currentUserService)
    {
        _storyMapRepository = storyMapRepository;
        _locationRepository = locationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Option<IReadOnlyCollection<PoiDto>, Error>> GetMapPoisAsync(Guid mapId,
        CancellationToken ct = default)
    {
        var map = await _storyMapRepository.GetMapAsync(mapId, ct);
        if (map is null)
        {
            return Option.None<IReadOnlyCollection<PoiDto>, Error>(Error.NotFound("Poi.Map.NotFound", "Map not found"));
        }

        var locations = await _locationRepository.GetByMapIdAsync(mapId, ct);
        var poiDtos = locations.Select(l => l.ToPoiDto()).ToList();
        return Option.Some<IReadOnlyCollection<PoiDto>, Error>(poiDtos);
    }

    public async Task<Option<IReadOnlyCollection<PoiDto>, Error>> GetSegmentPoisAsync(Guid mapId, Guid segmentId,
        CancellationToken ct = default)
    {
        var segment = await _storyMapRepository.GetSegmentAsync(segmentId, ct);
        if (segment is null || segment.MapId != mapId)
        {
            return Option.None<IReadOnlyCollection<PoiDto>, Error>(Error.NotFound("Poi.Segment.NotFound",
                "Segment not found"));
        }

        var locations = await _locationRepository.GetBySegmentIdAsync(segmentId, ct);
        var poiDtos = locations.Select(l => l.ToPoiDto()).ToList();
        return Option.Some<IReadOnlyCollection<PoiDto>, Error>(poiDtos);
    }

    public async Task<Option<IReadOnlyCollection<PoiDto>, Error>> GetZonePoisAsync(Guid zoneId,
        CancellationToken ct = default)
    {
        var zone = await _storyMapRepository.GetZoneAsync(zoneId, ct);
        if (zone is null)
        {
            return Option.None<IReadOnlyCollection<PoiDto>, Error>(Error.NotFound("Poi.Zone.NotFound",
                "Zone not found"));
        }

        var locations = await _locationRepository.GetByZoneIdAsync(zoneId, ct);
        var poiDtos = locations.Select(l => l.ToPoiDto()).ToList();
        return Option.Some<IReadOnlyCollection<PoiDto>, Error>(poiDtos);
    }

    public async Task<Option<IReadOnlyCollection<PoiDto>, Error>> GetPoisWithoutZoneAsync(Guid segmentId,
        CancellationToken ct = default)
    {
        var segment = await _storyMapRepository.GetSegmentAsync(segmentId, ct);
        if (segment is null)
        {
            return Option.None<IReadOnlyCollection<PoiDto>, Error>(Error.NotFound("Poi.Segment.NotFound",
                "Segment not found"));
        }

        var locations = await _locationRepository.GetWithoutZoneAsync(segmentId, ct);
        var poiDtos = locations.Select(l => l.ToPoiDto()).ToList();
        return Option.Some<IReadOnlyCollection<PoiDto>, Error>(poiDtos);
    }

    public async Task<Option<PoiDto, Error>> CreatePoiAsync(CreatePoiRequest request, CancellationToken ct = default)
    {
        var currentUserId = _currentUserService.GetUserId();

        if (!currentUserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var map = await _storyMapRepository.GetMapAsync(request.MapId, ct);
        if (map is null)
        {
            return Option.None<PoiDto, Error>(Error.NotFound("Poi.Map.NotFound", "Map not found"));
        }

        Guid? segmentId = NormalizeGuid(request.SegmentId);
        Guid? zoneId = NormalizeGuid(request.ZoneId);
        Guid? linkedPoiId = NormalizeGuid(request.LinkedPoiId);
        
        // Validate SegmentId if provided
        if (segmentId.HasValue)
        {
            var segment = await _storyMapRepository.GetSegmentAsync(segmentId.Value, ct);
            if (segment is null || segment.MapId != request.MapId)
            {
                return Option.None<PoiDto, Error>(Error.NotFound("Poi.Segment.NotFound", "Segment not found for this map"));
            }
        }
        
        // Validate ZoneId if provided
        if (zoneId.HasValue)
        {
            var zone = await _storyMapRepository.GetZoneAsync(zoneId.Value, ct);
            if (zone is null)
            {
                return Option.None<PoiDto, Error>(Error.NotFound("Poi.Zone.NotFound", "Zone not found"));
            }
        }

        if (linkedPoiId.HasValue)
        {
            var linked = await _locationRepository.GetByIdAsync(linkedPoiId.Value, ct);
            if (linked is null || linked.MapId != request.MapId)
            {
                return Option.None<PoiDto, Error>(Error.NotFound("Poi.Linked.NotFound",
                    "Linked POI not found in this map"));
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
            MarkerGeometry = request.MarkerGeometry ?? string.Empty,
            ShowTooltip = request.ShowTooltip,
            TooltipContent = request.TooltipContent,
            OpenPopupOnClick = request.OpenSlideOnClick,
            PopupContent = request.SlideContent,
            MediaUrls = request.MediaResources,
            LinkedLocationId = linkedPoiId,
            PlayAudioOnClick = request.PlayAudioOnClick,
            AudioUrl = request.AudioUrl,
            ExternalUrl = request.ExternalUrl,
            IsVisible = request.IsVisible,
            ZIndex = request.ZIndex,
            CreatedBy = currentUserId.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        location = await _locationRepository.CreateAsync(location, ct);

        var poiDto = location.ToPoiDto();

        return Option.Some<PoiDto, Error>(poiDto);
    }

    public async Task<Option<PoiDto, Error>> UpdatePoiAsync(Guid poiId, UpdatePoiRequest request,
        CancellationToken ct = default)
    {
        var location = await _locationRepository.GetByIdAsync(poiId, ct);
        if (location is null)
        {
            return Option.None<PoiDto, Error>(Error.NotFound("Poi.NotFound", "Point of interest not found"));
        }

        // Use MapId directly from location
        var mapId = location.MapId;

        Guid? segmentId = request.SegmentId.HasValue ? NormalizeGuid(request.SegmentId) : location.SegmentId;
        if (segmentId.HasValue)
        {
            var segment = await _storyMapRepository.GetSegmentAsync(segmentId.Value, ct);
            if (segment is null || segment.MapId != mapId)
            {
                return Option.None<PoiDto, Error>(Error.NotFound("Poi.Segment.NotFound",
                    "Segment not found for this map"));
            }
        }

        Guid? zoneId = request.ZoneId.HasValue ? NormalizeGuid(request.ZoneId) : location.ZoneId;
        if (zoneId.HasValue)
        {
            var zone = await _storyMapRepository.GetZoneAsync(zoneId.Value, ct);
            if (zone is null)
            {
                return Option.None<PoiDto, Error>(Error.NotFound("Poi.Zone.NotFound", "Zone not found"));
            }
        }

        Guid? linkedPoiId = request.LinkedPoiId.HasValue
            ? NormalizeGuid(request.LinkedPoiId)
            : location.LinkedLocationId;
        if (linkedPoiId.HasValue)
        {
            if (linkedPoiId.Value == poiId)
            {
                return Option.None<PoiDto, Error>(Error.ValidationError("Poi.Linked.Self",
                    "A POI cannot link to itself"));
            }

            var linked = await _locationRepository.GetByIdAsync(linkedPoiId.Value, ct);
            if (linked is null || linked.MapId != mapId)
            {
                return Option.None<PoiDto, Error>(Error.NotFound("Poi.Linked.NotFound",
                    "Linked POI not found in this map"));
            }
        }

        if (segmentId.HasValue)
        {
            location.SegmentId = segmentId.Value;
        }
        else
        {
            location.SegmentId = null;
        }

        location.ZoneId = zoneId;
        location.Title = request.Title;
        location.Subtitle = request.Subtitle;
        location.LocationType = request.LocationType;
        if (!string.IsNullOrEmpty(request.MarkerGeometry))
        {
            location.MarkerGeometry = request.MarkerGeometry;
        }

        location.ShowTooltip = request.ShowTooltip;
        location.TooltipContent = request.TooltipContent;
        location.OpenPopupOnClick = request.OpenSlideOnClick;
        location.PopupContent = request.SlideContent;
        location.MediaUrls = request.MediaResources;
        location.LinkedLocationId = linkedPoiId;
        location.PlayAudioOnClick = request.PlayAudioOnClick;
        location.AudioUrl = request.AudioUrl;
        location.ExternalUrl = request.ExternalUrl;
        location.DisplayOrder = request.DisplayOrder;

        if (request.IsVisible.HasValue)
        {
            location.IsVisible = request.IsVisible.Value;
        }

        if (request.ZIndex.HasValue)
        {
            location.ZIndex = request.ZIndex.Value;
        }

        location.UpdatedAt = DateTime.UtcNow;

        var updated = await _locationRepository.UpdateAsync(location, ct);
        if (updated is not null)
        {
            location = updated;
        }

        return Option.Some<PoiDto, Error>(location.ToPoiDto());
    }

    public async Task<Option<bool, Error>> DeletePoiAsync(Guid poiId, CancellationToken ct = default)
    {
        var location = await _locationRepository.GetByIdAsync(poiId, ct);
        if (location is null)
        {
            return Option.None<bool, Error>(Error.NotFound("Poi.NotFound", "Point of interest not found"));
        }

        await _locationRepository.DeleteAsync(poiId, ct);

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<PoiDto, Error>> UpdatePoiDisplayConfigAsync(Guid poiId,
        UpdatePoiDisplayConfigRequest request, CancellationToken ct = default)
    {
        var location = await _locationRepository.GetByIdAsync(poiId, ct);
        if (location is null)
        {
            return Option.None<PoiDto, Error>(Error.NotFound("Poi.NotFound", "Point of interest not found"));
        }

        if (request.IsVisible.HasValue)
        {
            location.IsVisible = request.IsVisible.Value;
        }

        if (request.ZIndex.HasValue)
        {
            location.ZIndex = request.ZIndex.Value;
        }

        if (request.ShowTooltip.HasValue)
        {
            location.ShowTooltip = request.ShowTooltip.Value;
        }

        if (request.TooltipContent is not null)
        {
            location.TooltipContent = request.TooltipContent;
        }

        location.UpdatedAt = DateTime.UtcNow;
        var updated = await _locationRepository.UpdateAsync(location, ct) ?? location;

        var dto = updated.ToPoiDto();

        return Option.Some<PoiDto, Error>(dto);
    }

    public async Task<Option<PoiDto, Error>> UpdatePoiInteractionConfigAsync(Guid poiId,
        UpdatePoiInteractionConfigRequest request, CancellationToken ct = default)
    {
        var location = await _locationRepository.GetByIdAsync(poiId, ct);
        if (location is null)
        {
            return Option.None<PoiDto, Error>(Error.NotFound("Poi.NotFound", "Point of interest not found"));
        }

        if (request.OpenSlideOnClick.HasValue)
        {
            location.OpenPopupOnClick = request.OpenSlideOnClick.Value;
        }

        if (request.PlayAudioOnClick.HasValue)
        {
            location.PlayAudioOnClick = request.PlayAudioOnClick.Value;
        }

        if (request.AudioUrl is not null)
        {
            location.AudioUrl = request.AudioUrl;
        }

        if (request.ExternalUrl is not null)
        {
            location.ExternalUrl = request.ExternalUrl;
        }

        location.UpdatedAt = DateTime.UtcNow;
        var updated = await _locationRepository.UpdateAsync(location, ct) ?? location;

        var dto = updated.ToPoiDto();

        return Option.Some<PoiDto, Error>(dto);
    }

    private static Guid? NormalizeGuid(Guid? value) =>
        value.HasValue && value.Value == Guid.Empty ? null : value;
}