namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Events;


public class SessionStatusChangedEvent
{
    public Guid SessionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTime ChangedAt { get; set; }
}