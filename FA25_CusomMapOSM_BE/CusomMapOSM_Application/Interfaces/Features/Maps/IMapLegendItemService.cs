using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Maps;

public interface IMapLegendItemService
{    
    Task<Option<GetMapLegendItemsResponse, Error>> GetByMapId(Guid mapId, CancellationToken ct = default);
    Task<Option<MapLegendItemDto, Error>> GetById(Guid mapId, Guid legendItemId, CancellationToken ct = default);
    Task<Option<CreateMapLegendItemResponse, Error>> Create(Guid mapId, Guid userId, CreateMapLegendItemRequest request, CancellationToken ct = default);
    Task<Option<UpdateMapLegendItemResponse, Error>> Update(Guid mapId, Guid legendItemId, Guid userId, UpdateMapLegendItemRequest request, CancellationToken ct = default);
    Task<Option<DeleteMapLegendItemResponse, Error>> Delete(Guid mapId, Guid legendItemId, Guid userId, CancellationToken ct = default);
    Task<Option<ReorderMapLegendItemsResponse, Error>> Reorder(Guid mapId, Guid userId, ReorderMapLegendItemsRequest request, CancellationToken ct = default);
}
