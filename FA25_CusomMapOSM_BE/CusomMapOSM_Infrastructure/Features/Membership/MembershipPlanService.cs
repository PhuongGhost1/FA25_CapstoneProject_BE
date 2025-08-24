using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Domain.Entities.Memberships;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;

namespace CusomMapOSM_Infrastructure.Features.Membership;

public class MembershipPlanService : IMembershipPlanService
{
    private readonly IMembershipPlanRepository _membershipPlanRepository;
    public MembershipPlanService(IMembershipPlanRepository membershipPlanRepository)
    {
        _membershipPlanRepository = membershipPlanRepository;
    }

    public async Task<IReadOnlyList<Plan>> GetActivePlansAsync(CancellationToken ct)
    {
        return await _membershipPlanRepository.GetActivePlansAsync(ct);
    }

    public async Task<Plan?> GetPlanByIdAsync(int planId, CancellationToken ct)
    {
        return await _membershipPlanRepository.GetPlanByIdAsync(planId, ct);
    }
}