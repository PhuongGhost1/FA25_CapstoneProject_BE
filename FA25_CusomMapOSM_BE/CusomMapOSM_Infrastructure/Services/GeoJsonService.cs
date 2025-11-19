using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CusomMapOSM_Application.Interfaces.Services.GeoJson;
using CusomMapOSM_Application.Models.DTOs.Services.GeoJson;

namespace CusomMapOSM_Infrastructure.Services;

public class GeoJsonService : IGeoJsonService
{
    public GeoJsonLayerData ProcessGeoJsonUpload(string geoJsonString, string layerName)
    {
        var result = new GeoJsonLayerData();

        try
        {
            // Validate GeoJSON format
            if (!ValidateGeoJson(geoJsonString))
            {
                result.IsValid = false;
                result.ValidationErrors = "Invalid GeoJSON format";
                return result;
            }

            var geoJson = JsonSerializer.Deserialize<JsonElement>(geoJsonString);

            // Calculate metadata
            var featureCount = 0;
            var geometryTypes = new HashSet<string>();
            var propertyNames = new HashSet<string>();

            if (geoJson.TryGetProperty("features", out var features) && features.ValueKind == JsonValueKind.Array)
            {
                featureCount = features.GetArrayLength();

                foreach (var feature in features.EnumerateArray())
                {
                    // Extract geometry types
                    if (feature.TryGetProperty("geometry", out var geometry) &&
                        geometry.TryGetProperty("type", out var geomType))
                    {
                        geometryTypes.Add(geomType.GetString() ?? "Unknown");
                    }

                    // Extract property names
                    if (feature.TryGetProperty("properties", out var properties) &&
                        properties.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in properties.EnumerateObject())
                        {
                            propertyNames.Add(prop.Name);
                        }
                    }
                }
            }

            var dataSizeKB = Encoding.UTF8.GetByteCount(geoJsonString) / 1024.0;

            // Calculate bounds
            var bounds = CalculateBounds(geoJsonString);

            // Generate default style
            var defaultStyle = GenerateDefaultStyle(geoJsonString);

            result.LayerData = geoJsonString;
            result.FeatureCount = featureCount;
            result.DataSizeKB = Math.Round(dataSizeKB, 2);
            result.DataBounds = bounds;
            result.LayerStyle = defaultStyle;
            result.GeometryType = string.Join(", ", geometryTypes);
            result.PropertyNames = propertyNames.ToList();
            result.IsValid = true;

            return result;
        }
        catch (JsonException ex)
        {
            result.IsValid = false;
            result.ValidationErrors = $"JSON parsing error: {ex.Message}";
            return result;
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.ValidationErrors = $"Processing error: {ex.Message}";
            return result;
        }
    }

    public bool ValidateGeoJson(string geoJsonString)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(geoJsonString))
                return false;

            var geoJson = JsonSerializer.Deserialize<JsonElement>(geoJsonString);

            // Check for required GeoJSON structure
            if (!geoJson.TryGetProperty("type", out var typeProperty))
                return false;

            var type = typeProperty.GetString();

            // Validate GeoJSON types
            var validTypes = new[]
            {
                "FeatureCollection", "Feature", "Point", "LineString", "Polygon",
                "MultiPoint", "MultiLineString", "MultiPolygon", "GeometryCollection"
            };

            if (!validTypes.Contains(type))
                return false;

            // Additional validation for FeatureCollection
            if (type == "FeatureCollection")
            {
                if (!geoJson.TryGetProperty("features", out var features) ||
                    features.ValueKind != JsonValueKind.Array)
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public string CalculateBounds(string geoJsonString)
    {
        try
        {
            var geoJson = JsonSerializer.Deserialize<JsonElement>(geoJsonString);

            double minLng = double.MaxValue;
            double maxLng = double.MinValue;
            double minLat = double.MaxValue;
            double maxLat = double.MinValue;

            // Extract coordinates from all features
            if (geoJson.TryGetProperty("features", out var features) && features.ValueKind == JsonValueKind.Array)
            {
                foreach (var feature in features.EnumerateArray())
                {
                    if (feature.TryGetProperty("geometry", out var geometry))
                    {
                        var bounds = ExtractCoordinateBounds(geometry);
                        if (bounds.HasValue)
                        {
                            minLng = Math.Min(minLng, bounds.Value.MinLng);
                            maxLng = Math.Max(maxLng, bounds.Value.MaxLng);
                            minLat = Math.Min(minLat, bounds.Value.MinLat);
                            maxLat = Math.Max(maxLat, bounds.Value.MaxLat);
                        }
                    }
                }
            }

            // Return bounding box as GeoJSON Polygon if valid bounds were found
            if (minLng != double.MaxValue && maxLng != double.MinValue &&
                minLat != double.MaxValue && maxLat != double.MinValue)
            {
                return $@"{{
                    ""type"": ""Polygon"",
                    ""coordinates"": [[
                        [{minLng}, {minLat}],
                        [{maxLng}, {minLat}],
                        [{maxLng}, {maxLat}],
                        [{minLng}, {maxLat}],
                        [{minLng}, {minLat}]
                    ]]
                }}";
            }

            // Default fallback bounds
            return @"{""type"":""Polygon"",""coordinates"":[[[0,0],[0,1],[1,1],[1,0],[0,0]]]}";
        }
        catch
        {
            // Default fallback bounds
            return @"{""type"":""Polygon"",""coordinates"":[[[0,0],[0,1],[1,1],[1,0],[0,0]]]}";
        }
    }

    public string GenerateDefaultStyle(string geoJsonString)
    {
        try
        {
            var geoJson = JsonSerializer.Deserialize<JsonElement>(geoJsonString);
            var geometryTypes = new HashSet<string>();

            // Determine geometry types in the GeoJSON
            if (geoJson.TryGetProperty("features", out var features) && features.ValueKind == JsonValueKind.Array)
            {
                foreach (var feature in features.EnumerateArray())
                {
                    if (feature.TryGetProperty("geometry", out var geometry) &&
                        geometry.TryGetProperty("type", out var geomType))
                    {
                        geometryTypes.Add(geomType.GetString() ?? "");
                    }
                }
            }

            // Generate style based on predominant geometry type
            var hasPolygon = geometryTypes.Any(t => t.Contains("Polygon"));
            var hasLine = geometryTypes.Any(t => t.Contains("LineString"));
            var hasPoint = geometryTypes.Any(t => t.Contains("Point"));

            if (hasPolygon)
            {
                return @"{
                    ""fill"": {""color"": ""#3388ff"", ""opacity"": 0.2},
                    ""stroke"": {""color"": ""#3388ff"", ""width"": 2},
                    ""labels"": {
                        ""enabled"": false,
                        ""field"": ""name"",
                        ""fontSize"": 12,
                        ""fontFamily"": ""Arial"",
                        ""fontColor"": ""#000000"",
                        ""fontWeight"": ""normal"",
                        ""textAnchor"": ""center"",
                        ""alignment"": ""center"",
                        ""offset"": [0, 0],
                        ""haloColor"": ""#FFFFFF"",
                        ""haloWidth"": 2
                    },
                    ""type"": ""polygon""
                }";
            }
            else if (hasLine)
            {
                return @"{
                    ""fill"": {""color"": ""#3388ff"", ""opacity"": 0.2},
                    ""stroke"": {""color"": ""#3388ff"", ""width"": 2},
                    ""labels"": {
                        ""enabled"": false,
                        ""field"": ""name"",
                        ""fontSize"": 12,
                        ""fontFamily"": ""Arial"",
                        ""fontColor"": ""#000000"",
                        ""fontWeight"": ""normal"",
                        ""textAnchor"": ""center"",
                        ""alignment"": ""center"",
                        ""offset"": [0, 0],
                        ""haloColor"": ""#FFFFFF"",
                        ""haloWidth"": 2
                    },
                    ""type"": ""polygon""
                }";
            }
            else if (hasPoint)
            {
                return @"{
                    ""marker"": {
                        ""color"": ""#51cf66"",
                        ""size"": 8,
                        ""symbol"": ""circle""
                    },
                    ""radius"": {
                        ""type"": ""fixed"",
                        ""value"": 8,
                        ""field"": null,
                        ""scale"": {
                            ""min"": 5,
                            ""max"": 50
                        }
                    },
                    ""labels"": {
                        ""enabled"": false,
                        ""field"": ""name"",
                        ""fontSize"": 12,
                        ""fontFamily"": ""Arial"",
                        ""fontColor"": ""#000000"",
                        ""fontWeight"": ""normal"",
                        ""textAnchor"": ""center"",
                        ""alignment"": ""top"",
                        ""offset"": [0, -10],
                        ""haloColor"": ""#FFFFFF"",
                        ""haloWidth"": 2
                    },
                    ""type"": ""point""
                }";
            }

            // Default mixed geometry style
            return @"{
                    ""fill"": {""color"": ""#3388ff"", ""opacity"": 0.2},
                    ""stroke"": {""color"": ""#3388ff"", ""width"": 2},
                    ""labels"": {
                        ""enabled"": false,
                        ""field"": ""name"",
                        ""fontSize"": 12,
                        ""fontFamily"": ""Arial"",
                        ""fontColor"": ""#000000"",
                        ""fontWeight"": ""normal"",
                        ""textAnchor"": ""center"",
                        ""alignment"": ""center"",
                        ""offset"": [0, 0],
                        ""haloColor"": ""#FFFFFF"",
                        ""haloWidth"": 2
                    },
                    ""type"": ""polygon""
                }";
        }
        catch
        {
            // Default fallback style
            return @"{
                ""fill"": {""color"": ""#3388ff"", ""opacity"": 0.2},
                ""stroke"": {""color"": ""#3388ff"", ""width"": 2},
                ""labels"": {
                    ""enabled"": false,
                    ""field"": ""name"",
                    ""fontSize"": 12,
                    ""fontFamily"": ""Arial"",
                    ""fontColor"": ""#000000"",
                    ""fontWeight"": ""normal"",
                    ""textAnchor"": ""center"",
                    ""alignment"": ""center"",
                    ""offset"": [0, 0],
                    ""haloColor"": ""#FFFFFF"",
                    ""haloWidth"": 2
                },
                ""popup"": {""enabled"": true}
            }";
        }
    }

    private (double MinLng, double MaxLng, double MinLat, double MaxLat)? ExtractCoordinateBounds(JsonElement geometry)
    {
        try
        {
            if (!geometry.TryGetProperty("coordinates", out var coordinates))
                return null;

            var allCoords = new List<(double lng, double lat)>();
            ExtractAllCoordinates(coordinates, allCoords);

            if (!allCoords.Any())
                return null;

            return (
                MinLng: allCoords.Min(c => c.lng),
                MaxLng: allCoords.Max(c => c.lng),
                MinLat: allCoords.Min(c => c.lat),
                MaxLat: allCoords.Max(c => c.lat)
            );
        }
        catch
        {
            return null;
        }
    }

    private void ExtractAllCoordinates(JsonElement coordinates, List<(double lng, double lat)> allCoords)
    {
        if (coordinates.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in coordinates.EnumerateArray())
            {
                if (element.ValueKind == JsonValueKind.Array)
                {
                    var array = element.EnumerateArray().ToArray();
                    if (array.Length == 2 &&
                        array[0].ValueKind == JsonValueKind.Number &&
                        array[1].ValueKind == JsonValueKind.Number)
                    {
                        allCoords.Add((array[0].GetDouble(), array[1].GetDouble()));
                    }
                    else
                    {
                        ExtractAllCoordinates(element, allCoords);
                    }
                }
            }
        }
    }
}