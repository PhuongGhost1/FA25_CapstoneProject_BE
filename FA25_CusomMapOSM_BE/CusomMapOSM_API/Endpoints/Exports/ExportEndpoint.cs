using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Exports;
using CusomMapOSM_Application.Models.DTOs.Features.Exports;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Exports;

public class ExportEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.Exports)
            .WithTags("Exports")
            .WithDescription("Map export management endpoints")
            .RequireAuthorization();

        // Create export
        group.MapPost("/", async (
                [FromBody] CreateExportRequest request,
                [FromServices] IExportService exportService) =>
            {
                var result = await exportService.CreateExportAsync(request);
                return result.Match(
                    success => Results.Created($"/api/v1/exports/{success.ExportId}", success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("CreateExport")
            .WithDescription("Create a new map export (async processing)")
            .Produces<ExportResponse>(201)
            .ProducesValidationProblem();

        // Get export by ID
        group.MapGet("/{exportId:int}", async (
                [FromRoute] int exportId,
                [FromServices] IExportService exportService) =>
            {
                var result = await exportService.GetExportByIdAsync(exportId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetExportById")
            .WithDescription("Get export by ID")
            .Produces<ExportResponse>(200)
            .Produces(404);

        // Get my exports
        group.MapGet("/my", async (
                [FromServices] IExportService exportService) =>
            {
                var result = await exportService.GetMyExportsAsync();
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetMyExports")
            .WithDescription("Get all exports for the current user")
            .Produces<ExportListResponse>(200);

        // Get exports by map ID
        group.MapGet("/map/{mapId:guid}", async (
                [FromRoute] Guid mapId,
                [FromServices] IExportService exportService) =>
            {
                var result = await exportService.GetExportsByMapIdAsync(mapId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetExportsByMapId")
            .WithDescription("Get all exports for a specific map")
            .Produces<ExportListResponse>(200);

        // Get download URL (only if approved)
        group.MapGet("/{exportId:int}/download", async (
                [FromRoute] int exportId,
                [FromServices] IExportService exportService) =>
            {
                var result = await exportService.GetExportDownloadUrlAsync(exportId);
                return result.Match(
                    success => Results.Ok(new { downloadUrl = success }),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetExportDownloadUrl")
            .WithDescription("Get download URL for an approved export")
            .Produces<object>(200)
            .Produces(403)
            .Produces(404);
    }
}

