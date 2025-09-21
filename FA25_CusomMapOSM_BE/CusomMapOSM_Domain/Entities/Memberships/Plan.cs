using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Memberships;

public class Plan
{
    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? PriceMonthly { get; set; }
    public int DurationMonths { get; set; }
    public int MaxOrganizations { get; set; }
    public int MaxLocationsPerOrg { get; set; }
    public int MaxMapsPerMonth { get; set; }
    public int MaxUsersPerOrg { get; set; }
    public int MapQuota { get; set; }
    public int ExportQuota { get; set; }
    public int MaxCustomLayers { get; set; }
    public int MonthlyTokens { get; set; } = 10000; // Token-based export quota
    public bool PrioritySupport { get; set; }
    public string? Features { get; set; } // Stored as JSON
    public string? AccessToolIds { get; set; } // Stored as JSON array of AccessTool IDs
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
