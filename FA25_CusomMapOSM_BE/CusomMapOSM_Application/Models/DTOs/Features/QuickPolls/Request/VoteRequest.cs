namespace CusomMapOSM_Application.Models.DTOs.Features.QuickPolls.Request;

public class VoteRequest
{
    public Guid PollId { get; set; }
    public Guid SessionParticipantId { get; set; }
    public Guid? OptionId { get; set; }
    public List<Guid> OptionIds { get; set; } = new();
    public decimal? RatingValue { get; set; }
}
