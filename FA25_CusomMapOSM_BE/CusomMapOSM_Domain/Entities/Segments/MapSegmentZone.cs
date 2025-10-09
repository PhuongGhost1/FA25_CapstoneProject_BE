using System;
using CusomMapOSM_Domain.Entities.Segments.Enums;

namespace CusomMapOSM_Domain.Entities.Segments;

public class MapSegmentZone
{
    public Guid SegmentZoneId { get; set; }
    public Guid SegmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SegmentZoneType ZoneType { get; set; } = SegmentZoneType.Area;
    public string ZoneGeometry { get; set; } = string.Empty;   // GeoJSON polygon/line
    public string? FocusCameraState { get; set; }              // JSON: zoom, bearing, pitch
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public MapSegment? Segment { get; set; }
}
