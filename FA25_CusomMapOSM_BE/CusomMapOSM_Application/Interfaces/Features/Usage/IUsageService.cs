using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Usage;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Usage;

public interface IUsageService
{
    // User usage tracking
    Task<Option<UserUsageResponse, Error>> GetUserUsageAsync(Guid userId, Guid orgId, CancellationToken ct = default);
    Task<Option<CheckQuotaResponse, Error>> CheckUserQuotaAsync(Guid userId, Guid orgId, string resourceType, int requestedAmount, CancellationToken ct = default);
    Task<Option<bool, Error>> ConsumeUserQuotaAsync(Guid userId, Guid orgId, string resourceType, int amount, CancellationToken ct = default);

    // Organization usage tracking (for org owners/admins)
    Task<Option<OrganizationUsageResponse, Error>> GetOrganizationUsageAsync(Guid orgId, CancellationToken ct = default);
    Task<Option<CheckQuotaResponse, Error>> CheckOrganizationQuotaAsync(Guid orgId, string resourceType, int requestedAmount, CancellationToken ct = default);
    Task<Option<bool, Error>> ConsumeOrganizationQuotaAsync(Guid orgId, string resourceType, int amount, CancellationToken ct = default);

    // Quota management
    Task<Option<bool, Error>> ResetUsageCycleAsync(Guid membershipId, CancellationToken ct = default);
    Task<Option<bool, Error>> CheckAndNotifyQuotaWarningsAsync(CancellationToken ct = default);
}
