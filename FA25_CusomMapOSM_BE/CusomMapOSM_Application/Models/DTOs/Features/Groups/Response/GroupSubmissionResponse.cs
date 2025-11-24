namespace CusomMapOSM_Application.Models.DTOs.Features.Groups.Response;

public class GroupSubmissionResponse
{
    public Guid SubmissionId { get; set; }
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public List<string>? AttachmentUrls { get; set; }
    public int? Score { get; set; }
    public string? Feedback { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? GradedAt { get; set; }
}
