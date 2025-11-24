namespace CusomMapOSM_Application.Models.DTOs.Features.QuickPolls.Response;

using CusomMapOSM_Application.Models.DTOs.Features.QuickPolls;

public class QuickPollResponse
{
    public Guid PollId { get; set; }
    public Guid SessionId { get; set; }
    public string Question { get; set; } = string.Empty;
    public List<PollOptionResponse> Options { get; set; } = new();
    public PollTypeEnum PollType { get; set; } = PollTypeEnum.SingleChoice;
    public PollStatusEnum Status { get; set; } = PollStatusEnum.Draft;
    public bool AllowMultipleSelections { get; set; }
    public int? MaxSelections { get; set; }
    public int TotalVotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public RatingSummaryResponse? RatingSummary { get; set; }
}

public class PollOptionResponse
{
    public Guid OptionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int VoteCount { get; set; }
    public decimal Percentage { get; set; }
}
