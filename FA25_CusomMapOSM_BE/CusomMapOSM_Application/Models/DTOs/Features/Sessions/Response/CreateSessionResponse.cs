namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Response;

public class CreateSessionResponse
{
    public Guid SessionId { get; set; }
    public string SessionCode { get; set; } = string.Empty;
    public string SessionName { get; set; } = string.Empty;
    public string Message { get; set; } = "Session created successfully";
    public DateTime CreatedAt { get; set; }
}