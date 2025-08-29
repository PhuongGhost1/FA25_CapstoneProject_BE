using CusomMapOSM_Application.Interfaces.Services.FileProcessors;
using CusomMapOSM_Domain.Entities.Layers.Enums;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace CusomMapOSM_Infrastructure.Services.FileProcessors;

public class RasterProcessor : IRasterProcessor
{
    private readonly string _uploadsPath;
    private readonly string _tilesPath;

    public RasterProcessor(IConfiguration configuration)
    {
        _uploadsPath = configuration["FileStorage:UploadsPath"] ?? "uploads/rasters";
        _tilesPath = configuration["FileStorage:TilesPath"] ?? "uploads/tiles";
        
        // Ensure directories exist
        Directory.CreateDirectory(_uploadsPath);
        Directory.CreateDirectory(_tilesPath);
    }

    public async Task<FileProcessingResult> ProcessGeoTIFF(IFormFile file, string layerName)
    {
        try
        {
            // Save the uploaded file
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(_uploadsPath, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Extract metadata and bounds (requires GDAL library in production)
            var bounds = await ExtractBounds(filePath);
            var metadata = await ExtractMetadata(filePath);
            
            // Generate tile directory
            var tileDir = Path.Combine(_tilesPath, Path.GetFileNameWithoutExtension(fileName));
            var tileUrlTemplate = await GenerateTiles(filePath, tileDir);

            return new FileProcessingResult
            {
                Success = true,
                LayerType = LayerTypeEnum.GeoTIFF,
                FilePath = filePath,
                TileUrlTemplate = tileUrlTemplate,
                DataBounds = bounds,
                DataSizeKB = file.Length / 1024.0,
                FeatureCount = 1, // Raster is single "feature"
                GeometryType = "Raster",
                PropertyNames = new List<string> { "raster_data" },
                Metadata = metadata
            };
        }
        catch (Exception ex)
        {
            return new FileProcessingResult
            {
                Success = false,
                ErrorMessage = $"GeoTIFF processing failed: {ex.Message}"
            };
        }
    }

    public async Task<FileProcessingResult> ProcessImageFile(IFormFile file, string layerName, double[] bounds)
    {
        try
        {
            // For non-georeferenced images, user must provide bounds
            if (bounds.Length != 4)
            {
                return new FileProcessingResult
                {
                    Success = false,
                    ErrorMessage = "Image files require bounds [minLon, minLat, maxLon, maxLat]"
                };
            }

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(_uploadsPath, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create bounds GeoJSON
            var boundsGeoJson = CreateBoundsGeoJson(bounds);

            var layerType = Path.GetExtension(file.FileName).ToLower() switch
            {
                ".png" => LayerTypeEnum.PNG,
                ".jpg" or ".jpeg" => LayerTypeEnum.JPG,
                _ => LayerTypeEnum.PNG
            };

            return new FileProcessingResult
            {
                Success = true,
                LayerType = layerType,
                FilePath = filePath,
                DataBounds = boundsGeoJson,
                DataSizeKB = file.Length / 1024.0,
                FeatureCount = 1,
                GeometryType = "Raster",
                PropertyNames = new List<string> { "image_data" },
                Metadata = new Dictionary<string, object>
                {
                    ["image_width"] = 0, // Would need image processing library to get actual dimensions
                    ["image_height"] = 0,
                    ["bounds"] = bounds
                }
            };
        }
        catch (Exception ex)
        {
            return new FileProcessingResult
            {
                Success = false,
                ErrorMessage = $"Image processing failed: {ex.Message}"
            };
        }
    }

    public async Task<string> GenerateTiles(string rasterFilePath, string outputDir)
    {
        // This is a placeholder - in production, use GDAL to generate map tiles
        // GDAL command example: gdal2tiles.py -z 0-18 input.tif output_dir/
        
        Directory.CreateDirectory(outputDir);
        
        // For now, return a template URL that would work with generated tiles
        var tileDirName = Path.GetFileName(outputDir);
        return $"/api/tiles/{tileDirName}/{{z}}/{{x}}/{{y}}.png";
    }

    public async Task<string?> ExtractBounds(string rasterFilePath)
    {
        // Placeholder - in production, use GDAL to extract bounds
        // For now, return Vietnam bounds as example
        var bounds = new double[] { 102.14441, 8.37994, 109.52848, 23.39327 };
        return CreateBoundsGeoJson(bounds);
    }

    public bool SupportsFormat(string fileExtension)
    {
        var supportedFormats = new[] { ".tif", ".tiff", ".png", ".jpg", ".jpeg" };
        return supportedFormats.Contains(fileExtension.ToLower());
    }

    private async Task<Dictionary<string, object>?> ExtractMetadata(string filePath)
    {
        // Placeholder - in production, extract GDAL metadata
        return new Dictionary<string, object>
        {
            ["file_size"] = new FileInfo(filePath).Length,
            ["format"] = Path.GetExtension(filePath),
            ["processed_at"] = DateTime.UtcNow
        };
    }

    private string CreateBoundsGeoJson(double[] bounds)
    {
        // bounds = [minLon, minLat, maxLon, maxLat]
        var polygon = new
        {
            type = "Polygon",
            coordinates = new[]
            {
                new[]
                {
                    new[] { bounds[0], bounds[1] }, // bottom-left
                    new[] { bounds[2], bounds[1] }, // bottom-right  
                    new[] { bounds[2], bounds[3] }, // top-right
                    new[] { bounds[0], bounds[3] }, // top-left
                    new[] { bounds[0], bounds[1] }  // close polygon
                }
            }
        };

        return JsonSerializer.Serialize(polygon, new JsonSerializerOptions { WriteIndented = false });
    }
}
