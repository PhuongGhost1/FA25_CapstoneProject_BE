namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Events;

public class TimeExtendedEvent
{
    public Guid SessionQuestionId { get; set; }
    public Guid QuestionId { get; set; }
    public int AdditionalSeconds { get; set; }
    public int NewTimeLimit { get; set; }
    public DateTime ExtendedAt { get; set; }
}