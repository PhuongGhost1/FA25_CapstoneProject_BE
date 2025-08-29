using Microsoft.AspNetCore.Http;
using CusomMapOSM_Domain.Entities.Layers.Enums;

namespace CusomMapOSM_Application.Interfaces.Services.FileProcessors;

public interface IFileProcessorService
{
    Task<FileProcessingResult> ProcessUploadedFile(IFormFile file, string layerName);
    LayerTypeEnum DetectFileType(string fileName, string contentType);
    bool IsSupported(string fileName, string contentType);
}

public class FileProcessingResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public LayerTypeEnum LayerType { get; set; }
    public string? LayerData { get; set; }  // GeoJSON for vector, null for raster
    public string? FilePath { get; set; }   // File path for raster/large files
    public string? TileUrlTemplate { get; set; }  // For raster tiles: /tiles/{z}/{x}/{y}.png
    public int FeatureCount { get; set; }
    public double DataSizeKB { get; set; }
    public string? DataBounds { get; set; }
    public string? LayerStyle { get; set; }
    public string? GeometryType { get; set; }
    public List<string> PropertyNames { get; set; } = new();
    public Dictionary<string, object>? Metadata { get; set; }
}
