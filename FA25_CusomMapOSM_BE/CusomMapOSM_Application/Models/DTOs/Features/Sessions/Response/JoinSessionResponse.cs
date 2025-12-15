namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Response;


public class JoinSessionResponse
{
    public Guid SessionParticipantId { get; set; }
    public Guid SessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Message { get; set; } = "Joined session successfully";
    public DateTime JoinedAt { get; set; }
}