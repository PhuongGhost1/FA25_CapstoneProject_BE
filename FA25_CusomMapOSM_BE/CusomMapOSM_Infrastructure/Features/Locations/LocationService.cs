using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Common.Mappers;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Interfaces.Services.Firebase;
using CusomMapOSM_Application.Interfaces.Features.Locations;
using CusomMapOSM_Application.Models.DTOs.Features.Locations;
using CusomMapOSM_Domain.Entities.Locations;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.StoryMaps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Locations;
using CusomMapOSM_Infrastructure.Services;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.Locations;

public class LocationService : ILocationService
{
    private readonly IStoryMapRepository _storyMapRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly HtmlContentImageProcessor _htmlImageProcessor;
    private readonly IFirebaseStorageService _firebaseStorageService;

    public LocationService(
        IStoryMapRepository storyMapRepository, 
        ILocationRepository locationRepository,
        ICurrentUserService currentUserService,
        HtmlContentImageProcessor htmlImageProcessor,
        IFirebaseStorageService firebaseStorageService)
    {
        _storyMapRepository = storyMapRepository;
        _locationRepository = locationRepository;
        _currentUserService = currentUserService;
        _htmlImageProcessor = htmlImageProcessor;
        _firebaseStorageService = firebaseStorageService;
    }

    public async Task<Option<IReadOnlyCollection<LocationDto>, Error>> GetMapLocations(Guid mapId,
        CancellationToken ct = default)
    {
        var map = await _storyMapRepository.GetMapAsync(mapId, ct);
        if (map is null)
        {
            return Option.None<IReadOnlyCollection<LocationDto>, Error>(Error.NotFound("Poi.Map.NotFound", "Map not found"));
        }

        var locations = await _locationRepository.GetByMapIdAsync(mapId, ct);
        var LocationDtos = locations.Select(l => l.ToDto()).ToList();
        return Option.Some<IReadOnlyCollection<LocationDto>, Error>(LocationDtos);
    }

    public async Task<Option<IReadOnlyCollection<LocationDto>, Error>> GetSegmentLocationsAsync(Guid mapId, Guid segmentId,
        CancellationToken ct = default)
    {
        var segment = await _storyMapRepository.GetSegmentAsync(segmentId, ct);
        if (segment is null || segment.MapId != mapId)
        {
            return Option.None<IReadOnlyCollection<LocationDto>, Error>(Error.NotFound("Poi.Segment.NotFound",
                "Segment not found"));
        }

        var locations = await _locationRepository.GetBySegmentIdAsync(segmentId, ct);
        var LocationDtos = locations.Select(l => l.ToDto()).ToList();
        return Option.Some<IReadOnlyCollection<LocationDto>, Error>(LocationDtos);
    }

    public async Task<Option<IReadOnlyCollection<LocationDto>, Error>> GetZoneLocationsAsync(Guid zoneId,
        CancellationToken ct = default)
    {
        var zone = await _storyMapRepository.GetZoneAsync(zoneId, ct);
        if (zone is null)
        {
            return Option.None<IReadOnlyCollection<LocationDto>, Error>(Error.NotFound("Poi.Zone.NotFound",
                "Zone not found"));
        }

        var locations = await _locationRepository.GetByZoneIdAsync(zoneId, ct);
        var LocationDtos = locations.Select(l => l.ToDto()).ToList();
        return Option.Some<IReadOnlyCollection<LocationDto>, Error>(LocationDtos);
    }

    public async Task<Option<IReadOnlyCollection<LocationDto>, Error>> GetLocationsWithoutZoneAsync(Guid segmentId,
        CancellationToken ct = default)
    {
        var segment = await _storyMapRepository.GetSegmentAsync(segmentId, ct);
        if (segment is null)
        {
            return Option.None<IReadOnlyCollection<LocationDto>, Error>(Error.NotFound("Poi.Segment.NotFound",
                "Segment not found"));
        }

        var locations = await _locationRepository.GetWithoutZoneAsync(segmentId, ct);
        var LocationDtos = locations.Select(l => l.ToDto()).ToList();
        return Option.Some<IReadOnlyCollection<LocationDto>, Error>(LocationDtos);
    }

    public async Task<Option<LocationDto, Error>> CreateLocationAsync(CreateLocationRequest request, CancellationToken ct = default)
    {
        var currentUserId = _currentUserService.GetUserId();

        if (!currentUserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var map = await _storyMapRepository.GetMapAsync(request.MapId, ct);
        if (map is null)
        {
            return Option.None<LocationDto, Error>(Error.NotFound("Poi.Map.NotFound", "Map not found"));
        }

        Guid? segmentId = (request.SegmentId);
        Guid? zoneId = (request.ZoneId);
        Guid? linkedPoiId = (request.LinkedLocationId);
        
        // Validate SegmentId if provided
        if (segmentId.HasValue)
        {
            var segment = await _storyMapRepository.GetSegmentAsync(segmentId.Value, ct);
            if (segment is null || segment.MapId != request.MapId)
            {
                return Option.None<LocationDto, Error>(Error.NotFound("Poi.Segment.NotFound", "Segment not found for this map"));
            }
        }
        
        // Validate ZoneId if provided
        if (zoneId.HasValue)
        {
            var zone = await _storyMapRepository.GetZoneAsync(zoneId.Value, ct);
            if (zone is null)
            {
                return Option.None<LocationDto, Error>(Error.NotFound("Poi.Zone.NotFound", "Zone not found"));
            }
        }

        if (linkedPoiId.HasValue)
        {
            var linked = await _locationRepository.GetByIdAsync(linkedPoiId.Value, ct);
            if (linked is null || linked.MapId != request.MapId)
            {
                return Option.None<LocationDto, Error>(Error.NotFound("Poi.Linked.NotFound",
                    "Linked POI not found in this map"));
            }
        }
        
        string? iconUrl = null;
        if (request.IconFile != null && request.IconFile.Length > 0)
        {
            using var stream = request.IconFile.OpenReadStream();
            iconUrl = await _firebaseStorageService.UploadFileAsync(request.IconFile.FileName, stream, "poi-icons");
        }
        
        // Process HTML content to upload base64 images to Firebase Storage
        var processedTooltipContent = await _htmlImageProcessor.ProcessHtmlContentAsync(
            request.TooltipContent, 
            folder: "poi-tooltips", 
            ct);
        
        var processedPopupContent = await _htmlImageProcessor.ProcessHtmlContentAsync(
            request.SlideContent, 
            folder: "poi-popups", 
            ct);

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
            IconUrl = iconUrl,
            ShowTooltip = request.ShowTooltip,
            TooltipContent = processedTooltipContent,
            OpenPopupOnClick = request.OpenSlideOnClick,
            PopupContent = processedPopupContent,
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

        var LocationDto = location.ToDto();

        return Option.Some<LocationDto, Error>(LocationDto);
    }

    public async Task<Option<LocationDto, Error>> UpdateLocationAsync(Guid poiId, UpdateLocationRequest request,
        CancellationToken ct = default)
    {
        var location = await _locationRepository.GetByIdAsync(poiId, ct);
        if (location is null)
        {
            return Option.None<LocationDto, Error>(Error.NotFound("Poi.NotFound", "Point of interest not found"));
        }

        // Use MapId directly from location
        var mapId = location.MapId;

        Guid? segmentId = (request.SegmentId);
        if (segmentId.HasValue)
        {
            var segment = await _storyMapRepository.GetSegmentAsync(segmentId.Value, ct);
            if (segment is null || segment.MapId != mapId)
            {
                return Option.None<LocationDto, Error>(Error.NotFound("Poi.Segment.NotFound",
                    "Segment not found for this map"));
            }
        }

        Guid? zoneId = (request.ZoneId);
        if (zoneId.HasValue)
        {
            var zone = await _storyMapRepository.GetZoneAsync(zoneId.Value, ct);
            if (zone is null)
            {
                return Option.None<LocationDto, Error>(Error.NotFound("Poi.Zone.NotFound", "Zone not found"));
            }
        }

        Guid? linkedPoiId = (request.LinkedLocationId);
        if (linkedPoiId.HasValue)
        {
            if (linkedPoiId.Value == poiId)
            {
                return Option.None<LocationDto, Error>(Error.ValidationError("Poi.Linked.Self",
                    "A POI cannot link to itself"));
            }

            var linked = await _locationRepository.GetByIdAsync(linkedPoiId.Value, ct);
            if (linked is null || linked.MapId != mapId)
            {
                return Option.None<LocationDto, Error>(Error.NotFound("Poi.Linked.NotFound",
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

        // Process HTML content to upload base64 images to Firebase Storage
        if (request.TooltipContent is not null)
        {
            location.TooltipContent = await _htmlImageProcessor.ProcessHtmlContentAsync(
                request.TooltipContent, 
                folder: "poi-tooltips", 
                ct);
        }
        
        if (request.SlideContent is not null)
        {
            location.PopupContent = await _htmlImageProcessor.ProcessHtmlContentAsync(
                request.SlideContent, 
                folder: "poi-popups", 
                ct);
        }

        location.ShowTooltip = request.ShowTooltip;
        location.OpenPopupOnClick = request.OpenSlideOnClick;
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

        return Option.Some<LocationDto, Error>(location.ToDto());
    }

    public async Task<Option<bool, Error>> DeleteLocationAsync(Guid poiId, CancellationToken ct = default)
    {
        var location = await _locationRepository.GetByIdAsync(poiId, ct);
        if (location is null)
        {
            return Option.None<bool, Error>(Error.NotFound("Poi.NotFound", "Point of interest not found"));
        }

        await _locationRepository.DeleteAsync(poiId, ct);

        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<LocationDto, Error>> UpdateLocationDisplayConfigAsync(Guid poiId,
        UpdateLocationDisplayConfigRequest request, CancellationToken ct = default)
    {
        var location = await _locationRepository.GetByIdAsync(poiId, ct);
        if (location is null)
        {
            return Option.None<LocationDto, Error>(Error.NotFound("Poi.NotFound", "Point of interest not found"));
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
            location.TooltipContent = await _htmlImageProcessor.ProcessHtmlContentAsync(
                request.TooltipContent, 
                folder: "poi-tooltips", 
                ct);
        }

        location.UpdatedAt = DateTime.UtcNow;
        var updated = await _locationRepository.UpdateAsync(location, ct) ?? location;

        var dto = updated.ToDto();

        return Option.Some<LocationDto, Error>(dto);
    }

    public async Task<Option<LocationDto, Error>> UpdateLocationInteractionConfigAsync(Guid poiId,
        UpdateLocationInteractionConfigRequest request, CancellationToken ct = default)
    {
        var location = await _locationRepository.GetByIdAsync(poiId, ct);
        if (location is null)
        {
            return Option.None<LocationDto, Error>(Error.NotFound("Poi.NotFound", "Point of interest not found"));
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

        var dto = updated.ToDto();

        return Option.Some<LocationDto, Error>(dto);
    }

    public async Task<Option<bool, Error>> MoveLocationToSegmentAsync(
        Guid locationId, Guid fromSegmentId, Guid toSegmentId, CancellationToken ct = default)
    {
        var location = await _locationRepository.GetByIdAsync(locationId, ct);
        if (location is null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Location.NotFound", "Location not found"));
        }
        
        if (location.SegmentId != fromSegmentId)
        {
            return Option.None<bool, Error>(
                Error.Failure("Location.MoveInvalid", "Location does not belong to source segment"));
        }

        var toSegment = await _storyMapRepository.GetSegmentAsync(toSegmentId, ct);
        if (toSegment is null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Segment.NotFound", $"Target segment {toSegmentId} not found"));
        }

        if (toSegment.MapId != location.MapId)
        {
            return Option.None<bool, Error>(
                Error.Failure("Location.MoveInvalid", "Cannot move location between segments from different maps"));
        }

        await _locationRepository.UpdateSegmentIdAsync(locationId, toSegmentId, ct);

        return Option.Some<bool, Error>(true);
    }
}