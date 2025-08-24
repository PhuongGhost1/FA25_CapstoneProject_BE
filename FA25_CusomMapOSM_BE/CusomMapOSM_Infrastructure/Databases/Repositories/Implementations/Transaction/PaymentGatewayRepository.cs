using CusomMapOSM_Domain.Entities.Transactions;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Transaction;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Transaction;

public class PaymentGatewayRepository : IPaymentGatewayRepository
{
    private readonly CustomMapOSMDbContext _context;
    public PaymentGatewayRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentGateway?> GetByIdAsync(PaymentGatewayEnum name, CancellationToken ct)
    {
        return await _context.PaymentGateways.FirstOrDefaultAsync(x => x.Name == name.ToString(), ct);
    }
}