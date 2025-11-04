using CusomMapOSM_Application.Interfaces.Services.FileProcessors;
using CusomMapOSM_Application.Interfaces.Services.GeoJson;
using CusomMapOSM_Domain.Entities.Layers.Enums;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using CusomMapOSM_Application.Models.DTOs.Features.FileProcessor.Response;

namespace CusomMapOSM_Infrastructure.Services.FileProcessors;

public class VectorProcessor : IVectorProcessor
{
    private readonly IGeoJsonService _geoJsonService;

    public VectorProcessor(IGeoJsonService geoJsonService)
    {
        _geoJsonService = geoJsonService;
    }

    public async Task<FileProcessingResult> ProcessGeoJSON(IFormFile file, string layerName)
    {
        try
        {
            string content;
            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            {
                content = await reader.ReadToEndAsync();
            }

            var processed = _geoJsonService.ProcessGeoJsonUpload(content, layerName);
            
            return new FileProcessingResult
            {
                Success = processed.IsValid,
                ErrorMessage = processed.ValidationErrors,
                LayerType = LayerType.GEOJSON,
                LayerData = processed.LayerData,
                FeatureCount = processed.FeatureCount,
                DataSizeKB = processed.DataSizeKB,
                DataBounds = processed.DataBounds,
                LayerStyle = processed.LayerStyle,
                GeometryType = processed.GeometryType,
                PropertyNames = processed.PropertyNames
            };
        }
        catch (Exception ex)
        {
            return new FileProcessingResult
            {
                Success = false,
                ErrorMessage = $"GeoJSON processing failed: {ex.Message}"
            };
        }
    }

    public async Task<FileProcessingResult> ProcessKML(IFormFile file, string layerName)
    {
        try
        {
            string kmlContent;
            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            {
                kmlContent = await reader.ReadToEndAsync();
            }

            // Convert KML to GeoJSON
            var geoJsonContent = await ConvertKmlToGeoJson(kmlContent);
            
            // Use existing GeoJSON processor
            return await ProcessGeoJsonContent(geoJsonContent, layerName, LayerType.KML);
        }
        catch (Exception ex)
        {
            return new FileProcessingResult
            {
                Success = false,
                ErrorMessage = $"KML processing failed: {ex.Message}"
            };
        }
    }

    public async Task<FileProcessingResult> ProcessGPX(IFormFile file, string layerName)
    {
        try
        {
            string gpxContent;
            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            {
                gpxContent = await reader.ReadToEndAsync();
            }

            // Convert GPX to GeoJSON
            var geoJsonContent = await ConvertGpxToGeoJson(gpxContent);
            
            return await ProcessGeoJsonContent(geoJsonContent, layerName, LayerType.GPX);
        }
        catch (Exception ex)
        {
            return new FileProcessingResult
            {
                Success = false,
                ErrorMessage = $"GPX processing failed: {ex.Message}"
            };
        }
    }

    public async Task<FileProcessingResult> ProcessShapefile(IFormFile file, string layerName)
    {
        // Note: Shapefile processing requires multiple files (.shp, .shx, .dbf, .prj)
        // This is a simplified implementation - in production, you'd need to handle ZIP files
        // containing all shapefile components and use a library like NetTopologySuite
        
        return new FileProcessingResult
        {
            Success = false,
            ErrorMessage = "Shapefile processing requires additional implementation. Please convert to GeoJSON format."
        };
    }

    public bool SupportsFormat(string fileExtension)
    {
        var supportedFormats = new[] { ".geojson", ".json", ".kml", ".gpx", ".shp" };
        return supportedFormats.Contains(fileExtension.ToLower());
    }

    private async Task<FileProcessingResult> ProcessGeoJsonContent(string geoJsonContent, string layerName, LayerType layerType)
    {
        var processed = _geoJsonService.ProcessGeoJsonUpload(geoJsonContent, layerName);
        
        return new FileProcessingResult
        {
            Success = processed.IsValid,
            ErrorMessage = processed.ValidationErrors,
            LayerType = layerType,
            LayerData = processed.LayerData,
            FeatureCount = processed.FeatureCount,
            DataSizeKB = processed.DataSizeKB,
            DataBounds = processed.DataBounds,
            LayerStyle = processed.LayerStyle,
            GeometryType = processed.GeometryType,
            PropertyNames = processed.PropertyNames
        };
    }

    private async Task<string> ConvertKmlToGeoJson(string kmlContent)
    {
        // Simplified KML to GeoJSON conversion
        // In production, use a proper library like SharpKml or NetTopologySuite
        
        var doc = XDocument.Parse(kmlContent);
        var ns = doc.Root?.GetDefaultNamespace();
        
        var features = new List<object>();
        
        // Extract placemarks
        var placemarks = doc.Descendants(ns + "Placemark");
        
        foreach (var placemark in placemarks)
        {
            var name = placemark.Element(ns + "name")?.Value ?? "Unnamed";
            var description = placemark.Element(ns + "description")?.Value ?? "";
            
            // Handle Point
            var point = placemark.Descendants(ns + "Point").FirstOrDefault();
            if (point != null)
            {
                var coordinates = point.Element(ns + "coordinates")?.Value?.Trim();
                if (!string.IsNullOrEmpty(coordinates))
                {
                    var coords = coordinates.Split(',').Select(double.Parse).ToArray();
                    features.Add(new
                    {
                        type = "Feature",
                        properties = new { name, description },
                        geometry = new
                        {
                            type = "Point",
                            coordinates = new[] { coords[0], coords[1] }
                        }
                    });
                }
            }
            
            // Handle LineString, Polygon etc. (simplified)
            // In production, implement full KML geometry support
        }

        var geoJson = new
        {
            type = "FeatureCollection",
            features = features
        };

        return JsonSerializer.Serialize(geoJson, new JsonSerializerOptions { WriteIndented = false });
    }

    private async Task<string> ConvertGpxToGeoJson(string gpxContent)
    {
        // Simplified GPX to GeoJSON conversion
        // In production, use a proper GPX library
        
        var doc = XDocument.Parse(gpxContent);
        var ns = doc.Root?.GetDefaultNamespace();
        
        var features = new List<object>();
        
        // Extract waypoints
        var waypoints = doc.Descendants(ns + "wpt");
        foreach (var wpt in waypoints)
        {
            var lat = double.Parse(wpt.Attribute("lat")?.Value ?? "0");
            var lon = double.Parse(wpt.Attribute("lon")?.Value ?? "0");
            var name = wpt.Element(ns + "name")?.Value ?? "Waypoint";
            
            features.Add(new
            {
                type = "Feature",
                properties = new { name, type = "waypoint" },
                geometry = new
                {
                    type = "Point",
                    coordinates = new[] { lon, lat }
                }
            });
        }
        
        // Extract tracks
        var tracks = doc.Descendants(ns + "trk");
        foreach (var track in tracks)
        {
            var trackName = track.Element(ns + "name")?.Value ?? "Track";
            var trackSegs = track.Descendants(ns + "trkseg");
            
            foreach (var seg in trackSegs)
            {
                var points = seg.Descendants(ns + "trkpt")
                    .Select(pt => new[]
                    {
                        double.Parse(pt.Attribute("lon")?.Value ?? "0"),
                        double.Parse(pt.Attribute("lat")?.Value ?? "0")
                    }).ToArray();
                
                if (points.Length > 1)
                {
                    features.Add(new
                    {
                        type = "Feature",
                        properties = new { name = trackName, type = "track" },
                        geometry = new
                        {
                            type = "LineString",
                            coordinates = points
                        }
                    });
                }
            }
        }

        var geoJson = new
        {
            type = "FeatureCollection",
            features = features
        };

        return JsonSerializer.Serialize(geoJson, new JsonSerializerOptions { WriteIndented = false });
    }
}
