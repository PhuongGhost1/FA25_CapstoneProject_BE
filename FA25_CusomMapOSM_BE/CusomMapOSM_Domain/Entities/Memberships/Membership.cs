using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Organizations;
using CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Memberships.Enums;

namespace CusomMapOSM_Domain.Entities.Memberships;

public class Membership
{
    public Guid MembershipId { get; set; }
    public Guid UserId { get; set; }
    public Guid OrgId { get; set; }
    public int PlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public MembershipStatusEnum Status { get; set; } = MembershipStatusEnum.PendingPayment;
    public bool AutoRenew { get; set; }
    public string? CurrentUsage { get; set; } // Stored as JSON
    public DateTime? LastResetDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User? User { get; set; }
    public Organization? Organization { get; set; }
    public Plan? Plan { get; set; }
}
