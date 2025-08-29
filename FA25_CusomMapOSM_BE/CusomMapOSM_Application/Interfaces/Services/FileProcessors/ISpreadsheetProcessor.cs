using Microsoft.AspNetCore.Http;

namespace CusomMapOSM_Application.Interfaces.Services.FileProcessors;

public interface ISpreadsheetProcessor
{
    Task<FileProcessingResult> ProcessCSV(IFormFile file, string layerName, SpreadsheetConfig config);
    Task<FileProcessingResult> ProcessExcel(IFormFile file, string layerName, SpreadsheetConfig config);
    bool SupportsFormat(string fileExtension);
}

public class SpreadsheetConfig
{
    public string? LatitudeColumn { get; set; } = "latitude";
    public string? LongitudeColumn { get; set; } = "longitude";
    public string? NameColumn { get; set; } = "name";
    public bool HasHeaders { get; set; } = true;
    public int? SheetIndex { get; set; } = 0; // For Excel
    public string? AddressColumn { get; set; } // For geocoding
}
