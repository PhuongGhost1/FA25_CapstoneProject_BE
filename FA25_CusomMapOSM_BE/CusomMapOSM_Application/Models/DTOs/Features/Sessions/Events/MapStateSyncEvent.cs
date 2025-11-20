namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Events;

public class MapStateSyncEvent
{
    public Guid SessionId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int ZoomLevel { get; set; }
    public decimal? Bearing { get; set; }
    public decimal? Pitch { get; set; }
    public string SyncedBy { get; set; } = string.Empty;
    public DateTime SyncedAt { get; set; }
}