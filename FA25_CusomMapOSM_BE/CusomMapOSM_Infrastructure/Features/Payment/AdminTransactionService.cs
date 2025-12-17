using System.IO.Compression;
using System.Text;
using Optional;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Payment;
using CusomMapOSM_Application.Models.DTOs.Features.Payment;
using CusomMapOSM_Domain.Entities.Transactions;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Transaction;
using Microsoft.Extensions.Logging;

namespace CusomMapOSM_Infrastructure.Features.Payment;

public class AdminTransactionService : IAdminTransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IReceiptService _receiptService;
    private readonly ILogger<AdminTransactionService> _logger;

    public AdminTransactionService(
        ITransactionRepository transactionRepository,
        IReceiptService receiptService,
        ILogger<AdminTransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _receiptService = receiptService;
        _logger = logger;
    }

    public async Task<Option<AdminTransactionListResponse, Error>> GetAdminTransactionsAsync(
        AdminTransactionFilterRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var (transactions, totalCount) = await _transactionRepository.GetAdminTransactionsAsync(
                request.Page,
                request.PageSize,
                request.SortBy,
                request.SortOrder,
                request.StartDate,
                request.EndDate,
                request.Status,
                request.UserId,
                request.OrgId,
                request.MinAmount,
                request.MaxAmount,
                request.PaymentGateway,
                request.Search,
                ct);

            var transactionDtos = transactions.Select(t => new AdminTransactionDto
            {
                TransactionId = t.TransactionId.ToString(),
                Amount = t.Amount,
                Currency = "USD",
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                Description = t.Purpose ?? "N/A",
                User = new AdminUserDto
                {
                    UserId = t.Membership?.UserId.ToString() ?? "N/A",
                    Email = "N/A", // Will need to join Users table if email is needed
                    FullName = "N/A"
                },
                Organization = new AdminOrgDto
                {
                    OrgId = t.Membership?.OrgId.ToString() ?? "N/A",
                    OrgName = t.Membership?.Organization?.OrgName ?? "N/A"
                },
                Plan = new AdminPlanDto
                {
                    PlanId = t.Membership?.PlanId ?? 0,
                    PlanName = t.Membership?.Plan?.PlanName ?? "N/A",
                    PriceMonthly = t.Membership?.Plan?.PriceMonthly ?? 0
                },
                PaymentGateway = new AdminPaymentGatewayDto
                {
                    GatewayId = t.PaymentGatewayId.ToString(),
                    Name = t.PaymentGateway?.Name ?? "N/A"
                },
                CanDownloadReceipt = t.Status?.ToLower() == "success" || t.Status?.ToLower() == "paid"
            }).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            return Option.Some<AdminTransactionListResponse, Error>(new AdminTransactionListResponse
            {
                Transactions = transactionDtos,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasMore = request.Page < totalPages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get admin transactions");
            return Option.None<AdminTransactionListResponse, Error>(
                Error.Failure("AdminTransaction.GetFailed", $"Failed to retrieve transactions: {ex.Message}"));
        }
    }

    public async Task<Option<TransactionStatistics, Error>> GetTransactionStatisticsAsync(
        AdminTransactionFilterRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var (totalCount, totalRevenue, successCount, pendingCount, failedCount, cancelledCount) =
                await _transactionRepository.GetTransactionStatisticsAsync(
                    request.StartDate,
                    request.EndDate,
                    request.Status,
                    request.UserId,
                    request.OrgId,
                    request.MinAmount,
                    request.MaxAmount,
                    request.PaymentGateway,
                    request.Search,
                    ct);

            var successRate = totalCount > 0 ? (double)successCount / totalCount * 100 : 0;

            return Option.Some<TransactionStatistics, Error>(new TransactionStatistics
            {
                TotalCount = totalCount,
                TotalRevenue = totalRevenue,
                Currency = "USD",
                SuccessCount = successCount,
                PendingCount = pendingCount,
                FailedCount = failedCount,
                CancelledCount = cancelledCount,
                SuccessRate = Math.Round(successRate, 1)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get transaction statistics");
            return Option.None<TransactionStatistics, Error>(
                Error.Failure("AdminTransaction.StatisticsFailed", $"Failed to retrieve statistics: {ex.Message}"));
        }
    }

    public async Task<Option<byte[], Error>> BulkDownloadReceiptsAsync(
        BulkDownloadReceiptsRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var transactionIds = request.TransactionIds.Select(Guid.Parse).ToList();
            var transactions = await _transactionRepository.GetTransactionsByIdsAsync(transactionIds, ct);

            // Filter only successful/paid transactions
            var successfulTransactions = transactions
                .Where(t => t.Status?.ToLower() == "success" || t.Status?.ToLower() == "paid")
                .Where(t => t.Membership != null)
                .ToList();

            if (!successfulTransactions.Any())
            {
                return Option.None<byte[], Error>(
                    Error.ValidationError("BulkDownload.NoValidTransactions", "No valid transactions found for receipt generation"));
            }

            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var errorMessages = new List<string>();

                foreach (var transaction in successfulTransactions)
                {
                    try
                    {
                        var pdfBytes = _receiptService.GenerateReceipt(transaction, transaction.Membership!);
                        var entry = archive.CreateEntry($"receipt-{transaction.TransactionId}.pdf");

                        using var entryStream = entry.Open();
                        await entryStream.WriteAsync(pdfBytes, 0, pdfBytes.Length, ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to generate receipt for transaction {TransactionId}", transaction.TransactionId);
                        errorMessages.Add($"Transaction {transaction.TransactionId}: {ex.Message}");
                    }
                }

                // Add error file if any errors occurred
                if (errorMessages.Any())
                {
                    var errorContent = string.Join(Environment.NewLine, errorMessages);
                    var errorBytes = Encoding.UTF8.GetBytes(errorContent);
                    var errorEntry = archive.CreateEntry("errors.txt");

                    using var errorStream = errorEntry.Open();
                    await errorStream.WriteAsync(errorBytes, 0, errorBytes.Length, ct);
                }
            }

            return Option.Some<byte[], Error>(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bulk download receipts");
            return Option.None<byte[], Error>(
                Error.Failure("BulkDownload.Failed", $"Failed to generate ZIP file: {ex.Message}"));
        }
    }

    public async Task<Option<byte[], Error>> ExportTransactionsAsync(
        ExportTransactionsRequest request,
        CancellationToken ct = default)
    {
        try
        {
            // Get all transactions matching the filters (up to 10,000)
            var (transactions, totalCount) = await _transactionRepository.GetAdminTransactionsAsync(
                1,
                10000,
                "createdAt",
                "desc",
                request.StartDate,
                request.EndDate,
                request.Status,
                request.UserId,
                request.OrgId,
                request.MinAmount,
                request.MaxAmount,
                request.PaymentGateway,
                request.Search,
                ct);

            if (totalCount > 10000)
            {
                return Option.None<byte[], Error>(
                    Error.ValidationError("Export.TooManyRecords",
                        $"Cannot export more than 10,000 transactions. Current filter matches {totalCount} records. Please apply additional filters."));
            }

            if (request.Format.ToLower() == "csv")
            {
                return await ExportToCsvAsync(transactions);
            }
            else if (request.Format.ToLower() == "xlsx")
            {
                return Option.None<byte[], Error>(
                    Error.Failure("Export.ExcelNotImplemented", "Excel export is not yet implemented"));
            }
            else
            {
                return Option.None<byte[], Error>(
                    Error.ValidationError("Export.InvalidFormat", "Invalid format. Must be 'csv' or 'xlsx'"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export transactions");
            return Option.None<byte[], Error>(
                Error.Failure("Export.Failed", $"Failed to export transactions: {ex.Message}"));
        }
    }

    private Task<Option<byte[], Error>> ExportToCsvAsync(List<Transactions> transactions)
    {
        try
        {
            var csv = new StringBuilder();

            // Headers
            csv.AppendLine("Transaction ID,Date,User Email,Organization,Plan,Amount,Currency,Status,Payment Gateway,Created At");

            // Rows
            foreach (var t in transactions)
            {
                csv.AppendLine($"{t.TransactionId}," +
                    $"{t.TransactionDate:yyyy-MM-dd}," +
                    $"N/A," + // User email would need Users table join
                    $"\"{t.Membership?.Organization?.OrgName ?? "N/A"}\"," +
                    $"\"{t.Membership?.Plan?.PlanName ?? "N/A"}\"," +
                    $"{t.Amount:F2}," +
                    $"USD," +
                    $"{t.Status}," +
                    $"{t.PaymentGateway?.Name ?? "N/A"}," +
                    $"{t.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return Task.FromResult(Option.Some<byte[], Error>(bytes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate CSV");
            return Task.FromResult(Option.None<byte[], Error>(
                Error.Failure("Export.CsvFailed", $"Failed to generate CSV: {ex.Message}")));
        }
    }
}
