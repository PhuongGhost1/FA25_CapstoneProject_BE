using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Users;

public class UserRole
{
    public Guid RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
}
