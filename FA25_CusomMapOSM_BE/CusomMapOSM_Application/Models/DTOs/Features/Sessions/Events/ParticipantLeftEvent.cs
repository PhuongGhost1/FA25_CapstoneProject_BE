namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Events;


public class ParticipantLeftEvent
{
    public Guid SessionParticipantId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int TotalParticipants { get; set; }
    public DateTime LeftAt { get; set; }
}