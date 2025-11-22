using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.QuestionBanks;
using CusomMapOSM_Domain.Entities.Sessions.Enums;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Sessions;

public class Session
{
    public Guid SessionId { get; set; }
    public Guid MapId { get; set; }
    public Guid QuestionBankId { get; set; }
    public Guid HostUserId { get; set; }
    public string SessionCode { get; set; } = string.Empty;
    public string SessionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SessionTypeEnum SessionType { get; set; } = SessionTypeEnum.LIVE;
    public SessionStatusEnum Status { get; set; } = SessionStatusEnum.DRAFT;
    public int MaxParticipants { get; set; } = 0;
    public bool AllowLateJoin { get; set; } = true;
    public bool ShowLeaderboard { get; set; } = true;
    public bool ShowCorrectAnswers { get; set; } = true;
    public bool ShuffleQuestions { get; set; } = false;
    public bool ShuffleOptions { get; set; } = false;
    public bool EnableHints { get; set; } = true;
    public bool PointsForSpeed { get; set; } = true;
    public DateTime? ScheduledStartTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int TotalParticipants { get; set; } = 0;
    public int TotalResponses { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Map? Map { get; set; }
    public QuestionBank? QuestionBank { get; set; }
    public User? HostUser { get; set; }
}