using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Collaborations;

public class CollaborationPermission
{
    public Guid PermissionId { get; set; }
    public string? PermissionName { get; set; }
    public string? Description { get; set; }
    public int LevelOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
