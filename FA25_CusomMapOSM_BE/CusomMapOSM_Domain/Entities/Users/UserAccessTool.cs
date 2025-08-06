using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.AccessTools;

namespace CusomMapOSM_Domain.Entities.Users;

public class UserAccessTool
{
    public int UserAccessToolId { get; set; }
    public required Guid UserId { get; set; }
    public required int AccessToolId { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiredAt { get; set; }

    public User User { get; set; } = new();
    public AccessTool AccessTool { get; set; } = new();
}
