namespace CusomMapOSM_Application.Models.DTOs.Features.TreasureHunts.Request;

public class CreateTreasureHuntRequest
{
    public Guid SessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<TreasureClueRequest> Clues { get; set; } = new();
    public int AcceptanceRadiusMeters { get; set; } = 100;
    public int DurationMinutes { get; set; } = 30;
}

public class TreasureClueRequest
{
    public string ClueText { get; set; } = string.Empty;
    public decimal TargetLatitude { get; set; }
    public decimal TargetLongitude { get; set; }
    public int Points { get; set; }
}
