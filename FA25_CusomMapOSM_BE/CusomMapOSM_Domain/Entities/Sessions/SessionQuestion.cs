using CusomMapOSM_Domain.Entities.QuestionBanks;
using CusomMapOSM_Domain.Entities.Sessions.Enums;

namespace CusomMapOSM_Domain.Entities.Sessions;

public class SessionQuestion
{
    public Guid SessionQuestionId { get; set; }
    public Guid SessionId { get; set; }
    public Guid QuestionId { get; set; }
    public int QueueOrder { get; set; } = 0;
    public SessionQuestionStatusEnum Status { get; set; } = SessionQuestionStatusEnum.QUEUED;
    public int? PointsOverride { get; set; }
    public int? TimeLimitOverride { get; set; }
    public int TimeLimitExtensions { get; set; } = 0;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int TotalResponses { get; set; } = 0;
    public int CorrectResponses { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Session? Session { get; set; }
    public Question? Question { get; set; }
}