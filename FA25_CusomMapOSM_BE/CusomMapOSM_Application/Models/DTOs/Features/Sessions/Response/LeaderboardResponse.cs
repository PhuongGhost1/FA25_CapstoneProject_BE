namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Response;

public class LeaderboardResponse
{
    public Guid SessionId { get; set; }
    public List<LeaderboardEntry> Leaderboard { get; set; } = new();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class LeaderboardEntry
{
    public int Rank { get; set; }
    public Guid SessionParticipantId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int TotalScore { get; set; }
    public int TotalCorrect { get; set; }
    public int TotalAnswered { get; set; }
    public decimal AverageResponseTime { get; set; }
    public bool IsCurrentUser { get; set; } = false;
}