namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;

public class MapReportDto
{
    public Guid ReportId { get; set; }
    public Guid MapId { get; set; }
    public string? MapName { get; set; }
    public Guid? ReporterUserId { get; set; }
    public string ReporterEmail { get; set; } = string.Empty;
    public string ReporterName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Status { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public Guid? ReviewedByUserId { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

