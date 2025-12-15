namespace CusomMapOSM_Application.Models.DTOs.Features.Usage;

public record UsageQuotaDto
{
    public required string ResourceType { get; set; }
    public required int CurrentUsage { get; set; }
    public required int Limit { get; set; }
    public required int PercentageUsed { get; set; }
    public required bool IsUnlimited { get; set; }
    public required bool IsExceeded { get; set; }
}

public record UserUsageResponse
{
    public required Guid UserId { get; set; }
    public required Guid MembershipId { get; set; }
    public required string PlanName { get; set; }
    public required List<UsageQuotaDto> Quotas { get; set; }
    public required DateTime LastResetDate { get; set; }
    public required DateTime NextResetDate { get; set; }
}

public record OrganizationUsageResponse
{
    public required Guid OrganizationId { get; set; }
    public required string OrganizationName { get; set; }
    public required Guid MembershipId { get; set; }
    public required string PlanName { get; set; }
    public required List<UsageQuotaDto> Quotas { get; set; }
    public required int TotalMembers { get; set; }
    public required DateTime LastResetDate { get; set; }
    public required DateTime NextResetDate { get; set; }
}

public record CheckQuotaRequest
{
    public required string ResourceType { get; set; }
    public required int RequestedAmount { get; set; }
}

public record CheckQuotaResponse
{
    public required bool IsAllowed { get; set; }
    public required int CurrentUsage { get; set; }
    public required int Limit { get; set; }
    public required int RemainingQuota { get; set; }
    public required string Message { get; set; }
}

public record PlanLimitsResponse
{
    public required string PlanName { get; set; }
    public required decimal PriceMonthly { get; set; }
    public int? OrganizationMax { get; set; }
    public int? LocationMax { get; set; }
    public int? ViewsMonthly { get; set; }
    public int? MapsMax { get; set; }
    public int? MembersMax { get; set; }
    public int? MapQuota { get; set; }
    public int? ExportQuota { get; set; }
    public int? MaxLayer { get; set; }
    public int? TokenMonthly { get; set; }
    public long? MediaFileMax { get; set; }
    public long? VideoFileMax { get; set; }
    public long? AudioFileMax { get; set; }
}