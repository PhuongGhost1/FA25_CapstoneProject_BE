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
}