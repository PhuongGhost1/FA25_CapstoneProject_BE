namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;

public class ReportMapRequest
{
    public Guid MapId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ReporterEmail { get; set; }
    public string? ReporterName { get; set; }
}

