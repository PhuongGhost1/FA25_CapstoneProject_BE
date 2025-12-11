using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Features.Notifications;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using CusomMapOSM_Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.Maps;

public class MapReportService : IMapReportService
{
    private readonly IMapReportRepository _reportRepository;
    private readonly IMapRepository _mapRepository;
    private readonly IMapService _mapService;
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHubContext<NotificationHub> _hubContext;

    public MapReportService(
        IMapReportRepository reportRepository,
        IMapRepository mapRepository,
        IMapService mapService,
        INotificationService notificationService,
        ICurrentUserService currentUserService,
        IHubContext<NotificationHub> hubContext)
    {
        _reportRepository = reportRepository;
        _mapRepository = mapRepository;
        _mapService = mapService;
        _notificationService = notificationService;
        _currentUserService = currentUserService;
        _hubContext = hubContext;
    }

    public async Task<Option<MapReportDto, Error>> ReportMapAsync(ReportMapRequest request)
    {
        var map = await _mapRepository.GetMapById(request.MapId);
        if (map == null)
        {
            return Option.None<MapReportDto, Error>(
                Error.NotFound("Map.NotFound", "Map not found"));
        }

        if (!map.IsPublic && map.Status != MapStatusEnum.Published)
        {
            return Option.None<MapReportDto, Error>(
                Error.ValidationError("Map.NotPublic", "Cannot report a non-public map"));
        }

        var currentUserId = _currentUserService.GetUserId();

        var report = new MapReport
        {
            MapReportId = Guid.NewGuid(),
            MapId = request.MapId,
            ReporterUserId = currentUserId,
            ReporterEmail = request.ReporterEmail ?? string.Empty,
            ReporterName = request.ReporterName ?? string.Empty,
            Reason = request.Reason,
            Description = request.Description,
            Status = MapReportStatusEnum.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _reportRepository.CreateReportAsync(report);
        if (!result)
        {
            return Option.None<MapReportDto, Error>(
                Error.Failure("Report.CreateFailed", "Failed to create report"));
        }

        try
        {
            await _hubContext.Clients.Group("admin").SendAsync("AdminNotification", new
            {
                type = "map_report",
                title = "Báo cáo vi phạm map mới",
                message = $"Có báo cáo vi phạm mới cho map: {map.MapName ?? "Không tên"}",
                reportId = report.MapReportId.ToString(),
                mapId = report.MapId.ToString(),
                reason = report.Reason,
                createdAt = report.CreatedAt
            });
        }
        catch
        {
            // Ignore SignalR errors - notification is best-effort and should not block the main operation
        }

        return Option.Some<MapReportDto, Error>(ToDto(report, map));
    }

    public async Task<Option<MapReportListResponse, Error>> GetReportsAsync(int page = 1, int pageSize = 20)
    {
        var reports = await _reportRepository.GetAllReportsAsync(page, pageSize);
        var totalCount = await _reportRepository.GetReportsCountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var reportDtos = reports.Select(r =>
        {
            var map = r.Map;
            return ToDto(r, map);
        }).ToList();

        return Option.Some<MapReportListResponse, Error>(new MapReportListResponse
        {
            Reports = reportDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        });
    }

    public async Task<Option<MapReportListResponse, Error>> GetReportsByStatusAsync(int status, int page = 1, int pageSize = 20)
    {
        var reports = await _reportRepository.GetReportsByStatusAsync(status, page, pageSize);
        var totalCount = await _reportRepository.GetReportsCountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var reportDtos = reports.Select(r =>
        {
            var map = r.Map;
            return ToDto(r, map);
        }).ToList();

        return Option.Some<MapReportListResponse, Error>(new MapReportListResponse
        {
            Reports = reportDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        });
    }

    public async Task<Option<MapReportDto, Error>> GetReportByIdAsync(Guid reportId)
    {
        var report = await _reportRepository.GetReportByIdAsync(reportId);
        if (report == null)
        {
            return Option.None<MapReportDto, Error>(
                Error.NotFound("Report.NotFound", "Report not found"));
        }

        return Option.Some<MapReportDto, Error>(ToDto(report, report.Map));
    }

    public async Task<Option<MapReportDto, Error>> ReviewReportAsync(Guid reportId, ReviewReportRequest request)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<MapReportDto, Error>(
                Error.Unauthorized("Report.Unauthorized", "User not authenticated"));
        }

        var report = await _reportRepository.GetReportByIdAsync(reportId);
        if (report == null)
        {
            return Option.None<MapReportDto, Error>(
                Error.NotFound("Report.NotFound", "Report not found"));
        }

        if (!Enum.IsDefined(typeof(MapReportStatusEnum), request.Status))
        {
            return Option.None<MapReportDto, Error>(
                Error.ValidationError("Report.InvalidStatus", "Invalid status value"));
        }

        var status = (MapReportStatusEnum)request.Status;

        // If status is Resolved and should delete map, delete the map and send notification
        if (status == MapReportStatusEnum.Resolved && request.ShouldDeleteMap)
        {
            var map = await _mapRepository.GetMapById(report.MapId);
            if (map != null)
            {
                // Delete the map
                var deleteResult = await _mapService.Delete(report.MapId);
                if (!deleteResult.HasValue)
                {
                    return Option.None<MapReportDto, Error>(
                        Error.Failure("Map.DeleteFailed", "Failed to delete map"));
                }

                // Send notification to map owner
                var mapOwnerId = map.UserId;
                var mapName = map.MapName ?? "Map";
                var notificationMessage = $"Map của bạn \"{mapName}\" đã bị xóa do vi phạm chính sách. Lý do: {report.Reason}";
                if (!string.IsNullOrEmpty(request.ReviewNotes))
                {
                    notificationMessage += $"\n\nGhi chú từ quản trị viên: {request.ReviewNotes}";
                }

                var notificationResult = await _notificationService.CreateNotificationAsync(
                    mapOwnerId,
                    "map_violation",
                    notificationMessage,
                    $"{{\"mapId\":\"{report.MapId}\",\"reportId\":\"{reportId}\"}}"
                );

                // Log if notification fails but don't fail the whole operation
                if (!notificationResult.HasValue)
                {
                    // Log error but continue
                    Console.WriteLine($"Failed to send notification to user {mapOwnerId} for map deletion");
                }
            }
        }

        report.Status = status;
        report.ReviewedByUserId = currentUserId;
        report.ReviewedAt = DateTime.UtcNow;
        report.ReviewNotes = request.ReviewNotes;
        report.UpdatedAt = DateTime.UtcNow;

        var result = await _reportRepository.UpdateReportAsync(report);
        if (!result)
        {
            return Option.None<MapReportDto, Error>(
                Error.Failure("Report.UpdateFailed", "Failed to update report"));
        }

        return Option.Some<MapReportDto, Error>(ToDto(report, report.Map));
    }

    public async Task<Option<int, Error>> GetPendingReportsCountAsync()
    {
        var count = await _reportRepository.GetPendingReportsCountAsync();
        return Option.Some<int, Error>(count);
    }

    private static MapReportDto ToDto(MapReport report, Map? map)
    {
        return new MapReportDto
        {
            ReportId = report.MapReportId,
            MapId = report.MapId,
            MapName = map?.MapName,
            ReporterUserId = report.ReporterUserId,
            ReporterEmail = report.ReporterEmail,
            ReporterName = report.ReporterName,
            Reason = report.Reason,
            Description = report.Description,
            Status = (int)report.Status,
            StatusLabel = report.Status.ToString(),
            ReviewedByUserId = report.ReviewedByUserId,
            ReviewedByName = report.ReviewedByUser?.FullName,
            ReviewedAt = report.ReviewedAt,
            ReviewNotes = report.ReviewNotes,
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt
        };
    }
}

