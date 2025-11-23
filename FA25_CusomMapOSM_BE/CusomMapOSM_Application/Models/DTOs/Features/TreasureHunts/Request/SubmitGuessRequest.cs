namespace CusomMapOSM_Application.Models.DTOs.Features.TreasureHunts.Request;

public class SubmitGuessRequest
{
    public Guid TreasureHuntId { get; set; }
    public Guid ClueId { get; set; }
    public Guid SessionParticipantId { get; set; }
    public decimal GuessLatitude { get; set; }
    public decimal GuessLongitude { get; set; }
}
