using System;
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
    /// <summary>
    /// Start date of the current billing cycle. 
    /// Set when membership is created or renewed.
    /// </summary>
    public DateTime BillingCycleStartDate { get; set; }
    
    /// <summary>
    /// End date of the current billing cycle.
    /// Typically 30 days from BillingCycleStartDate.
    /// </summary>
    public DateTime BillingCycleEndDate { get; set; }
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
