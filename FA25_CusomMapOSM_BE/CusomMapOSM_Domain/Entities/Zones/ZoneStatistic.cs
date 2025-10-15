using System;
using CusomMapOSM_Domain.Entities.Zones.Enums;

namespace CusomMapOSM_Domain.Entities.Zones;

public class ZoneStatistic
{
    public Guid ZoneStatisticId { get; set; }
    public Guid ZoneId { get; set; }
    public ZoneMetricType MetricType { get; set; }
    public double? NumericValue { get; set; }
    public string? TextValue { get; set; }                     // Non-numeric value fallback
    public string? Unit { get; set; }
    public int? Year { get; set; }
    public int? Quarter { get; set; }
    public string? Source { get; set; }
    public string? Metadata { get; set; }                      // Additional JSON metadata
    public DateTime CollectedAt { get; set; }

    public Zone? Zone { get; set; }
}
