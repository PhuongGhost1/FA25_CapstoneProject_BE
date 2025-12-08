using System.IO;
using CusomMapOSM_Application.Common;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Common.Mappers;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Interfaces.Services.Firebase;
using CusomMapOSM_Application.Interfaces.Services.Assets;
using CusomMapOSM_Application.Interfaces.Features.Locations;
using CusomMapOSM_Application.Models.DTOs.Features.Locations;
using Microsoft.AspNetCore.Http;
using CusomMapOSM_Domain.Entities.Locations;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.StoryMaps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Locations;
using CusomMapOSM_Infrastructure.Services;
using Optional;

using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Workspaces;
using CusomMapOSM_Infrastructure.Features.Locations;

namespace CusomMapOSM_Infrastructure.Features.Locations;

public class LocationService : ILocationService
{
    private readonly IStoryMapRepository _storyMapRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly HtmlContentImageProcessor _htmlImageProcessor;
    private readonly IFirebaseStorageService _firebaseStorageService;
    private readonly IUserAssetService _userAssetService;
    private readonly IWorkspaceRepository _workspaceRepository;

    public LocationService(
        IStoryMapRepository storyMapRepository, 
        ILocationRepository locationRepository,
        ICurrentUserService currentUserService,
        HtmlContentImageProcessor htmlImageProcessor,
        IFirebaseStorageService firebaseStorageService,
        IUserAssetService userAssetService,
        IWorkspaceRepository workspaceRepository)
    {
        _storyMapRepository = storyMapRepository;
        _locationRepository = locationRepository;
        _currentUserService = currentUserService;
        _htmlImageProcessor = htmlImageProcessor;
        _firebaseStorageService = firebaseStorageService;
        _userAssetService = userAssetService;
        _workspaceRepository = workspaceRepository;
    }

    private async Task<Guid?> GetOrganizationIdAsync(Guid mapId, CancellationToken ct)
    {
        var map = await _storyMapRepository.GetMapAsync(mapId, ct);
        if (map == null || !map.WorkspaceId.HasValue)
        {
            return null;
        }

        var workspace = await _workspaceRepository.GetByIdAsync(map.WorkspaceId.Value);
        return workspace?.OrgId;
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

    var segmentId = GuidHelper.ParseNullableGuid(request.SegmentId);
    var zoneId = GuidHelper.ParseNullableGuid(request.ZoneId);
    var linkedPoiId = GuidHelper.ParseNullableGuid(request.LinkedLocationId);
    
    if (segmentId.HasValue)
    {
        var segment = await _storyMapRepository.GetSegmentAsync(segmentId.Value, ct);
        if (segment is null || segment.MapId != request.MapId)
        {
            return Option.None<LocationDto, Error>(Error.NotFound("Poi.Segment.NotFound", "Segment not found for this map"));
        }
    }

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
            return Option.None<LocationDto, Error>(Error.NotFound("Poi.Linked.NotFound", "Linked POI not found in this map"));
        }
    }
    
    var orgId = await GetOrganizationIdAsync(request.MapId, ct);

    string? iconUrl = null;
    if (request.IconFile != null && request.IconFile.Length > 0)
    {
        using var stream = request.IconFile.OpenReadStream();
        iconUrl = await _firebaseStorageService.UploadFileAsync(request.IconFile.FileName, stream, "poi-icons");
        
        // Register in User Library
        try 
        {
            await _userAssetService.CreateAssetMetadataAsync(
                request.IconFile.FileName,
                iconUrl,
                request.IconFile.ContentType,
                request.IconFile.Length,
                orgId);
        }
        catch (Exception ex)
        {
            // Don't fail the whole request if library registration fails
            // Console.WriteLine($"Failed to register icon asset: {ex.Message}");
        }
    }
    else if (!string.IsNullOrWhiteSpace(request.IconUrl))
    {
        iconUrl = request.IconUrl;
    }
    
    string? audioUrl = null;
    if (request.AudioFile != null && request.AudioFile.Length > 0)
    {
        using var stream = request.AudioFile.OpenReadStream();
        audioUrl = await _firebaseStorageService.UploadFileAsync(request.AudioFile.FileName, stream, "poi-audio");
        
        // Register in User Library
        try 
        {
            await _userAssetService.CreateAssetMetadataAsync(
                request.AudioFile.FileName,
                audioUrl,
                request.AudioFile.ContentType,
                request.AudioFile.Length,
                orgId);
        }
        catch (Exception)
        {
            // Ignore failure
        }
    }
    else if (!string.IsNullOrWhiteSpace(request.AudioUrl))
    {
        audioUrl = request.AudioUrl;
    }

    // Process HTML content
    var processedTooltipContent = await _htmlImageProcessor.ProcessHtmlContentAsync(
        request.TooltipContent, 
        folder: "poi-tooltips", 
        orgId: orgId,
        ct: ct);

    var processedPopupContent = await _htmlImageProcessor.ProcessHtmlContentAsync(
        request.PopupContent, 
        folder: "poi-popups", 
        orgId: orgId,
        ct: ct);
    
    var location = new Location
    {
        LocationId = Guid.NewGuid(),
        MapId = request.MapId,
        SegmentId = segmentId,
        ZoneId = zoneId,
        Title = request.Title,
        Subtitle = request.Subtitle,
        Description = request.Description,
        LocationType = request.LocationType,
        DisplayOrder = request.DisplayOrder,
        MarkerGeometry = request.MarkerGeometry,
        IconType = request.IconType,
        IconUrl = iconUrl ?? request.IconUrl,
        IconColor = request.IconColor,
        IconSize = request.IconSize,
        ZIndex = request.ZIndex,
        ShowTooltip = request.ShowTooltip,
        TooltipContent = processedTooltipContent,
        OpenPopupOnClick = request.OpenPopupOnClick,
        PopupContent = processedPopupContent,
        MediaUrls = request.MediaResources,
        PlayAudioOnClick = request.PlayAudioOnClick,
        AudioUrl = audioUrl ?? request.AudioUrl,
        EntryDelayMs = request.EntryDelayMs,
        EntryDurationMs = request.EntryDurationMs,
        ExitDelayMs = request.ExitDelayMs,
        ExitDurationMs = request.ExitDurationMs,
        EntryEffect = request.EntryEffect,
        ExitEffect = request.ExitEffect,
        LinkedLocationId = linkedPoiId,
        ExternalUrl = request.ExternalUrl,
        IsVisible = request.IsVisible,
        CreatedBy = currentUserId.Value,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    location = await _locationRepository.CreateAsync(location, ct);

    var locationDto = location.ToDto();

    return Option.Some<LocationDto, Error>(locationDto);
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
        var orgId = await GetOrganizationIdAsync(mapId, ct);

        var segmentId = GuidHelper.ParseNullableGuid(request.SegmentId);
        if (segmentId.HasValue)
        {
            var segment = await _storyMapRepository.GetSegmentAsync(segmentId.Value, ct);
            if (segment is null || segment.MapId != mapId)
            {
                return Option.None<LocationDto, Error>(Error.NotFound("Poi.Segment.NotFound",
                    "Segment not found for this map"));
            }
        }

        var zoneId = GuidHelper.ParseNullableGuid(request.ZoneId);
        if (zoneId.HasValue)
        {
            var zone = await _storyMapRepository.GetZoneAsync(zoneId.Value, ct);
            if (zone is null)
            {
                return Option.None<LocationDto, Error>(Error.NotFound("Poi.Zone.NotFound", "Zone not found"));
            }
        }

        var linkedPoiId = GuidHelper.ParseNullableGuid(request.LinkedLocationId);
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
        var currentUserId = _currentUserService.GetUserId();

        if (request.IconFile != null && request.IconFile.Length > 0)
        {
            using var iconStream = request.IconFile.OpenReadStream();
            var newIconUrl = await _firebaseStorageService.UploadFileAsync(request.IconFile.FileName, iconStream, "poi-icons");
            location.IconUrl = newIconUrl;

            // Register in User Library if we have a user
            if (currentUserId.HasValue)
            {
                try 
                {
                    await _userAssetService.CreateAssetMetadataAsync(
                        request.IconFile.FileName,
                        newIconUrl,
                        request.IconFile.ContentType,
                        request.IconFile.Length,
                        orgId);
                }
                catch (Exception) { /* Ensure robust */ }
            }
        }
        
        if (request.AudioFile != null && request.AudioFile.Length > 0)
        {
            using var audioStream = request.AudioFile.OpenReadStream();
            var newAudioUrl = await _firebaseStorageService.UploadFileAsync(request.AudioFile.FileName, audioStream, "poi-audio");
            location.AudioUrl = newAudioUrl;

            // Register in User Library if we have a user
            if (currentUserId.HasValue)
            {
                try 
                {
                    await _userAssetService.CreateAssetMetadataAsync(
                        request.AudioFile.FileName,
                        newAudioUrl,
                        request.AudioFile.ContentType,
                        request.AudioFile.Length,
                        orgId);
                }
                catch (Exception) { /* Ensure robust */ }
            }
        }
        else if (!string.IsNullOrWhiteSpace(request.AudioUrl))
        {

            location.AudioUrl = request.AudioUrl;
        }

        location.SegmentId = segmentId;
        location.ZoneId = zoneId;
        location.Title = request.Title;
        location.Subtitle = request.Subtitle;
        location.Description = request.Description;
        location.LocationType = request.LocationType;
        location.DisplayOrder = request.DisplayOrder;
        location.MarkerGeometry = request.MarkerGeometry;
        location.IconType = request.IconType;
        location.IconColor = request.IconColor;
        location.IconSize = request.IconSize;
        location.ZIndex = request.ZIndex;
        location.ShowTooltip = request.ShowTooltip;
        location.OpenPopupOnClick = request.OpenPopupOnClick;
        location.MediaUrls = request.MediaResources;
        location.PlayAudioOnClick = request.PlayAudioOnClick;
        location.EntryDelayMs = request.EntryDelayMs;
        location.EntryDurationMs = request.EntryDurationMs;
        location.ExitDelayMs = request.ExitDelayMs;
        location.ExitDurationMs = request.ExitDurationMs;
        location.EntryEffect = request.EntryEffect;
        location.ExitEffect = request.ExitEffect;
        location.LinkedLocationId = linkedPoiId;
        location.ExternalUrl = request.ExternalUrl;
        location.IsVisible = request.IsVisible;

        // Process HTML content to upload base64 images to Firebase Storage
        if (request.TooltipContent is not null)
        {
            location.TooltipContent = await _htmlImageProcessor.ProcessHtmlContentAsync(
                request.TooltipContent, 
                folder: "poi-tooltips", 
                orgId: orgId,
                ct: ct);
        }
        else
        {
            location.TooltipContent = null;
        }
        
        if (request.PopupContent is not null)
        {
            location.PopupContent = await _htmlImageProcessor.ProcessHtmlContentAsync(
                request.PopupContent, 
                folder: "poi-popups", 
                orgId: orgId,
                ct: ct);
        }
        else
        {
            location.PopupContent = null;
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
        var orgId = await GetOrganizationIdAsync(location.MapId, ct);

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
                orgId: orgId,
                ct: ct);
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

    public async Task<Option<UploadLocationAudioResponse, Error>> UploadLocationAudioAsync(IFormFile file, Guid? mapId, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
        {
            return Option.None<UploadLocationAudioResponse, Error>(
                Error.ValidationError("Location.Audio.Empty", "No file provided"));
        }

        var allowedExtensions = new[] { ".mp3", ".wav", ".ogg", ".m4a" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return Option.None<UploadLocationAudioResponse, Error>(
                Error.ValidationError("Location.Audio.InvalidType", "Invalid file type. Only audio files are allowed."));
        }

        try
        {
            using var stream = file.OpenReadStream();
            var storageUrl = await _firebaseStorageService.UploadFileAsync(file.FileName, stream, "location-audio");
            
            var userId = _currentUserService.GetUserId();
            if (userId.HasValue)
            {
                Guid? orgId = null;
                if (mapId.HasValue)
                {
                    orgId = await GetOrganizationIdAsync(mapId.Value, ct);
                }

                try 
                {
                    await _userAssetService.CreateAssetMetadataAsync(
                        file.FileName,
                        storageUrl,
                        file.ContentType,
                        file.Length,
                        orgId);
                }
                catch (Exception)
                {
                }
            }

            return Option.Some<UploadLocationAudioResponse, Error>(new UploadLocationAudioResponse(storageUrl));
        }
        catch (Exception ex)
        {
            return Option.None<UploadLocationAudioResponse, Error>(
                Error.Problem("Location.Audio.UploadFailed", $"Failed to upload audio: {ex.Message}"));
        }
    }
}