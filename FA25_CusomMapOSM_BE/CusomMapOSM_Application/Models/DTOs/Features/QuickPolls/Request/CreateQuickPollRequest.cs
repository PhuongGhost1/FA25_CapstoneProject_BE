
namespace CusomMapOSM_Application.Models.DTOs.Features.QuickPolls.Request;

using CusomMapOSM_Application.Models.DTOs.Features.QuickPolls;

public class CreateQuickPollRequest
{
    public Guid SessionId { get; set; }
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int DurationMinutes { get; set; } = 5;
    public PollTypeEnum PollType { get; set; } = PollTypeEnum.SingleChoice;
    public bool AllowMultipleSelections { get; set; } = false;
    public int? MaxSelections { get; set; }
    public int? RatingScaleMin { get; set; }
    public int? RatingScaleMax { get; set; }
    public bool AutoActivate { get; set; } = true;
}
