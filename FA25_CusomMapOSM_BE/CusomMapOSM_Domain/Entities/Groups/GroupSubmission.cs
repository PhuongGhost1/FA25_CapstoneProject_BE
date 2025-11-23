namespace CusomMapOSM_Domain.Entities.Groups;

public class GroupSubmission
{
    public Guid SubmissionId { get; set; }
    public Guid GroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? AttachmentUrls { get; set; }  // JSON array of URLs
    public int? Score { get; set; }
    public string? Feedback { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? GradedAt { get; set; }
    public SessionGroup? Group { get; set; }
}
