using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Maps;

public class MapReportEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.Maps + "/reports")
            .WithTags(Tags.Map)
            .WithDescription("Map report endpoints");

        group.MapPost("/", async (
                [FromBody] ReportMapRequest request,
                [FromServices] IMapReportService reportService) =>
            {
                var result = await reportService.ReportMapAsync(request);
                return result.Match(
                    success => Results.Created($"/api/maps/reports/{success.ReportId}", success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("ReportMap")
            .WithDescription("Report a map for policy violation")
            .Produces<CusomMapOSM_Application.Models.DTOs.Features.Maps.Response.MapReportDto>(201)
            .ProducesProblem(400)
            .ProducesProblem(404);

        group.MapGet("/", async (
                [FromServices] IMapReportService reportService,
                [FromQuery] int page = 1, 
                [FromQuery] int pageSize = 20) =>
            {
                var result = await reportService.GetReportsAsync(page, pageSize);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetReports")
            .WithDescription("Get all map reports (Admin only)")
            .RequireAuthorization()
            .Produces<CusomMapOSM_Application.Models.DTOs.Features.Maps.Response.MapReportListResponse>(200)
            .ProducesProblem(401)
            .ProducesProblem(403);

        group.MapGet("/status/{status}", async (
                [FromRoute] int status,
                [FromServices] IMapReportService reportService,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 20) =>
            {
                var result = await reportService.GetReportsByStatusAsync(status, page, pageSize);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetReportsByStatus")
            .WithDescription("Get map reports by status (Admin only)")
            .RequireAuthorization()
            .Produces<CusomMapOSM_Application.Models.DTOs.Features.Maps.Response.MapReportListResponse>(200)
            .ProducesProblem(401)
            .ProducesProblem(403);

        group.MapGet("/{reportId:guid}", async (
                [FromRoute] Guid reportId,
                [FromServices] IMapReportService reportService) =>
            {
                var result = await reportService.GetReportByIdAsync(reportId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetReportById")
            .WithDescription("Get a specific map report (Admin only)")
            .RequireAuthorization()
            .Produces<CusomMapOSM_Application.Models.DTOs.Features.Maps.Response.MapReportDto>(200)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404);

        group.MapPost("/{reportId:guid}/review", async (
                [FromRoute] Guid reportId,
                [FromBody] ReviewReportRequest request,
                [FromServices] IMapReportService reportService) =>
            {
                var result = await reportService.ReviewReportAsync(reportId, request);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("ReviewReport")
            .WithDescription("Review a map report (Admin only)")
            .RequireAuthorization()
            .Produces<CusomMapOSM_Application.Models.DTOs.Features.Maps.Response.MapReportDto>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404);

        group.MapGet("/pending/count", async (
                [FromServices] IMapReportService reportService) =>
            {
                var result = await reportService.GetPendingReportsCountAsync();
                return result.Match(
                    success => Results.Ok(new { count = success }),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetPendingReportsCount")
            .WithDescription("Get count of pending reports (Admin only)")
            .RequireAuthorization()
            .Produces<object>(200)
            .ProducesProblem(401)
            .ProducesProblem(403);
    }
}

