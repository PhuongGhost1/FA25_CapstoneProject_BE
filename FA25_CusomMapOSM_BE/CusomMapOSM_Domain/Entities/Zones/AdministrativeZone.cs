using System;
using CusomMapOSM_Domain.Entities.Zones.Enums;

namespace CusomMapOSM_Domain.Entities.Zones;

public class AdministrativeZone
{
    public Guid ZoneId { get; set; }
    public string ExternalId { get; set; } = string.Empty;     // e.g. OSM id or national code
    public string ZoneCode { get; set; } = string.Empty;       // e.g. province code
    public string Name { get; set; } = string.Empty;
    public ZoneAdminLevel AdminLevel { get; set; }
    public Guid? ParentZoneId { get; set; }
    public string Geometry { get; set; } = string.Empty;       // GeoJSON
    public string? SimplifiedGeometry { get; set; }
    public string? Centroid { get; set; }                      // GeoJSON point
    public string? BoundingBox { get; set; }                   // GeoJSON polygon
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public AdministrativeZone? ParentZone { get; set; }
}
