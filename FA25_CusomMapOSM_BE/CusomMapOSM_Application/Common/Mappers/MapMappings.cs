using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.Layers.Enums;
using CusomMapOSM_Domain.Entities.Maps;

namespace CusomMapOSM_Application.Common.Mappers;

public static class MapMappings
{
    public static GetMapByIdResponse ToGetMapByIdResponse(this Map map, List<Layer> layers, Dictionary<Guid, string> layerDataMap)
        => new GetMapByIdResponse
        {
            Id = map.MapId,
            Name = map.MapName,
            Description = map.Description ?? string.Empty,
            PreviewImage = map.PreviewImage,
            DefaultBounds = JsonDocument.Parse(string.IsNullOrEmpty(map.DefaultBounds) ? "{}" : map.DefaultBounds),
            BaseLayer = map.BaseLayer,
            ViewState = string.IsNullOrEmpty(map.ViewState) ? JsonDocument.Parse("{}") : JsonDocument.Parse(map.ViewState),
            IsPublic = map.IsPublic,
            Status = map.Status,
            PublishedAt = map.PublishedAt,
            CreatedAt = map.CreatedAt,
            UpdatedAt = map.UpdatedAt,
            Layers = layers?.Select(l => l.ToLayerDto(layerDataMap.GetValueOrDefault(l.LayerId))).ToList() ?? new List<LayerDTO>()
        };
}

