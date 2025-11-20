namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Events;


public class QuestionActivatedEvent
{
    public Guid SessionQuestionId { get; set; }
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public int Points { get; set; }
    public int TimeLimit { get; set; }
    public int QuestionNumber { get; set; }
    public int TotalQuestions { get; set; }
    public List<QuestionOptionDto>? Options { get; set; }
    public DateTime ActivatedAt { get; set; }
}

public class QuestionOptionDto
{
    public Guid QuestionOptionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
}