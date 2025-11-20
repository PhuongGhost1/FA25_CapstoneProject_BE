namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Events;

public class ParticipantJoinedEvent
{
    public Guid SessionParticipantId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool IsGuest { get; set; }
    public int TotalParticipants { get; set; }
    public DateTime JoinedAt { get; set; }
}