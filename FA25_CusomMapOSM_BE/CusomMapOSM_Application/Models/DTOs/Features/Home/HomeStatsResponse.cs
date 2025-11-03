namespace CusomMapOSM_Application.Models.DTOs.Features.Home;

public record HomeStatsResponse
{

    public int OrganizationCount { get; init; }

    public int TemplateCount { get; init; }

    public int TotalMaps { get; init; }

    public int MonthlyExports { get; init; }
}
