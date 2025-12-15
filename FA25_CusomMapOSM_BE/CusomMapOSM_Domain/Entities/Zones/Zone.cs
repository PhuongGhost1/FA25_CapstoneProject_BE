using System;
using CusomMapOSM_Domain.Entities.Zones.Enums;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Segments.Enums;
namespace CusomMapOSM_Domain.Entities.Zones;

public class Zone
{
    public Guid ZoneId { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string ZoneCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ZoneType ZoneType { get; set; }
    public ZoneAdminLevel AdminLevel { get; set; }
    public Guid? ParentZoneId { get; set; }
    public string Geometry { get; set; } = string.Empty;
    public string? SimplifiedGeometry { get; set; }
    public string? Centroid { get; set; }
    public string? BoundingBox { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Zone? ParentZone { get; set; }
}