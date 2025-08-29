using Microsoft.AspNetCore.Http;

namespace CusomMapOSM_Application.Interfaces.Services.FileProcessors;

public interface IVectorProcessor
{
    Task<FileProcessingResult> ProcessShapefile(IFormFile file, string layerName);
    Task<FileProcessingResult> ProcessKML(IFormFile file, string layerName);
    Task<FileProcessingResult> ProcessGPX(IFormFile file, string layerName);
    Task<FileProcessingResult> ProcessGeoJSON(IFormFile file, string layerName);
    bool SupportsFormat(string fileExtension);
}
