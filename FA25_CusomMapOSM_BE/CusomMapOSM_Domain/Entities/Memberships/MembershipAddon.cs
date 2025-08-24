using System;

namespace CusomMapOSM_Domain.Entities.Memberships;

public class MembershipAddon
{
    public Guid AddonId { get; set; }
    public Guid MembershipId { get; set; }
    public Guid OrgId { get; set; }

    // Addon types identified by key, e.g., "extra_exports", "extra_maps", "extra_users", "feature_pack"
    public string AddonKey { get; set; } = string.Empty;
    public int? Quantity { get; set; } // e.g., number of extra exports/maps/users provided by this addon
    public string? FeaturePayload { get; set; } // JSON for feature flags or details if feature-based addon

    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveFrom { get; set; } // if effective immediately or from next cycle
    public DateTime? EffectiveUntil { get; set; } // optional expiration
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}


