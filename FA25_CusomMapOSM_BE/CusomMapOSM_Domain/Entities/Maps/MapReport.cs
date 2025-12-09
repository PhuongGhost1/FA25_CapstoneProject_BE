using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Maps;

public class MapReport
{
    public Guid MapReportId { get; set; }
    public Guid MapId { get; set; }
    public Guid? ReporterUserId { get; set; }
    public string ReporterEmail { get; set; } = string.Empty;
    public string ReporterName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MapReportStatusEnum Status { get; set; } = MapReportStatusEnum.Pending;
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Map? Map { get; set; }
    public User? ReporterUser { get; set; }
    public User? ReviewedByUser { get; set; }
}

