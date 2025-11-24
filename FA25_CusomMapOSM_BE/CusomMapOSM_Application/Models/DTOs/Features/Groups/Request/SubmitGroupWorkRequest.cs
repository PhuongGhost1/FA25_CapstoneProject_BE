namespace CusomMapOSM_Application.Models.DTOs.Features.Groups.Request;

public class SubmitGroupWorkRequest
{
    public Guid GroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public List<string>? AttachmentUrls { get; set; }
}
