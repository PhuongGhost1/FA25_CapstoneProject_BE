using System;
using CusomMapOSM_Domain.Entities.Zones.Enums;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Segments.Enums;

namespace CusomMapOSM_Domain.Entities.Zones;

public class Zone
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
    public Guid? SegmentId { get; set; }
    public string? Description { get; set; }
    public ZoneType ZoneType { get; set; }
    public string? FocusCameraState { get; set; }              // JSON: zoom, bearing, pitch
    public int DisplayOrder { get; set; } = 0;
    public bool IsPrimary { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Zone? ParentZone { get; set; }
    public Segment? Segment { get; set; }
}
