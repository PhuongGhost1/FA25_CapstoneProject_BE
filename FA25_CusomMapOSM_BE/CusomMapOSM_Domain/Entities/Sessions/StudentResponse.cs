using CusomMapOSM_Domain.Entities.QuestionBanks;

namespace CusomMapOSM_Domain.Entities.Sessions;

public class StudentResponse
{
    public Guid StudentResponseId { get; set; }
    public Guid SessionQuestionId { get; set; }
    public Guid SessionParticipantId { get; set; }
    public Guid? QuestionOptionId { get; set; }
    public string? ResponseText { get; set; }
    public decimal? ResponseLatitude { get; set; }
    public decimal? ResponseLongitude { get; set; }
    public bool IsCorrect { get; set; } = false;
    public int PointsEarned { get; set; } = 0;
    public decimal ResponseTimeSeconds { get; set; } = 0;
    public bool UsedHint { get; set; } = false;
    public decimal? DistanceErrorMeters { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public SessionQuestion? SessionQuestion { get; set; }
    public SessionParticipant? SessionParticipant { get; set; }
    public QuestionOption? QuestionOption { get; set; }
}