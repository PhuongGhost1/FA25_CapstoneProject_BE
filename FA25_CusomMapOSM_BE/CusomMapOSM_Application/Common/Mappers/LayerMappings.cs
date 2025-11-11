using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.Layers.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CusomMapOSM_Application.Common.Mappers
{
    public static class LayerMappings
    {
        public static LayerDTO ToLayerDto(this Layer layer, string? layerData = null)
        {
            JsonDocument? layerDataDoc = null;
            var dataToParse = layerData ?? layer.LayerData;
            if (!string.IsNullOrEmpty(dataToParse))
            {
                    layerDataDoc = JsonDocument.Parse(dataToParse);
            }

            return new LayerDTO
            {
                Id = layer.LayerId,
                LayerName = layer.LayerName ?? string.Empty,
                LayerType = layer.LayerType,
                SourceType = layer.SourceType,
                FilePath = layer.FilePath ?? string.Empty,
                LayerData = layerDataDoc,
                LayerStyle = JsonDocument.Parse(layer.LayerStyle ?? string.Empty),
                IsPublic = layer.IsPublic,
                FeatureCount = layer.FeatureCount ?? 0,
                DataSizeKB = layer.DataSizeKB ?? 0,
                DataBounds = layer.DataBounds ?? string.Empty,
                CreatedAt = layer.CreatedAt,
                UpdatedAt = layer.UpdatedAt
            };
        }
    }
}
