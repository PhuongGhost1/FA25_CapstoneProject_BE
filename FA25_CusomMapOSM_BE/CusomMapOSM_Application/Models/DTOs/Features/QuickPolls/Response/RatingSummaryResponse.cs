namespace CusomMapOSM_Application.Models.DTOs.Features.QuickPolls.Response;

public class RatingSummaryResponse
{
    public int? ScaleMin { get; set; }
    public int? ScaleMax { get; set; }
    public decimal Average { get; set; }
    public int TotalVotes { get; set; }
}

