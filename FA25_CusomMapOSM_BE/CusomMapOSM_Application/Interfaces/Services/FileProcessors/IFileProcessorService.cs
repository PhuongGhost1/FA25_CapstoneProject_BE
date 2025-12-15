using CusomMapOSM_Application.Models.DTOs.Features.FileProcessor.Response;
using Microsoft.AspNetCore.Http;
using CusomMapOSM_Domain.Entities.Layers.Enums;

namespace CusomMapOSM_Application.Interfaces.Services.FileProcessors;

public interface IFileProcessorService
{
    Task<FileProcessingResult> ProcessUploadedFile(IFormFile file, string layerName);
    LayerType DetectFileType(string fileName);
    bool IsSupported(string fileName);
}
