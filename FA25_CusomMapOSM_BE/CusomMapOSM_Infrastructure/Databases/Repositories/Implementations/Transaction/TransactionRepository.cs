using CusomMapOSM_Domain.Entities.Transactions;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Transaction;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Transaction;

public class TransactionRepository : ITransactionRepository
{
    private readonly CustomMapOSMDbContext _context;
    public TransactionRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<Transactions> CreateAsync(Transactions transaction, CancellationToken ct)
    {
        await _context.Transactions.AddAsync(transaction, ct);
        await _context.SaveChangesAsync(ct);
        return transaction;
    }

    public async Task<Transactions?> GetByIdAsync(Guid transactionId, CancellationToken ct)
    {
        return await _context.Transactions.FirstOrDefaultAsync(t => t.TransactionId == transactionId, ct);
    }

    public async Task<Transactions?> GetByIdWithDetailsAsync(Guid transactionId, CancellationToken ct)
    {
        return await _context.Transactions
            .Include(t => t.Membership)
                .ThenInclude(m => m.Organization)
            .Include(t => t.Membership)
                .ThenInclude(m => m.Plan)
            .Include(t => t.PaymentGateway)
            .FirstOrDefaultAsync(t => t.TransactionId == transactionId, ct);
    }

    public async Task<Transactions> UpdateAsync(Transactions transaction, CancellationToken ct)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync(ct);
        return transaction;
    }

    public async Task<List<Transactions>> GetByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return await _context.Transactions
            .Where(t => t.Purpose != null && t.Purpose.Contains(userId.ToString()))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Transactions?> GetByTransactionReferenceAsync(string transactionReference, CancellationToken ct)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.TransactionReference == transactionReference, ct);
    }

    public async Task<Transactions?> GetPendingTransactionByOrgAsync(Guid orgId, CancellationToken ct)
    {
        return await _context.Transactions
            .Include(t => t.Membership)
            .Where(t => t.Membership != null && t.Membership.OrgId == orgId && t.Status.ToLower() == "pending")
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Transactions>> GetAllPendingTransactionsByOrgAsync(Guid orgId, CancellationToken ct)
    {
        return await _context.Transactions
            .Include(t => t.Membership)
            .Where(t => t.Membership != null && t.Membership.OrgId == orgId && t.Status.ToLower() == "pending")
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task UpdateRangeAsync(List<Transactions> transactions, CancellationToken ct)
    {
        _context.Transactions.UpdateRange(transactions);
        await _context.SaveChangesAsync(ct);
    }

    // Admin transaction methods
    public async Task<(List<Transactions> transactions, int totalCount)> GetAdminTransactionsAsync(
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
        CancellationToken ct)
    {
        var query = _context.Transactions
            .Include(t => t.Membership)
                .ThenInclude(m => m!.Organization)
            .Include(t => t.Membership)
                .ThenInclude(m => m!.Plan)
            .Include(t => t.PaymentGateway)
            .AsQueryable();

        // Apply filters
        if (startDate.HasValue)
            query = query.Where(t => t.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.CreatedAt <= endDate.Value);

        if (!string.IsNullOrEmpty(status))
        {
            var statuses = status.Split(',').Select(s => s.Trim().ToLower()).ToList();
            query = query.Where(t => statuses.Contains(t.Status.ToLower()));
        }

        if (userId.HasValue)
            query = query.Where(t => t.Purpose != null && t.Purpose.Contains(userId.Value.ToString()));

        if (orgId.HasValue)
            query = query.Where(t => t.Membership != null && t.Membership.OrgId == orgId.Value);

        if (minAmount.HasValue)
            query = query.Where(t => t.Amount >= minAmount.Value);

        if (maxAmount.HasValue)
            query = query.Where(t => t.Amount <= maxAmount.Value);

        if (!string.IsNullOrEmpty(paymentGateway))
            query = query.Where(t => t.PaymentGateway.Name.ToLower() == paymentGateway.ToLower());

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(t =>
                t.TransactionId.ToString().ToLower().Contains(searchLower) ||
                (t.Membership != null && t.Membership.Organization != null &&
                    t.Membership.Organization.OrgName.ToLower().Contains(searchLower)));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(ct);

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "amount" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(t => t.Amount)
                : query.OrderByDescending(t => t.Amount),
            "status" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(t => t.Status)
                : query.OrderByDescending(t => t.Status),
            _ => sortOrder.ToLower() == "asc"
                ? query.OrderBy(t => t.CreatedAt)
                : query.OrderByDescending(t => t.CreatedAt)
        };

        // Apply pagination
        var transactions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (transactions, totalCount);
    }

    public async Task<(int totalCount, decimal totalRevenue, int successCount, int pendingCount, int failedCount, int cancelledCount)> GetTransactionStatisticsAsync(
        DateTime? startDate,
        DateTime? endDate,
        string? status,
        Guid? userId,
        Guid? orgId,
        decimal? minAmount,
        decimal? maxAmount,
        string? paymentGateway,
        string? search,
        CancellationToken ct)
    {
        var query = _context.Transactions
            .Include(t => t.Membership)
                .ThenInclude(m => m!.Organization)
            .Include(t => t.PaymentGateway)
            .AsQueryable();

        // Apply same filters as GetAdminTransactionsAsync
        if (startDate.HasValue)
            query = query.Where(t => t.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.CreatedAt <= endDate.Value);

        if (!string.IsNullOrEmpty(status))
        {
            var statuses = status.Split(',').Select(s => s.Trim().ToLower()).ToList();
            query = query.Where(t => statuses.Contains(t.Status.ToLower()));
        }

        if (userId.HasValue)
            query = query.Where(t => t.Purpose != null && t.Purpose.Contains(userId.Value.ToString()));

        if (orgId.HasValue)
            query = query.Where(t => t.Membership != null && t.Membership.OrgId == orgId.Value);

        if (minAmount.HasValue)
            query = query.Where(t => t.Amount >= minAmount.Value);

        if (maxAmount.HasValue)
            query = query.Where(t => t.Amount <= maxAmount.Value);

        if (!string.IsNullOrEmpty(paymentGateway))
            query = query.Where(t => t.PaymentGateway.Name.ToLower() == paymentGateway.ToLower());

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(t =>
                t.TransactionId.ToString().ToLower().Contains(searchLower) ||
                (t.Membership != null && t.Membership.Organization != null &&
                    t.Membership.Organization.OrgName.ToLower().Contains(searchLower)));
        }

        var totalCount = await query.CountAsync(ct);

        var totalRevenue = await query
            .Where(t => t.Status.ToLower() == "success" || t.Status.ToLower() == "paid")
            .SumAsync(t => t.Amount, ct);

        var successCount = await query.CountAsync(t => t.Status.ToLower() == "success" || t.Status.ToLower() == "paid", ct);
        var pendingCount = await query.CountAsync(t => t.Status.ToLower() == "pending", ct);
        var failedCount = await query.CountAsync(t => t.Status.ToLower() == "failed", ct);
        var cancelledCount = await query.CountAsync(t => t.Status.ToLower() == "cancelled", ct);

        return (totalCount, totalRevenue, successCount, pendingCount, failedCount, cancelledCount);
    }

    public async Task<List<Transactions>> GetTransactionsByIdsAsync(List<Guid> transactionIds, CancellationToken ct)
    {
        return await _context.Transactions
            .Include(t => t.Membership)
                .ThenInclude(m => m!.Organization)
            .Include(t => t.Membership)
                .ThenInclude(m => m!.Plan)
            .Include(t => t.PaymentGateway)
            .Where(t => transactionIds.Contains(t.TransactionId))
            .ToListAsync(ct);
    }
}