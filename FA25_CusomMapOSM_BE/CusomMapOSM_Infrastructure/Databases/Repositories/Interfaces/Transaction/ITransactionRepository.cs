using CusomMapOSM_Domain.Entities.Transactions;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Transaction;

public interface ITransactionRepository
{
    Task<Transactions> CreateAsync(Transactions transaction, CancellationToken ct);
    Task<Transactions?> GetByIdAsync(Guid transactionId, CancellationToken ct);
    Task<Transactions?> GetByIdWithDetailsAsync(Guid transactionId, CancellationToken ct);
    Task<Transactions> UpdateAsync(Transactions transaction, CancellationToken ct);
    Task<List<Transactions>> GetByUserIdAsync(Guid userId, CancellationToken ct);
    Task<Transactions?> GetByTransactionReferenceAsync(string transactionReference, CancellationToken ct);
    Task<Transactions?> GetPendingTransactionByOrgAsync(Guid orgId, CancellationToken ct);
    Task<List<Transactions>> GetAllPendingTransactionsByOrgAsync(Guid orgId, CancellationToken ct);
    Task UpdateRangeAsync(List<Transactions> transactions, CancellationToken ct);

    // Admin transaction methods
    Task<(List<Transactions> transactions, int totalCount)> GetAdminTransactionsAsync(
        int page,
        int pageSize,
        string sortBy,
        string sortOrder,
        DateTime? startDate,
        DateTime? endDate,
        string? status,
        Guid? userId,
        Guid? orgId,
        decimal? minAmount,
        decimal? maxAmount,
        string? paymentGateway,
        string? search,
        CancellationToken ct);

    Task<(int totalCount, decimal totalRevenue, int successCount, int pendingCount, int failedCount, int cancelledCount)> GetTransactionStatisticsAsync(
        DateTime? startDate,
        DateTime? endDate,
        string? status,
        Guid? userId,
        Guid? orgId,
        decimal? minAmount,
        decimal? maxAmount,
        string? paymentGateway,
        string? search,
        CancellationToken ct);

    Task<List<Transactions>> GetTransactionsByIdsAsync(List<Guid> transactionIds, CancellationToken ct);
}