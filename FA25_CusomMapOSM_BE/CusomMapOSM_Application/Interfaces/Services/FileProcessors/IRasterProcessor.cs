using CusomMapOSM_Application.Models.DTOs.Features.FileProcessor.Response;
using Microsoft.AspNetCore.Http;

namespace CusomMapOSM_Application.Interfaces.Services.FileProcessors;

public interface IRasterProcessor
{
    Task<FileProcessingResult> ProcessGeoTIFF(IFormFile file, string layerName);
    Task<FileProcessingResult> ProcessImageFile(IFormFile file, string layerName, double[] bounds);
    Task<string> GenerateTiles(string rasterFilePath, string outputDir);
    Task<string?> ExtractBounds(string rasterFilePath);
    bool SupportsFormat(string fileExtension);
}
