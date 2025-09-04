using CusomMapOSM_Application.Models.DTOs.Features.FileProcessor.Response;
using Microsoft.AspNetCore.Http;

namespace CusomMapOSM_Application.Interfaces.Services.FileProcessors;

public interface ISpreadsheetProcessor
{
    Task<FileProcessingResult> ProcessCSV(IFormFile file, string layerName, SpreadsheetConfig config);
    Task<FileProcessingResult> ProcessExcel(IFormFile file, string layerName, SpreadsheetConfig config);
    bool SupportsFormat(string fileExtension);
}
