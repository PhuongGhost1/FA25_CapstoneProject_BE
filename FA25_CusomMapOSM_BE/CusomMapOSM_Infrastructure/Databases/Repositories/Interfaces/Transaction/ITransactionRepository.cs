using CusomMapOSM_Domain.Entities.Transactions;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Transaction;

public interface ITransactionRepository
{
    Task<Transactions> CreateAsync(Transactions transaction, CancellationToken ct);
    Task<Transactions?> GetByIdAsync(Guid transactionId, CancellationToken ct);
    Task<Transactions> UpdateAsync(Transactions transaction, CancellationToken ct);
    Task<List<Transactions>> GetByUserIdAsync(Guid userId, CancellationToken ct);
    Task<Transactions?> GetByTransactionReferenceAsync(string transactionReference, CancellationToken ct);
}