using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.OrganizationAdmin;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.OrganizationAdmin;

public interface IOrganizationAdminService
{
    // Organization Management Endpoints (Org Owners/Admins)
    Task<Option<OrganizationUsageResponse, Error>> GetOrganizationUsageAsync(Guid orgId, CancellationToken ct = default);
    Task<Option<OrganizationSubscriptionResponse, Error>> GetOrganizationSubscriptionAsync(Guid orgId, CancellationToken ct = default);
    Task<Option<OrganizationBillingResponse, Error>> GetOrganizationBillingAsync(Guid orgId, CancellationToken ct = default);

    // Authorization Helpers
    Task<Option<bool, Error>> IsUserOrganizationAdminAsync(Guid userId, Guid orgId, CancellationToken ct = default);
    Task<Option<bool, Error>> IsUserOrganizationOwnerAsync(Guid userId, Guid orgId, CancellationToken ct = default);
}
