namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Events;

public class QuestionResponsesUpdateEvent
{
    public Guid SessionQuestionId { get; set; }
    public Guid SessionId { get; set; }
    public int TotalResponses { get; set; }
    public List<StudentAnswerDetailEvent> Answers { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
}

public class StudentAnswerDetailEvent
{
    public Guid StudentResponseId { get; set; }
    public Guid ParticipantId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }
    public decimal ResponseTimeSeconds { get; set; }
    public DateTime SubmittedAt { get; set; }
    
    // Answer content - depends on question type
    // For MULTIPLE_CHOICE and TRUE_FALSE
    public Guid? QuestionOptionId { get; set; }
    public string? OptionText { get; set; }
    
    // For SHORT_ANSWER
    public string? ResponseText { get; set; }
    
    // For PIN_ON_MAP
    public decimal? ResponseLatitude { get; set; }
    public decimal? ResponseLongitude { get; set; }
    public decimal? DistanceErrorMeters { get; set; }
}
