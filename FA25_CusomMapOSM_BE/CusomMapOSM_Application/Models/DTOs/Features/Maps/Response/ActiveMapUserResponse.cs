namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;

public class ActiveMapUserResponse
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserAvatar { get; set; } = string.Empty;
    public string HighlightColor { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public DateTime LastActiveAt { get; set; }
    public bool IsIdle { get; set; } // True if inactive > 2 minutes
    public MapSelectionResponse? CurrentSelection { get; set; }
}