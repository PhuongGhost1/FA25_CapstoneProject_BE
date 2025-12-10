using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Maps;

public interface IMapReportService
{
    Task<Option<MapReportDto, Error>> ReportMapAsync(ReportMapRequest request);
    Task<Option<MapReportListResponse, Error>> GetReportsAsync(int page = 1, int pageSize = 20);
    Task<Option<MapReportListResponse, Error>> GetReportsByStatusAsync(int status, int page = 1, int pageSize = 20);
    Task<Option<MapReportDto, Error>> GetReportByIdAsync(Guid reportId);
    Task<Option<MapReportDto, Error>> ReviewReportAsync(Guid reportId, ReviewReportRequest request);
    Task<Option<int, Error>> GetPendingReportsCountAsync();
}

