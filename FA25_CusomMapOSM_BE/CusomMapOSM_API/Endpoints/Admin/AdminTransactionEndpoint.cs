using System.Security.Claims;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Payment;
using CusomMapOSM_Application.Models.DTOs.Features.Payment;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Admin;

public class AdminTransactionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/billing/transactions")
            .WithTags("Admin - Transactions")
            .WithDescription("Admin endpoints for managing all system transactions")
            .RequireAuthorization(); // TODO: Add admin role requirement

        // Get all transactions with filtering and pagination
        group.MapGet("", async (
                [AsParameters] AdminTransactionFilterRequest request,
                [FromServices] IAdminTransactionService adminTransactionService,
                CancellationToken ct) =>
            {
                // TODO: Add admin role check
                var result = await adminTransactionService.GetAdminTransactionsAsync(request, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetAdminTransactions")
            .WithDescription("Get paginated list of all transactions with filtering")
            .Produces<AdminTransactionListResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(500);

        // Get transaction statistics
        group.MapGet("/statistics", async (
                [AsParameters] AdminTransactionFilterRequest request,
                [FromServices] IAdminTransactionService adminTransactionService,
                CancellationToken ct) =>
            {
                // TODO: Add admin role check
                var result = await adminTransactionService.GetTransactionStatisticsAsync(request, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetTransactionStatistics")
            .WithDescription("Get transaction statistics based on filters")
            .Produces<TransactionStatistics>(200)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(500);

        // Bulk download receipts as ZIP
        group.MapPost("/receipts/bulk-download", async (
                [FromBody] BulkDownloadReceiptsRequest request,
                [FromServices] IAdminTransactionService adminTransactionService,
                CancellationToken ct) =>
            {
                // TODO: Add admin role check
                var result = await adminTransactionService.BulkDownloadReceiptsAsync(request, ct);
                return result.Match(
                    success => Results.File(
                        success,
                        contentType: "application/zip",
                        fileDownloadName: $"receipts-{DateTime.UtcNow:yyyy-MM-dd}-{request.TransactionIds.Count}.zip"),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("BulkDownloadReceipts")
            .WithDescription("Download multiple receipts as a ZIP file")
            .Produces(200, contentType: "application/zip")
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(500);

        // Export transactions to CSV or Excel
        group.MapGet("/export", async (
                [AsParameters] ExportTransactionsRequest request,
                [FromServices] IAdminTransactionService adminTransactionService,
                CancellationToken ct) =>
            {
                // TODO: Add admin role check
                var result = await adminTransactionService.ExportTransactionsAsync(request, ct);

                var contentType = request.Format.ToLower() == "csv"
                    ? "text/csv"
                    : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                var filename = $"transactions-export-{DateTime.UtcNow:yyyy-MM-dd}.{request.Format.ToLower()}";

                return result.Match(
                    success => Results.File(success, contentType: contentType, fileDownloadName: filename),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("ExportTransactions")
            .WithDescription("Export transactions to CSV or Excel")
            .Produces(200, contentType: "text/csv")
            .Produces(200, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(413)
            .ProducesProblem(500);
    }
}
