namespace CusomMapOSM_Application.Models.DTOs.Features.FileProcessor.Response;

public class SpreadsheetConfig
{
    public string? LatitudeColumn { get; set; } = "latitude";
    public string? LongitudeColumn { get; set; } = "longitude";
    public string? NameColumn { get; set; } = "name";
    public bool HasHeaders { get; set; } = true;
    public int? SheetIndex { get; set; } = 0; // For Excel
    public string? AddressColumn { get; set; } // For geocoding
}