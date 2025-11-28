using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using CusomMapOSM_Application.Models.Documents;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Maps.Enums;

namespace CusomMapOSM_Application.Common.Mappers;

public static class MapFeatureMappings
{
    public static MapFeatureResponse ToResponse(this MapFeature feature, MapFeatureDocument? document = null)
    {
        var coordinates = document?.Geometry?.ToString() ?? string.Empty;

        if (feature.GeometryType == GeometryTypeEnum.Rectangle && document?.Geometry != null)
        {
            try
            {
                var geometryJson = JsonSerializer.Deserialize<JsonNode>(document.Geometry.ToString() ?? "{}");
                if (geometryJson != null && geometryJson["bounds"] != null)
                {
                    coordinates = geometryJson["bounds"]?.ToString() ?? coordinates;
                }
                else if (geometryJson != null && geometryJson["type"]?.GetValue<string>() == "Polygon")
                {
                    var polygonCoords = geometryJson["coordinates"]?[0]?.AsArray();
                    if (polygonCoords != null && polygonCoords.Count >= 4)
                    {
                        var minLng = polygonCoords[0]?[0]?.GetValue<double>() ?? 0;
                        var minLat = polygonCoords[0]?[1]?.GetValue<double>() ?? 0;
                        var maxLng = polygonCoords[2]?[0]?.GetValue<double>() ?? 0;
                        var maxLat = polygonCoords[2]?[1]?.GetValue<double>() ?? 0;

                        coordinates = $"[{minLng},{minLat},{maxLng},{maxLat}]";
                    }
                }
            }
            catch (Exception)
            {
                // Ignore geometry parsing failures and return original document geometry.
            }
        }

        var properties = document?.Properties != null
            ? JsonSerializer.Serialize(document.Properties)
            : null;
        var style = document?.Style != null
            ? JsonSerializer.Serialize(document.Style)
            : null;

        return new MapFeatureResponse
        {
            FeatureId = feature.FeatureId,
            MapId = feature.MapId,
            LayerId = feature.LayerId,
            Name = feature.Name,
            Description = feature.Description,
            FeatureCategory = feature.FeatureCategory,
            AnnotationType = feature.AnnotationType,
            GeometryType = feature.GeometryType,
            Coordinates = coordinates,
            Properties = properties,
            Style = style,
            IsVisible = feature.IsVisible,
            ZIndex = feature.ZIndex,
            CreatedBy = feature.CreatedBy,
            CreatedAt = feature.CreatedAt,
            UpdatedAt = feature.UpdatedAt
        };
    }
}


