using CusomMapOSM_Domain.Entities.Memberships;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;

public interface IMembershipPlanRepository
{
    Task<IReadOnlyList<Plan>> GetActivePlansAsync(CancellationToken ct);
    Task<Plan?> GetPlanByIdAsync(int planId, CancellationToken ct);
}