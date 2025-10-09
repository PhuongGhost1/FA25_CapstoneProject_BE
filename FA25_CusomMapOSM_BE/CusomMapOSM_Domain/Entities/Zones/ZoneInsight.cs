using System;
using CusomMapOSM_Domain.Entities.Zones.Enums;

namespace CusomMapOSM_Domain.Entities.Zones;

public class ZoneInsight
{
    public Guid ZoneInsightId { get; set; }
    public Guid ZoneId { get; set; }
    public ZoneInsightType InsightType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? ExternalUrl { get; set; }
    public string? Location { get; set; }                      // GeoJSON point or address
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public AdministrativeZone? Zone { get; set; }
}
