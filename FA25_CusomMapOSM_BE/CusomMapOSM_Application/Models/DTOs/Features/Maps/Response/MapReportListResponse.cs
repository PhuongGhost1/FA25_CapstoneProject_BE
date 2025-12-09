namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;

public class MapReportListResponse
{
    public List<MapReportDto> Reports { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

