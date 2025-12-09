namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;

public class ReviewReportRequest
{
    public int Status { get; set; }
    public string? ReviewNotes { get; set; }
    public bool ShouldDeleteMap { get; set; } = false; // Nếu true, sẽ xóa map khi status = Resolved
}

