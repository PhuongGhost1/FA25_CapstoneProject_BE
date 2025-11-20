namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Response;


public class SubmitResponseResponse
{
    public Guid StudentResponseId { get; set; }
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }
    public int TotalScore { get; set; }
    public int CurrentRank { get; set; }
    public string? Explanation { get; set; }
    public string Message { get; set; } = "Response submitted successfully";
    public DateTime SubmittedAt { get; set; }
}