using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Layers;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Layers;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Layers;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.Layers;

public class LayerService : ILayerService
{
    private readonly ILayerRepository _layerRepository;
    private readonly ICurrentUserService _currentUserService;

    public LayerService(ILayerRepository layerRepository, ICurrentUserService currentUserService)
    {
        _layerRepository = layerRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Option<List<LayerSummaryDto>, Error>> GetAvailableLayersAsync(CancellationToken ct = default)
    {
        var userId = _currentUserService.GetUserId();
        if (!userId.HasValue)
        {
            return Option.None<List<LayerSummaryDto>, Error>(Error.Unauthorized("Layer.Unauthorized", "User is not authenticated"));
        }

        try
        {
            var layers = await _layerRepository.GetAvailableLayersAsync(userId.Value, ct);
            return Option.Some<List<LayerSummaryDto>, Error>(layers.Select(ToSummaryDto).ToList());
        }
        catch (Exception ex)
        {
            return Option.None<List<LayerSummaryDto>, Error>(Error.Failure("Layer.GetAvailable", $"Failed to load layers: {ex.Message}"));
        }
    }

    public async Task<Option<LayerDetailDto, Error>> GetLayerByIdAsync(Guid layerId, CancellationToken ct = default)
    {
        var userId = _currentUserService.GetUserId();
        if (!userId.HasValue)
        {
            return Option.None<LayerDetailDto, Error>(Error.Unauthorized("Layer.Unauthorized", "User is not authenticated"));
        }

        try
        {
            var layer = await _layerRepository.GetLayerByIdAsync(layerId, userId.Value, ct);
            if (layer == null)
            {
                return Option.None<LayerDetailDto, Error>(Error.NotFound("Layer.NotFound", "Layer not found or access denied"));
            }

            return Option.Some<LayerDetailDto, Error>(ToDetailDto(layer));
        }
        catch (Exception ex)
        {
            return Option.None<LayerDetailDto, Error>(Error.Failure("Layer.GetById", $"Failed to load layer: {ex.Message}"));
        }
    }

    public async Task<Option<List<LayerDetailDto>, Error>> GetLayersByMapAsync(Guid mapId, CancellationToken ct = default)
    {
        var userId = _currentUserService.GetUserId();
        if (!userId.HasValue)
        {
            return Option.None<List<LayerDetailDto>, Error>(Error.Unauthorized("Layer.Unauthorized", "User is not authenticated"));
        }

        try
        {
            var layers = await _layerRepository.GetLayersByMapAsync(mapId, userId.Value, ct);
            return Option.Some<List<LayerDetailDto>, Error>(layers.Select(ToDetailDto).ToList());
        }
        catch (Exception ex)
        {
            return Option.None<List<LayerDetailDto>, Error>(Error.Failure("Layer.GetByMap", $"Failed to load map layers: {ex.Message}"));
        }
    }

    private static LayerSummaryDto ToSummaryDto(Layer layer)
    {
        return new LayerSummaryDto
        {
            LayerId = layer.LayerId,
            MapId = layer.MapId,
            LayerName = layer.LayerName,
            LayerType = layer.LayerType,
            SourceType = layer.SourceType,
            IsPublic = layer.Map?.IsPublic ?? false,
            FeatureCount = layer.FeatureCount,
            DataSizeKB = layer.DataSizeKB,
            DataBounds = layer.DataBounds,
            CreatedAt = layer.CreatedAt,
            UpdatedAt = layer.UpdatedAt
        };
    }

    private static LayerDetailDto ToDetailDto(Layer layer)
    {
        return new LayerDetailDto
        {
            LayerId = layer.LayerId,
            MapId = layer.MapId,
            LayerName = layer.LayerName,
            LayerType = layer.LayerType,
            SourceType = layer.SourceType,
            IsPublic = layer.Map?.IsPublic ?? false,
            FeatureCount = layer.FeatureCount,
            DataSizeKB = layer.DataSizeKB,
            DataBounds = layer.DataBounds,
            CreatedAt = layer.CreatedAt,
            UpdatedAt = layer.UpdatedAt,
            UserId = layer.UserId,
            FilePath = layer.FilePath,
            DataStoreKey = layer.DataStoreKey,
            LayerData = layer.LayerData,
            LayerStyle = layer.LayerStyle,
            IsVisible = layer.IsVisible
        };
    }
}

