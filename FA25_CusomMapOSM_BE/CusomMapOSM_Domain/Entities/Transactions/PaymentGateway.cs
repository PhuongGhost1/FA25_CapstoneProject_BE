using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Transactions;

public class PaymentGateway
{
    public Guid GatewayId { get; set; }
    public string Name { get; set; } = string.Empty;
}
