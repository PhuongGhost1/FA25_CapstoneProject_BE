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
    public Guid? QuestionOptionId { get; set; }
    public string? OptionText { get; set; }
    public string? ResponseText { get; set; }
    public decimal? ResponseLatitude { get; set; }
    public decimal? ResponseLongitude { get; set; }
    public decimal? DistanceErrorMeters { get; set; }
}