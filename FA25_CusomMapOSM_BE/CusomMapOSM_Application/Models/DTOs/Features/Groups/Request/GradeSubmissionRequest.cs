namespace CusomMapOSM_Application.Models.DTOs.Features.Groups.Request;

public class GradeSubmissionRequest
{
    public Guid SubmissionId { get; set; }
    public int Score { get; set; }
    public string? Feedback { get; set; }
}
