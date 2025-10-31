using System;

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
    
    // Interactive Points Feature Limits
    public int MaxInteractionsPerMap { get; set; } = 50;
    public long MaxMediaFileSizeBytes { get; set; } = 10_485_760; // 10MB default for images
    public long MaxVideoFileSizeBytes { get; set; } = 104_857_600; // 100MB default for videos
    public long MaxAudioFileSizeBytes { get; set; } = 20_971_520; // 20MB default for audio
    public int MaxConnectionsPerMap { get; set; } = 100;
    public bool Allow3DEffects { get; set; } = false; // Pro feature
    public bool AllowVideoContent { get; set; } = true;
    public bool AllowAudioContent { get; set; } = true;
    public bool AllowAnimatedConnections { get; set; } = true;
    
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

}
