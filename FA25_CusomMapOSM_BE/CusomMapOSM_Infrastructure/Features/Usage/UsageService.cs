using CusomMapOSM_Application.Interfaces.Features.Usage;
using CusomMapOSM_Application.Interfaces.Features.Notifications;
using CusomMapOSM_Application.Models.DTOs.Features.Usage;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
using DomainMembership = CusomMapOSM_Domain.Entities.Memberships;
using Optional;
using CusomMapOSM_Application.Common.Errors;
using Optional.Unsafe;

namespace CusomMapOSM_Infrastructure.Features.Usage;

public class UsageService : IUsageService
{
    private readonly IMembershipService _membershipService;
    private readonly INotificationService _notificationService;
    private readonly IOrganizationRepository _organizationRepository;

    public UsageService(
        IMembershipService membershipService,
        INotificationService notificationService,
        IOrganizationRepository organizationRepository)
    {
        _membershipService = membershipService;
        _notificationService = notificationService;
        _organizationRepository = organizationRepository;
    }

    public async Task<Option<UserUsageResponse, Error>> GetUserUsageAsync(Guid userId, Guid orgId, CancellationToken ct = default)
    {
        try
        {
            var membershipResult = await _membershipService.GetCurrentMembershipWithIncludesAsync(userId, orgId, ct);
            if (!membershipResult.HasValue)
            {
                return Option.None<UserUsageResponse, Error>(Error.NotFound("Membership.NotFound", "No active membership found"));
            }

            var membership = membershipResult.ValueOrDefault();
            var usageResult = await _membershipService.GetOrCreateUsageAsync(membership.MembershipId, orgId, ct);
            if (!usageResult.HasValue)
            {
                return Option.None<UserUsageResponse, Error>(Error.Failure("Usage.GetFailed", "Failed to get usage information"));
            }

            var usage = usageResult.ValueOrDefault();
            var quotas = CreateUsageQuotas(usage, membership.Plan);

            return Option.Some<UserUsageResponse, Error>(new UserUsageResponse
            {
                UserId = userId,
                MembershipId = membership.MembershipId,
                PlanName = membership.Plan?.PlanName ?? "Unknown",
                Quotas = quotas,
                LastResetDate = usage.CycleStartDate,
                NextResetDate = usage.CycleEndDate
            });
        }
        catch (Exception ex)
        {
            return Option.None<UserUsageResponse, Error>(Error.Failure("Usage.GetFailed", $"Failed to get user usage: {ex.Message}"));
        }
    }

    public async Task<Option<CheckQuotaResponse, Error>> CheckUserQuotaAsync(Guid userId, Guid orgId, string resourceType, int requestedAmount, CancellationToken ct = default)
    {
        try
        {
            var membershipResult = await _membershipService.GetCurrentMembershipWithIncludesAsync(userId, orgId, ct);
            if (!membershipResult.HasValue)
            {
                return Option.None<CheckQuotaResponse, Error>(Error.NotFound("Membership.NotFound", "No active membership found"));
            }

            var membership = membershipResult.ValueOrDefault();
            var usageResult = await _membershipService.GetOrCreateUsageAsync(membership.MembershipId, orgId, ct);
            if (!usageResult.HasValue)
            {
                return Option.None<CheckQuotaResponse, Error>(Error.Failure("Usage.GetFailed", "Failed to get usage information"));
            }

            var usage = usageResult.ValueOrDefault();
            var quotaInfo = GetQuotaInfo(resourceType, usage, membership.Plan);

            var isAllowed = quotaInfo.IsUnlimited || (quotaInfo.CurrentUsage + requestedAmount <= quotaInfo.Limit);
            var remainingQuota = quotaInfo.IsUnlimited ? int.MaxValue : Math.Max(0, quotaInfo.Limit - quotaInfo.CurrentUsage);

            var message = isAllowed
                ? $"Quota check passed. {remainingQuota} {resourceType} remaining."
                : $"Quota exceeded. You have {quotaInfo.CurrentUsage}/{quotaInfo.Limit} {resourceType} used. Requested: {requestedAmount}";

            return Option.Some<CheckQuotaResponse, Error>(new CheckQuotaResponse
            {
                IsAllowed = isAllowed,
                CurrentUsage = quotaInfo.CurrentUsage,
                Limit = quotaInfo.Limit,
                RemainingQuota = remainingQuota,
                Message = message
            });
        }
        catch (Exception ex)
        {
            return Option.None<CheckQuotaResponse, Error>(Error.Failure("Usage.CheckFailed", $"Failed to check quota: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> ConsumeUserQuotaAsync(Guid userId, Guid orgId, string resourceType, int amount, CancellationToken ct = default)
    {
        try
        {
            var membershipResult = await _membershipService.GetCurrentMembershipWithIncludesAsync(userId, orgId, ct);
            if (!membershipResult.HasValue)
            {
                return Option.None<bool, Error>(Error.NotFound("Membership.NotFound", "No active membership found"));
            }

            var membership = membershipResult.ValueOrDefault();
            var consumeResult = await _membershipService.TryConsumeQuotaAsync(membership.MembershipId, orgId, resourceType, amount, ct);

            if (!consumeResult.HasValue)
            {
                return Option.None<bool, Error>(Error.Failure("Usage.ConsumeFailed", "Failed to consume quota"));
            }

            if (!consumeResult.ValueOrDefault())
            {
                return Option.None<bool, Error>(Error.ValidationError("Usage.QuotaExceeded", "Quota exceeded for this resource"));
            }

            return Option.Some<bool, Error>(true);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("Usage.ConsumeFailed", $"Failed to consume quota: {ex.Message}"));
        }
    }

    public async Task<Option<OrganizationUsageResponse, Error>> GetOrganizationUsageAsync(Guid orgId, CancellationToken ct = default)
    {
        try
        {
            var organization = await _organizationRepository.GetOrganizationById(orgId);
            if (organization == null)
            {
                return Option.None<OrganizationUsageResponse, Error>(Error.NotFound("Organization.NotFound", "Organization not found"));
            }

            // Get organization owner's membership (assuming owner has the main membership)
            var ownerMembershipResult = await _membershipService.GetCurrentMembershipWithIncludesAsync(organization.OwnerUserId, orgId, ct);
            if (!ownerMembershipResult.HasValue)
            {
                return Option.None<OrganizationUsageResponse, Error>(Error.NotFound("Membership.NotFound", "No active membership found for organization"));
            }

            var membership = ownerMembershipResult.ValueOrDefault();
            var usageResult = await _membershipService.GetOrCreateUsageAsync(membership.MembershipId, orgId, ct);
            if (!usageResult.HasValue)
            {
                return Option.None<OrganizationUsageResponse, Error>(Error.Failure("Usage.GetFailed", "Failed to get usage information"));
            }

            var usage = usageResult.ValueOrDefault();
            var quotas = CreateUsageQuotas(usage, membership.Plan);

            return Option.Some<OrganizationUsageResponse, Error>(new OrganizationUsageResponse
            {
                OrganizationId = orgId,
                OrganizationName = organization.OrgName,
                MembershipId = membership.MembershipId,
                PlanName = membership.Plan?.PlanName ?? "Unknown",
                Quotas = quotas,
                TotalMembers = 1, // This would need to be calculated from organization members
                LastResetDate = usage.CycleStartDate,
                NextResetDate = usage.CycleEndDate
            });
        }
        catch (Exception ex)
        {
            return Option.None<OrganizationUsageResponse, Error>(Error.Failure("Usage.GetFailed", $"Failed to get organization usage: {ex.Message}"));
        }
    }

    public async Task<Option<CheckQuotaResponse, Error>> CheckOrganizationQuotaAsync(Guid orgId, string resourceType, int requestedAmount, CancellationToken ct = default)
    {
        try
        {
            var organization = await _organizationRepository.GetOrganizationById(orgId);
            if (organization == null)
            {
                return Option.None<CheckQuotaResponse, Error>(Error.NotFound("Organization.NotFound", "Organization not found"));
            }

            return await CheckUserQuotaAsync(organization.OwnerUserId, orgId, resourceType, requestedAmount, ct);
        }
        catch (Exception ex)
        {
            return Option.None<CheckQuotaResponse, Error>(Error.Failure("Usage.CheckFailed", $"Failed to check organization quota: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> ConsumeOrganizationQuotaAsync(Guid orgId, string resourceType, int amount, CancellationToken ct = default)
    {
        try
        {
            var organization = await _organizationRepository.GetOrganizationById(orgId);
            if (organization == null)
            {
                return Option.None<bool, Error>(Error.NotFound("Organization.NotFound", "Organization not found"));
            }

            return await ConsumeUserQuotaAsync(organization.OwnerUserId, orgId, resourceType, amount, ct);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("Usage.ConsumeFailed", $"Failed to consume organization quota: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> ResetUsageCycleAsync(Guid membershipId, CancellationToken ct = default)
    {
        try
        {
            var result = await _membershipService.ResetUsageCycleAsync(membershipId, ct);
            return result;
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("Usage.ResetFailed", $"Failed to reset usage cycle: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> CheckAndNotifyQuotaWarningsAsync(CancellationToken ct = default)
    {
        try
        {
            // This would be implemented as a background job
            // For now, return success
            return Option.Some<bool, Error>(true);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("Usage.NotificationFailed", $"Failed to check and notify quota warnings: {ex.Message}"));
        }
    }

    private List<UsageQuotaDto> CreateUsageQuotas(DomainMembership.MembershipUsage usage, DomainMembership.Plan? plan)
    {
        var quotas = new List<UsageQuotaDto>();

        if (plan != null)
        {
            // Map Quota
            quotas.Add(CreateQuotaDto("maps", usage.MapsCreatedThisCycle, plan.MapQuota));

            // Export Quota
            quotas.Add(CreateQuotaDto("exports", usage.ExportsThisCycle, plan.ExportQuota));

            // Users Quota
            quotas.Add(CreateQuotaDto("users", usage.ActiveUsersInOrg, plan.MaxUsersPerOrg));

            // Custom Layers Quota
            quotas.Add(CreateQuotaDto("custom_layers", 0, plan.MaxCustomLayers)); // This would need to be tracked

            // Monthly Tokens
            quotas.Add(CreateQuotaDto("tokens", 0, plan.MonthlyTokens)); // This would need to be tracked
        }

        return quotas;
    }

    private UsageQuotaDto CreateQuotaDto(string resourceType, int currentUsage, int limit)
    {
        var isUnlimited = limit == -1;
        var actualLimit = isUnlimited ? int.MaxValue : limit;
        var percentageUsed = isUnlimited ? 0 : (int)Math.Round((double)currentUsage / actualLimit * 100);
        var isExceeded = !isUnlimited && currentUsage > limit;

        return new UsageQuotaDto
        {
            ResourceType = resourceType,
            CurrentUsage = currentUsage,
            Limit = actualLimit,
            PercentageUsed = percentageUsed,
            IsUnlimited = isUnlimited,
            IsExceeded = isExceeded
        };
    }

    private (int CurrentUsage, int Limit, bool IsUnlimited) GetQuotaInfo(string resourceType, DomainMembership.MembershipUsage usage, DomainMembership.Plan? plan)
    {
        if (plan == null)
        {
            return (0, 0, false);
        }

        return resourceType.ToLower() switch
        {
            "maps" => (usage.MapsCreatedThisCycle, plan.MapQuota, plan.MapQuota == -1),
            "exports" => (usage.ExportsThisCycle, plan.ExportQuota, plan.ExportQuota == -1),
            "users" => (usage.ActiveUsersInOrg, plan.MaxUsersPerOrg, plan.MaxUsersPerOrg == -1),
            "custom_layers" => (0, plan.MaxCustomLayers, plan.MaxCustomLayers == -1),
            "tokens" => (0, plan.MonthlyTokens, plan.MonthlyTokens == -1),
            _ => (0, 0, false)
        };
    }
}
