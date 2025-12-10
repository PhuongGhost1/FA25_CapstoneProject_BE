using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Exports;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Exports;

public interface IExportService
{
    Task<Option<ExportResponse, Error>> CreateExportAsync(CreateExportRequest request);
    Task<Option<ExportResponse, Error>> GetExportByIdAsync(int exportId);
    Task<Option<ExportListResponse, Error>> GetMyExportsAsync();
    Task<Option<ExportListResponse, Error>> GetExportsByMapIdAsync(Guid mapId);
    Task<Option<ExportListResponse, Error>> GetExportsByOrganizationIdAsync(Guid organizationId);
    Task ProcessPendingExportsAsync();

    // Admin approval methods
    Task<Option<ExportListResponse, Error>> GetPendingApprovalExportsAsync();
    Task<Option<ExportResponse, Error>> ApproveExportAsync(int exportId, Guid adminUserId);
    Task<Option<ExportResponse, Error>> RejectExportAsync(int exportId, Guid adminUserId, string reason);
    Task<Option<string, Error>> GetExportDownloadUrlAsync(int exportId);
}

