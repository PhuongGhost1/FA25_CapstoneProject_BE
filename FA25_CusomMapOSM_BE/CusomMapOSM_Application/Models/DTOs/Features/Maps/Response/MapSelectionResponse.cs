namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;

public class MapSelectionResponse
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserAvatar { get; set; } = string.Empty;
    public string SelectionType { get; set; } = string.Empty; // "Layer", "Point", "Line", "Polygon", "Marker"
    public string? SelectedObjectId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateTime SelectedAt { get; set; }
    public string HighlightColor { get; set; } = string.Empty; // Color for UI highlight
}