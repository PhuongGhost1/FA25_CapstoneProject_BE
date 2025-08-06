using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Memberships;

public class MembershipStatus
{
    public Guid StatusId { get; set; }
    public string Name { get; set; } = string.Empty;
}
