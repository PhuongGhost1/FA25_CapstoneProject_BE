using CusomMapOSM_Application.Interfaces.Services.FileProcessors;
using CusomMapOSM_Application.Interfaces.Services.GeoJson;
using CusomMapOSM_Domain.Entities.Layers.Enums;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Text;
using System.Text.Json;
using CusomMapOSM_Application.Models.DTOs.Features.FileProcessor.Response;

namespace CusomMapOSM_Infrastructure.Services.FileProcessors;

public class SpreadsheetProcessor : ISpreadsheetProcessor
{
    private readonly IGeoJsonService _geoJsonService;

    public SpreadsheetProcessor(IGeoJsonService geoJsonService)
    {
        _geoJsonService = geoJsonService;
    }

    public async Task<FileProcessingResult> ProcessCSV(IFormFile file, string layerName, SpreadsheetConfig config)
    {
        try
        {
            string csvContent;
            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            {
                csvContent = await reader.ReadToEndAsync();
            }

            var geoJsonContent = await ConvertCsvToGeoJson(csvContent, config);
            return await ProcessGeoJsonContent(geoJsonContent, layerName, LayerTypeEnum.CSV);
        }
        catch (Exception ex)
        {
            return new FileProcessingResult
            {
                Success = false,
                ErrorMessage = $"CSV processing failed: {ex.Message}"
            };
        }
    }

    public async Task<FileProcessingResult> ProcessExcel(IFormFile file, string layerName, SpreadsheetConfig config)
    {
        // Note: Excel processing requires a library like EPPlus or ClosedXML
        // This is a placeholder implementation
        
        return new FileProcessingResult
        {
            Success = false,
            ErrorMessage = "Excel processing requires additional library implementation. Please convert to CSV format."
        };
    }

    public bool SupportsFormat(string fileExtension)
    {
        var supportedFormats = new[] { ".csv", ".xlsx", ".xls" };
        return supportedFormats.Contains(fileExtension.ToLower());
    }

    private async Task<string> ConvertCsvToGeoJson(string csvContent, SpreadsheetConfig config)
    {
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
            throw new ArgumentException("CSV file is empty");

        string[] headers = new string[0];
        int startRow = 0;

        if (config.HasHeaders)
        {
            headers = ParseCsvLine(lines[0]);
            startRow = 1;
        }

        // Auto-detect coordinate columns if not specified
        if (string.IsNullOrEmpty(config.LatitudeColumn) || string.IsNullOrEmpty(config.LongitudeColumn))
        {
            var (latCol, lonCol) = AutoDetectCoordinateColumns(headers);
            config.LatitudeColumn = latCol;
            config.LongitudeColumn = lonCol;
        }

        var latIndex = Array.IndexOf(headers, config.LatitudeColumn);
        var lonIndex = Array.IndexOf(headers, config.LongitudeColumn);
        var nameIndex = Array.IndexOf(headers, config.NameColumn ?? "name");

        if (latIndex == -1 || lonIndex == -1)
        {
            throw new ArgumentException($"Could not find coordinate columns: {config.LatitudeColumn}, {config.LongitudeColumn}");
        }

        var features = new List<object>();

        for (int i = startRow; i < lines.Length; i++)
        {
            try
            {
                var values = ParseCsvLine(lines[i]);
                if (values.Length <= Math.Max(latIndex, lonIndex))
                    continue;

                if (!double.TryParse(values[latIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) ||
                    !double.TryParse(values[lonIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out var lon))
                    continue;

                // Validate coordinate ranges
                if (lat < -90 || lat > 90 || lon < -180 || lon > 180)
                    continue;

                // Create properties object
                var properties = new Dictionary<string, object>();
                for (int j = 0; j < Math.Min(headers.Length, values.Length); j++)
                {
                    if (j != latIndex && j != lonIndex) // Don't duplicate coordinates in properties
                    {
                        properties[headers[j]] = values[j];
                    }
                }

                // Set name if available
                if (nameIndex >= 0 && nameIndex < values.Length)
                {
                    properties["name"] = values[nameIndex];
                }

                features.Add(new
                {
                    type = "Feature",
                    properties = properties,
                    geometry = new
                    {
                        type = "Point",
                        coordinates = new[] { lon, lat }
                    }
                });
            }
            catch (Exception ex)
            {
                // Skip invalid rows
                Console.WriteLine($"Skipping row {i}: {ex.Message}");
                continue;
            }
        }

        var geoJson = new
        {
            type = "FeatureCollection",
            features = features
        };

        return JsonSerializer.Serialize(geoJson, new JsonSerializerOptions { WriteIndented = false });
    }

    private string[] ParseCsvLine(string line)
    {
        // Simple CSV parser - in production, use a proper CSV library like CsvHelper
        var values = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        values.Add(current.ToString().Trim());
        return values.ToArray();
    }

    private (string? lat, string? lon) AutoDetectCoordinateColumns(string[] headers)
    {
        var latPatterns = new[] { "lat", "latitude", "y", "northing" };
        var lonPatterns = new[] { "lon", "lng", "longitude", "x", "easting" };

        var latColumn = headers.FirstOrDefault(h => 
            latPatterns.Any(p => h.ToLower().Contains(p)));
        var lonColumn = headers.FirstOrDefault(h => 
            lonPatterns.Any(p => h.ToLower().Contains(p)));

        return (latColumn, lonColumn);
    }

    private async Task<FileProcessingResult> ProcessGeoJsonContent(string geoJsonContent, string layerName, LayerTypeEnum layerType)
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
}
