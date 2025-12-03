using System;
using CusomMapOSM_Domain.Entities.Organizations;

namespace CusomMapOSM_Domain.Entities.Memberships;

public class MembershipUsage
{
    public Guid UsageId { get; set; }
    public Guid MembershipId { get; set; }
    public Guid OrgId { get; set; }

    // Rolling counters for the current cycle
    public int MapsCreatedThisCycle { get; set; }
    public int ExportsThisCycle { get; set; }
    public int ActiveUsersInOrg { get; set; }
    public string? FeatureFlags { get; set; } // JSON for arbitrary feature toggles/entitlements

    public DateTime CycleStartDate { get; set; }
    public DateTime CycleEndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public Organization? Organizations { get; set; }
    public Membership? Membership { get; set; }
}


