using CusomMapOSM_Domain.Entities.Layers.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.FileProcessor.Response;

public class FileProcessingResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public LayerTypeEnum LayerType { get; set; }
    public string? LayerData { get; set; }
    public string? FilePath { get; set; }
    public string? TileUrlTemplate { get; set; }
    public int FeatureCount { get; set; }
    public double DataSizeKB { get; set; }
    public string? DataBounds { get; set; }
    public string? LayerStyle { get; set; }
    public string? GeometryType { get; set; }
    public List<string> PropertyNames { get; set; } = new();
    public Dictionary<string, object>? Metadata { get; set; }
}