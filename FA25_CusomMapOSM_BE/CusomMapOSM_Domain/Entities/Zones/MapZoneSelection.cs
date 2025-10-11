using System;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Zones;

public class MapZoneSelection
{
    public Guid MapZoneSelectionId { get; set; }
    public Guid MapId { get; set; }
    public Guid CreatedBy { get; set; }
    public string SelectionGeometry { get; set; } = string.Empty; // GeoJSON polygon
    public string? IncludedZoneIds { get; set; }                  // JSON array
    public bool PersistResults { get; set; }
    public string? Summary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Map? Map { get; set; }
    public User? Creator { get; set; }
}
