using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using Microsoft.Extensions.Logging;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.Maps;

public class MapLegendItemService : IMapLegendItemService
{
    private readonly IMapLegendItemRepository _repository;
    private readonly IMapRepository _mapRepository;
    private readonly ILogger<MapLegendItemService> _logger;

    public MapLegendItemService(
        IMapLegendItemRepository repository,
        IMapRepository mapRepository,
        ILogger<MapLegendItemService> logger)
    {
        _repository = repository;
        _mapRepository = mapRepository;
        _logger = logger;
    }

    public async Task<Option<GetMapLegendItemsResponse, Error>> GetByMapId(Guid mapId, CancellationToken ct = default)
    {
        try
        {
            var items = await _repository.GetByMapIdAsync(mapId, ct);
            
            var response = new GetMapLegendItemsResponse
            {
                Items = items.Select(ToDto).ToList()
            };

            return Option.Some<GetMapLegendItemsResponse, Error>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting legend items for map {MapId}", mapId);
            return Option.None<GetMapLegendItemsResponse, Error>(Error.Failure("LegendItem.GetFailed", "Failed to get legend items"));
        }
    }

    public async Task<Option<MapLegendItemDto, Error>> GetById(Guid mapId, Guid legendItemId, CancellationToken ct = default)
    {
        try
        {
            var item = await _repository.GetByIdAsync(legendItemId, ct);
            
            if (item == null)
            {
                return Option.None<MapLegendItemDto, Error>(Error.NotFound("LegendItem.NotFound", "Legend item not found"));
            }

            if (item.MapId != mapId)
            {
                return Option.None<MapLegendItemDto, Error>(Error.NotFound("LegendItem.NotFound", "Legend item not found for this map"));
            }

            return Option.Some<MapLegendItemDto, Error>(ToDto(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting legend item {LegendItemId}", legendItemId);
            return Option.None<MapLegendItemDto, Error>(Error.Failure("LegendItem.GetFailed", "Failed to get legend item"));
        }
    }

    public async Task<Option<CreateMapLegendItemResponse, Error>> Create(Guid mapId, Guid userId, CreateMapLegendItemRequest request, CancellationToken ct = default)
    {
        try
        {
            // Verify map exists
            var map = await _mapRepository.GetMapById(mapId);
            if (map == null)
            {
                return Option.None<CreateMapLegendItemResponse, Error>(Error.NotFound("Map.NotFound", "Map not found"));
            }

            // Get next display order
            var maxOrder = await _repository.GetMaxDisplayOrderAsync(mapId, ct);
            var displayOrder = request.DisplayOrder != 0 ? request.DisplayOrder : maxOrder + 1;

            var item = new MapLegendItem
            {
                LegendItemId = Guid.NewGuid(),
                MapId = mapId,
                CreatedBy = userId,
                Label = request.Label,
                Description = request.Description,
                Emoji = request.Emoji,
                IconUrl = request.IconUrl,
                Color = request.Color,
                DisplayOrder = displayOrder,
                IsVisible = request.IsVisible,
                CreatedAt = DateTime.UtcNow
            };

            var success = await _repository.CreateAsync(item, ct);
            
            if (!success)
            {
                return Option.None<CreateMapLegendItemResponse, Error>(Error.Failure("LegendItem.CreateFailed", "Failed to create legend item"));
            }

            _logger.LogInformation("Created legend item {LegendItemId} for map {MapId}", item.LegendItemId, mapId);

            return Option.Some<CreateMapLegendItemResponse, Error>(new CreateMapLegendItemResponse
            {
                LegendItemId = item.LegendItemId,
                Message = "Legend item created successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating legend item for map {MapId}", mapId);
            return Option.None<CreateMapLegendItemResponse, Error>(Error.Failure("LegendItem.CreateFailed", "Failed to create legend item"));
        }
    }

    public async Task<Option<UpdateMapLegendItemResponse, Error>> Update(Guid mapId, Guid legendItemId, Guid userId, UpdateMapLegendItemRequest request, CancellationToken ct = default)
    {
        try
        {
            var item = await _repository.GetByIdAsync(legendItemId, ct);
            
            if (item == null)
            {
                return Option.None<UpdateMapLegendItemResponse, Error>(Error.NotFound("LegendItem.NotFound", "Legend item not found"));
            }

            if (item.MapId != mapId)
            {
                return Option.None<UpdateMapLegendItemResponse, Error>(Error.NotFound("LegendItem.NotFound", "Legend item not found for this map"));
            }

            // Update fields if provided
            if (request.Label != null)
                item.Label = request.Label;
            
            if (request.Description != null)
                item.Description = request.Description;
            
            if (request.Emoji != null)
                item.Emoji = request.Emoji;
            
            if (request.IconUrl != null)
                item.IconUrl = request.IconUrl;
            
            if (request.Color != null)
                item.Color = request.Color;
            
            if (request.DisplayOrder.HasValue)
                item.DisplayOrder = request.DisplayOrder.Value;
            
            if (request.IsVisible.HasValue)
                item.IsVisible = request.IsVisible.Value;

            var success = await _repository.UpdateAsync(item, ct);
            
            if (!success)
            {
                return Option.None<UpdateMapLegendItemResponse, Error>(Error.Failure("LegendItem.UpdateFailed", "Failed to update legend item"));
            }

            _logger.LogInformation("Updated legend item {LegendItemId}", legendItemId);

            return Option.Some<UpdateMapLegendItemResponse, Error>(new UpdateMapLegendItemResponse
            {
                Message = "Legend item updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating legend item {LegendItemId}", legendItemId);
            return Option.None<UpdateMapLegendItemResponse, Error>(Error.Failure("LegendItem.UpdateFailed", "Failed to update legend item"));
        }
    }

    public async Task<Option<DeleteMapLegendItemResponse, Error>> Delete(Guid mapId, Guid legendItemId, Guid userId, CancellationToken ct = default)
    {
        try
        {
            var item = await _repository.GetByIdAsync(legendItemId, ct);
            
            if (item == null)
            {
                return Option.None<DeleteMapLegendItemResponse, Error>(Error.NotFound("LegendItem.NotFound", "Legend item not found"));
            }

            if (item.MapId != mapId)
            {
                return Option.None<DeleteMapLegendItemResponse, Error>(Error.NotFound("LegendItem.NotFound", "Legend item not found for this map"));
            }

            var success = await _repository.DeleteAsync(legendItemId, ct);
            
            if (!success)
            {
                return Option.None<DeleteMapLegendItemResponse, Error>(Error.Failure("LegendItem.DeleteFailed", "Failed to delete legend item"));
            }

            _logger.LogInformation("Deleted legend item {LegendItemId}", legendItemId);

            return Option.Some<DeleteMapLegendItemResponse, Error>(new DeleteMapLegendItemResponse
            {
                Message = "Legend item deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting legend item {LegendItemId}", legendItemId);
            return Option.None<DeleteMapLegendItemResponse, Error>(Error.Failure("LegendItem.DeleteFailed", "Failed to delete legend item"));
        }
    }

    public async Task<Option<ReorderMapLegendItemsResponse, Error>> Reorder(Guid mapId, Guid userId, ReorderMapLegendItemsRequest request, CancellationToken ct = default)
    {
        try
        {
            var success = await _repository.ReorderAsync(mapId, request.ItemIds, ct);
            
            if (!success)
            {
                return Option.None<ReorderMapLegendItemsResponse, Error>(Error.Failure("LegendItem.ReorderFailed", "Failed to reorder legend items"));
            }

            _logger.LogInformation("Reordered legend items for map {MapId}", mapId);

            return Option.Some<ReorderMapLegendItemsResponse, Error>(new ReorderMapLegendItemsResponse
            {
                Message = "Legend items reordered successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering legend items for map {MapId}", mapId);
            return Option.None<ReorderMapLegendItemsResponse, Error>(Error.Failure("LegendItem.ReorderFailed", "Failed to reorder legend items"));
        }
    }

    private static MapLegendItemDto ToDto(MapLegendItem item)
    {
        return new MapLegendItemDto
        {
            LegendItemId = item.LegendItemId,
            MapId = item.MapId,
            Label = item.Label,
            Description = item.Description,
            Emoji = item.Emoji,
            IconUrl = item.IconUrl,
            Color = item.Color,
            DisplayOrder = item.DisplayOrder,
            IsVisible = item.IsVisible,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }
}
