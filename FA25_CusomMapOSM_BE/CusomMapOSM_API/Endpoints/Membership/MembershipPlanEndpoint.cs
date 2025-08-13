using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Membership;

namespace CusomMapOSM_API.Endpoints.Memberships;

public class MembershipPlanEndpoint : IEndpoint
{
    private const string API_PREFIX = "membership-plan";
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(API_PREFIX);

        group.MapGet("/active", async (IMembershipPlanService planService, CancellationToken ct) =>
        {
            var plans = await planService.GetActivePlansAsync(ct);
            return Results.Ok(plans);
        });
    }
}