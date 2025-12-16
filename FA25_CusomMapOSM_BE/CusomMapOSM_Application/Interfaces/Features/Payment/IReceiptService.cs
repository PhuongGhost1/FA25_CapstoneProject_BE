using CusomMapOSM_Domain.Entities.Transactions;
using DomainMembership = CusomMapOSM_Domain.Entities.Memberships.Membership;

namespace CusomMapOSM_Application.Interfaces.Features.Payment;

public interface IReceiptService
{
    byte[] GenerateReceipt(Transactions transaction, DomainMembership membership);
}
