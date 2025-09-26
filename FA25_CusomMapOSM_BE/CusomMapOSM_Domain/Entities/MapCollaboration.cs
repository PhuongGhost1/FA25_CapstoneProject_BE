namespace CusomMapOSM_Domain.Entities;

public class MapCollaboration
{
    public string MapId { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public DateTime JoinedAt { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public string? CurrentAction { get; set; }
    public double? CursorLat { get; set; }
    public double? CursorLng { get; set; }
}
