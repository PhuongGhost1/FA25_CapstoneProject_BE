using System.ComponentModel.DataAnnotations;
using CusomMapOSM_Domain.Entities.Sessions.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Request;

public class CreateSessionRequest
{
    [Required]
    public Guid MapId { get; set; }

    [Required]
    public Guid QuestionBankId { get; set; }

    [Required]
    [StringLength(200)]
    public string SessionName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public SessionTypeEnum SessionType { get; set; } = SessionTypeEnum.LIVE;

    public int MaxParticipants { get; set; } = 0; // 0 = unlimited

    public bool AllowLateJoin { get; set; } = true;
    public bool ShowLeaderboard { get; set; } = true;
    public bool ShowCorrectAnswers { get; set; } = true;
    public bool ShuffleQuestions { get; set; } = false;
    public bool ShuffleOptions { get; set; } = false;
    public bool EnableHints { get; set; } = true;
    public bool PointsForSpeed { get; set; } = true;

    public DateTime? ScheduledStartTime { get; set; }
}