namespace CusomMapOSM_Application.Models.DTOs.Features.TreasureHunts.Response;

public class TreasureHuntResponse
{
    public Guid TreasureHuntId { get; set; }
    public Guid SessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<TreasureClueResponse> Clues { get; set; } = new();
    public int AcceptanceRadiusMeters { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class TreasureClueResponse
{
    public Guid ClueId { get; set; }
    public string ClueText { get; set; } = string.Empty;
    public int Points { get; set; }
    public int DisplayOrder { get; set; }
    // Don't expose target coordinates to students
}
