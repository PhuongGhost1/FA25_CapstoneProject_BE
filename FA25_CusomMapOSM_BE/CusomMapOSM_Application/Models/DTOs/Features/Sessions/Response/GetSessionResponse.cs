using CusomMapOSM_Domain.Entities.Sessions.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Response;

public class GetSessionResponse
{
    public Guid SessionId { get; set; }
    public string SessionCode { get; set; } = string.Empty;
    public string SessionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SessionTypeEnum SessionType { get; set; }
    public SessionStatusEnum Status { get; set; }

    public Guid MapId { get; set; }
    public string MapName { get; set; } = string.Empty;

    public List<QuestionBankInfo> QuestionBanks { get; set; } = new();

    public Guid HostUserId { get; set; }
    public string HostUserName { get; set; } = string.Empty;

    public int MaxParticipants { get; set; }
    public int TotalParticipants { get; set; }
    public int TotalResponses { get; set; }

    public bool AllowLateJoin { get; set; }
    public bool ShowLeaderboard { get; set; }
    public bool ShowCorrectAnswers { get; set; }

    public DateTime? ScheduledStartTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class QuestionBankInfo
{
    public Guid QuestionBankId { get; set; }
    public string QuestionBankName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime AttachedAt { get; set; }
}