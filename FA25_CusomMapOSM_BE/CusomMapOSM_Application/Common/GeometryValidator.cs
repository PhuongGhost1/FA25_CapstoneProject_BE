using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CusomMapOSM_Application.Common;

public static class GeometryValidator
{
    public static string ValidateAndNormalizeGeometry(string coordinatesJson, string geometryType)
    {
        if (string.IsNullOrEmpty(coordinatesJson))
            return coordinatesJson;

        try
        {
            var parsed = JsonSerializer.Deserialize<JsonNode>(coordinatesJson);
            if (parsed == null)
                return coordinatesJson;

            JsonNode geometry;
            if (parsed is JsonObject obj && obj["type"] != null && obj["coordinates"] != null)
            {
                geometry = parsed;
            }
            else
            {
                geometry = new JsonObject
                {
                    ["type"] = geometryType,
                    ["coordinates"] = parsed
                };
            }

            var normalizedGeometry = NormalizeGeometry(geometry, geometryType);
            return JsonSerializer.Serialize(normalizedGeometry);
        }
        catch (JsonException)
        {
            return coordinatesJson;
        }
    }

    private static JsonNode NormalizeGeometry(JsonNode geometry, string geometryType)
    {
        switch (geometryType.ToLowerInvariant())
        {
            case "point":
                return NormalizePoint(geometry);
            case "linestring":
                return NormalizeLineString(geometry);
            case "polygon":
                return NormalizePolygon(geometry);
            case "multipoint":
                return NormalizeMultiPoint(geometry);
            case "multilinestring":
                return NormalizeMultiLineString(geometry);
            case "multipolygon":
                return NormalizeMultiPolygon(geometry);
            case "circle":
                return ConvertCircleToPolygon(geometry);
            case "rectangle":
                return ConvertRectangleToPolygon(geometry);
            default:
                return geometry;
        }
    }

    private static JsonNode NormalizePolygon(JsonNode geometry)
    {
        if (geometry["type"]?.GetValue<string>() != "Polygon")
            return geometry;

        var coordinates = geometry["coordinates"];
        if (coordinates == null || coordinates is not JsonArray)
            return geometry;

        var rings = coordinates.AsArray();
        if (rings.Count == 0)
            return geometry;

        for (int i = 0; i < rings.Count; i++)
        {
            var ring = rings[i];
            if (ring != null && ring is JsonArray)
            {
                rings[i] = NormalizeRing(ring.AsArray());
            }
        }

        return geometry;
    }

    private static JsonNode NormalizeMultiPolygon(JsonNode geometry)
    {
        if (geometry["type"]?.GetValue<string>() != "MultiPolygon")
            return geometry;

        var coordinates = geometry["coordinates"];
        if (coordinates == null || coordinates is not JsonArray)
            return geometry;

        var polygons = coordinates.AsArray();
        for (int i = 0; i < polygons.Count; i++)
        {
            var polygon = polygons[i];
            if (polygon != null && polygon is JsonArray)
            {
                var rings = polygon.AsArray();
                for (int j = 0; j < rings.Count; j++)
                {
                    var ring = rings[j];
                    if (ring != null && ring is JsonArray)
                    {
                        rings[j] = NormalizeRing(ring.AsArray());
                    }
                }
            }
        }

        return geometry;
    }

    private static JsonArray NormalizeRing(JsonArray ring)
    {
        if (ring.Count < 3)
            return ring;

        var normalizedRing = new JsonArray();
        
        for (int i = 0; i < ring.Count; i++)
        {
            normalizedRing.Add(ring[i]?.DeepClone());
        }

        var firstPoint = ring[0];
        var lastPoint = ring[ring.Count - 1];
        
        if (!ArePointsEqual(firstPoint, lastPoint))
        {
            normalizedRing.Add(firstPoint?.DeepClone());
        }

        return normalizedRing;
    }

    private static bool ArePointsEqual(JsonNode? point1, JsonNode? point2)
    {
        if (point1 == null || point2 == null)
            return false;

        if (point1 is not JsonArray || point2 is not JsonArray)
            return false;

        var array1 = point1.AsArray();
        var array2 = point2.AsArray();

        if (array1.Count != array2.Count)
            return false;

        for (int i = 0; i < array1.Count; i++)
        {
            var val1 = array1[i]?.GetValue<double>();
            var val2 = array2[i]?.GetValue<double>();
            
            if (!val1.HasValue || !val2.HasValue || Math.Abs(val1.Value - val2.Value) > 1e-10)
                return false;
        }

        return true;
    }

    private static JsonNode ConvertCircleToPolygon(JsonNode geometry)
    {
        if (geometry["type"]?.GetValue<string>() != "Circle")
            return geometry;

        var coordinates = geometry["coordinates"];
        if (coordinates == null || coordinates is not JsonArray)
            return geometry;

        var coordsArray = coordinates.AsArray();
        if (coordsArray.Count != 3)
            return geometry;

        var longitude = coordsArray[0]?.GetValue<double>() ?? 0;
        var latitude = coordsArray[1]?.GetValue<double>() ?? 0;
        var radiusMeters = coordsArray[2]?.GetValue<double>() ?? 100;

        // Convert radius from meters to degrees (approximate)
        // 1 degree latitude ≈ 111,320 meters
        // 1 degree longitude ≈ 111,320 * cos(latitude) meters
        var latRadius = radiusMeters / 111320.0;
        var lngRadius = radiusMeters / (111320.0 * Math.Cos(latitude * Math.PI / 180.0));

        // Create a polygon approximation of the circle (32 points)
        var points = new JsonArray();
        for (int i = 0; i < 32; i++)
        {
            var angle = 2.0 * Math.PI * i / 32.0;
            var x = longitude + lngRadius * Math.Cos(angle);
            var y = latitude + latRadius * Math.Sin(angle);
            
            var point = new JsonArray();
            point.Add(x);
            point.Add(y);
            points.Add(point);
        }
        
        // Close the polygon by adding the first point again
        var firstPoint = new JsonArray();
        firstPoint.Add(longitude + lngRadius);
        firstPoint.Add(latitude);
        points.Add(firstPoint);

        var ring = new JsonArray();
        ring.Add(points);

        var polygon = new JsonObject
        {
            ["type"] = "Polygon",
            ["coordinates"] = ring
        };

        return polygon;
    }

    private static JsonNode NormalizePoint(JsonNode geometry)
    {
        if (geometry["type"]?.GetValue<string>() != "Point")
            return geometry;

        var coordinates = geometry["coordinates"];
        if (coordinates == null || coordinates is not JsonArray)
            return geometry;

        var coordsArray = coordinates.AsArray();
        if (coordsArray.Count < 2)
            return geometry;

        // Ensure we have at least longitude and latitude
        var normalizedCoords = new JsonArray();
        normalizedCoords.Add(coordsArray[0]?.GetValue<double>() ?? 0);
        normalizedCoords.Add(coordsArray[1]?.GetValue<double>() ?? 0);
        
        // Add altitude if present
        if (coordsArray.Count > 2)
        {
            normalizedCoords.Add(coordsArray[2]?.GetValue<double>() ?? 0);
        }

        return new JsonObject
        {
            ["type"] = "Point",
            ["coordinates"] = normalizedCoords
        };
    }

    private static JsonNode NormalizeLineString(JsonNode geometry)
    {
        if (geometry["type"]?.GetValue<string>() != "LineString")
            return geometry;

        var coordinates = geometry["coordinates"];
        if (coordinates == null || coordinates is not JsonArray)
            return geometry;

        var coordsArray = coordinates.AsArray();
        if (coordsArray.Count < 2)
            return geometry;

        var normalizedCoords = new JsonArray();
        foreach (var coord in coordsArray)
        {
            if (coord is JsonArray point && point.Count >= 2)
            {
                var normalizedPoint = new JsonArray();
                normalizedPoint.Add(point[0]?.GetValue<double>() ?? 0);
                normalizedPoint.Add(point[1]?.GetValue<double>() ?? 0);
                
                // Add altitude if present
                if (point.Count > 2)
                {
                    normalizedPoint.Add(point[2]?.GetValue<double>() ?? 0);
                }
                
                normalizedCoords.Add(normalizedPoint);
            }
        }

        return new JsonObject
        {
            ["type"] = "LineString",
            ["coordinates"] = normalizedCoords
        };
    }

    private static JsonNode NormalizeMultiPoint(JsonNode geometry)
    {
        if (geometry["type"]?.GetValue<string>() != "MultiPoint")
            return geometry;

        var coordinates = geometry["coordinates"];
        if (coordinates == null || coordinates is not JsonArray)
            return geometry;

        var coordsArray = coordinates.AsArray();
        var normalizedCoords = new JsonArray();
        
        foreach (var coord in coordsArray)
        {
            if (coord is JsonArray point && point.Count >= 2)
            {
                var normalizedPoint = new JsonArray();
                normalizedPoint.Add(point[0]?.GetValue<double>() ?? 0);
                normalizedPoint.Add(point[1]?.GetValue<double>() ?? 0);
                
                // Add altitude if present
                if (point.Count > 2)
                {
                    normalizedPoint.Add(point[2]?.GetValue<double>() ?? 0);
                }
                
                normalizedCoords.Add(normalizedPoint);
            }
        }

        return new JsonObject
        {
            ["type"] = "MultiPoint",
            ["coordinates"] = normalizedCoords
        };
    }

    private static JsonNode NormalizeMultiLineString(JsonNode geometry)
    {
        if (geometry["type"]?.GetValue<string>() != "MultiLineString")
            return geometry;

        var coordinates = geometry["coordinates"];
        if (coordinates == null || coordinates is not JsonArray)
            return geometry;

        var coordsArray = coordinates.AsArray();
        var normalizedCoords = new JsonArray();
        
        foreach (var lineString in coordsArray)
        {
            if (lineString is JsonArray lineCoords)
            {
                var normalizedLineString = new JsonArray();
                foreach (var coord in lineCoords)
                {
                    if (coord is JsonArray point && point.Count >= 2)
                    {
                        var normalizedPoint = new JsonArray();
                        normalizedPoint.Add(point[0]?.GetValue<double>() ?? 0);
                        normalizedPoint.Add(point[1]?.GetValue<double>() ?? 0);
                        
                        // Add altitude if present
                        if (point.Count > 2)
                        {
                            normalizedPoint.Add(point[2]?.GetValue<double>() ?? 0);
                        }
                        
                        normalizedLineString.Add(normalizedPoint);
                    }
                }
                normalizedCoords.Add(normalizedLineString);
            }
        }

        return new JsonObject
        {
            ["type"] = "MultiLineString",
            ["coordinates"] = normalizedCoords
        };
    }

    private static JsonNode ConvertRectangleToPolygon(JsonNode geometry)
    {
        if (geometry["type"]?.GetValue<string>() != "Rectangle")
            return geometry;

        var coordinates = geometry["coordinates"];
        if (coordinates == null || coordinates is not JsonArray)
            return geometry;

        var coordsArray = coordinates.AsArray();
        if (coordsArray.Count != 4)
            return geometry;

        // Rectangle coordinates: [minLng, minLat, maxLng, maxLat]
        var minLng = coordsArray[0]?.GetValue<double>() ?? 0;
        var minLat = coordsArray[1]?.GetValue<double>() ?? 0;
        var maxLng = coordsArray[2]?.GetValue<double>() ?? 0;
        var maxLat = coordsArray[3]?.GetValue<double>() ?? 0;

        // Create rectangle polygon coordinates
        var rectangleCoords = new JsonArray();
        
        // Bottom-left
        var point1 = new JsonArray();
        point1.Add(minLng);
        point1.Add(minLat);
        rectangleCoords.Add(point1);
        
        // Bottom-right
        var point2 = new JsonArray();
        point2.Add(maxLng);
        point2.Add(minLat);
        rectangleCoords.Add(point2);
        
        // Top-right
        var point3 = new JsonArray();
        point3.Add(maxLng);
        point3.Add(maxLat);
        rectangleCoords.Add(point3);
        
        // Top-left
        var point4 = new JsonArray();
        point4.Add(minLng);
        point4.Add(maxLat);
        rectangleCoords.Add(point4);
        
        // Close the rectangle
        var point5 = new JsonArray();
        point5.Add(minLng);
        point5.Add(minLat);
        rectangleCoords.Add(point5);

        var ring = new JsonArray();
        ring.Add(rectangleCoords);

        var polygon = new JsonObject
        {
            ["type"] = "Polygon",
            ["coordinates"] = ring
        };

        return polygon;
    }
}
