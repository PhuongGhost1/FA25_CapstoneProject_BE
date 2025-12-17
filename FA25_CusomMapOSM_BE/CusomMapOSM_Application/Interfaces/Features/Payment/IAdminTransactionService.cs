using Optional;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Payment;

namespace CusomMapOSM_Application.Interfaces.Features.Payment;

public interface IAdminTransactionService
{
    /// <summary>
    /// Get paginated list of all transactions with filtering
    /// </summary>
    Task<Option<AdminTransactionListResponse, Error>> GetAdminTransactionsAsync(
        AdminTransactionFilterRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Get transaction statistics based on filters
    /// </summary>
    Task<Option<TransactionStatistics, Error>> GetTransactionStatisticsAsync(
        AdminTransactionFilterRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Generate ZIP file with multiple receipts
    /// </summary>
    Task<Option<byte[], Error>> BulkDownloadReceiptsAsync(
        BulkDownloadReceiptsRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Export transactions to CSV or Excel
    /// </summary>
    Task<Option<byte[], Error>> ExportTransactionsAsync(
        ExportTransactionsRequest request,
        CancellationToken ct = default);
}
