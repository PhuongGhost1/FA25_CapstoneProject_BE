namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;

public class UpdateSelectionRequest
{
    public Guid MapId { get; set; }
    public string SelectionType { get; set; } = string.Empty;
    public string? SelectedObjectId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}