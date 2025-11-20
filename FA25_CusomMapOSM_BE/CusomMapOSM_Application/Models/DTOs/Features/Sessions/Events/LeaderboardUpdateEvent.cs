namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Events;

public class LeaderboardUpdateEvent
{
    public Guid SessionId { get; set; }
    public List<LeaderboardEntry> TopParticipants { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
}

public class LeaderboardEntry
{
    public Guid SessionParticipantId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int TotalScore { get; set; }
    public int TotalCorrect { get; set; }
    public int TotalAnswered { get; set; }
    public decimal AverageResponseTime { get; set; }
    public int Rank { get; set; }
}