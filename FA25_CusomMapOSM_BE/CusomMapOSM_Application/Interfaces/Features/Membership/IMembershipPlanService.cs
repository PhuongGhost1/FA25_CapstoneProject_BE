using CusomMapOSM_Domain.Entities.Memberships;

namespace CusomMapOSM_Application.Interfaces.Features.Membership;

public interface IMembershipPlanService
{
    Task<IReadOnlyList<Plan>> GetActivePlansAsync(CancellationToken ct);
    Task<Plan?> GetPlanByIdAsync(int planId, CancellationToken ct);
}