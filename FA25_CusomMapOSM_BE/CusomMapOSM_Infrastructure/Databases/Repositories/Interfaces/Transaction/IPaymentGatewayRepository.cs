using CusomMapOSM_Domain.Entities.Transactions;
using CusomMapOSM_Domain.Entities.Transactions.Enums;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Transaction;

public interface IPaymentGatewayRepository
{
    Task<PaymentGateway?> GetByIdAsync(PaymentGatewayEnum name, CancellationToken ct);
}