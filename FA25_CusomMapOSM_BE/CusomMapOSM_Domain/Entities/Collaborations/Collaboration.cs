using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Domain.Entities.Collaborations;

public class Collaboration
{
    public int CollaborationId { get; set; }
    public required Guid TargetTypeId { get; set; }
    public required string TargetId { get; set; }
    public required Guid UserId { get; set; }
    public required Guid PermissionId { get; set; }
    public Guid? InvitedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public CollaborationTargetType TargetType { get; set; } = new();
    public User User { get; set; } = new();
    public CollaborationPermission Permission { get; set; } = new();
    public User? Inviter { get; set; }
}
