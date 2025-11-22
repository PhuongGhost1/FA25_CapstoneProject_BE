namespace CusomMapOSM_Application.Models.DTOs.Features.TreasureHunts.Response;

public class SubmitGuessResponse
{
    public bool IsCorrect { get; set; }
    public decimal DistanceMeters { get; set; }
    public int PointsEarned { get; set; }
    public string Message { get; set; } = string.Empty;
}
