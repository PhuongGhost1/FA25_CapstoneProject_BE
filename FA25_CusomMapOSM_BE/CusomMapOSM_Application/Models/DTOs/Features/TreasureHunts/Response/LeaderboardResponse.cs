namespace CusomMapOSM_Application.Models.DTOs.Features.TreasureHunts.Response;

public class LeaderboardResponse
{
    public Guid TreasureHuntId { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<LeaderboardEntry> Entries { get; set; } = new();
}

public class LeaderboardEntry
{
    public Guid SessionParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public int CluesFound { get; set; }
    public int Rank { get; set; }
}
