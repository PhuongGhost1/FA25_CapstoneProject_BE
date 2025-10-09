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
