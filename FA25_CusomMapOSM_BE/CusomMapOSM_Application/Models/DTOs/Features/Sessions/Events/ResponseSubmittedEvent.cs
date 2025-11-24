namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Events;

public class ResponseSubmittedEvent
{
    public Guid SessionQuestionId { get; set; }
    public Guid ParticipantId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }
    public decimal ResponseTimeSeconds { get; set; }
    public int TotalResponses { get; set; }
    public DateTime SubmittedAt { get; set; }
}