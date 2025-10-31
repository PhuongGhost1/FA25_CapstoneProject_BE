using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.POIs;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.POIs;

public interface IPoiService
{
    Task<Option<IReadOnlyCollection<PoiDto>, Error>> GetMapPoisAsync(Guid mapId, CancellationToken ct = default);
    Task<Option<IReadOnlyCollection<PoiDto>, Error>> GetSegmentPoisAsync(Guid mapId, Guid segmentId, CancellationToken ct = default);
    Task<Option<PoiDto, Error>> CreatePoiAsync(CreatePoiRequest request, CancellationToken ct = default);
    Task<Option<PoiDto, Error>> UpdatePoiAsync(Guid poiId, UpdatePoiRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> DeletePoiAsync(Guid poiId, CancellationToken ct = default);
    Task<Option<PoiDto, Error>> UpdatePoiDisplayConfigAsync(Guid poiId, UpdatePoiDisplayConfigRequest request, CancellationToken ct = default);
    Task<Option<PoiDto, Error>> UpdatePoiInteractionConfigAsync(Guid poiId, UpdatePoiInteractionConfigRequest request, CancellationToken ct = default);
}
