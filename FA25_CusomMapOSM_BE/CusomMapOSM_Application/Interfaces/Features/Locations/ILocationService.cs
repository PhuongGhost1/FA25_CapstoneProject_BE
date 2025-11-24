using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Locations;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Locations;

public interface ILocationService
{
    Task<Option<IReadOnlyCollection<LocationDto>, Error>> GetMapLocations(Guid mapId, CancellationToken ct = default);
    Task<Option<IReadOnlyCollection<LocationDto>, Error>> GetSegmentLocationsAsync(Guid mapId, Guid segmentId, CancellationToken ct = default);
    Task<Option<IReadOnlyCollection<LocationDto>, Error>> GetZoneLocationsAsync(Guid zoneId, CancellationToken ct = default);
    Task<Option<IReadOnlyCollection<LocationDto>, Error>> GetLocationsWithoutZoneAsync(Guid segmentId, CancellationToken ct = default);
    Task<Option<LocationDto, Error>> CreateLocationAsync(CreateLocationRequest request, CancellationToken ct = default);
    Task<Option<LocationDto, Error>> UpdateLocationAsync(Guid poiId, UpdateLocationRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> DeleteLocationAsync(Guid poiId, CancellationToken ct = default);
    Task<Option<LocationDto, Error>> UpdateLocationDisplayConfigAsync(Guid poiId, UpdateLocationDisplayConfigRequest request, CancellationToken ct = default);
    Task<Option<LocationDto, Error>> UpdateLocationInteractionConfigAsync(Guid poiId, UpdateLocationInteractionConfigRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> MoveLocationToSegmentAsync(Guid locationId, Guid fromSegmentId, Guid toSegmentId, CancellationToken ct = default);
}
