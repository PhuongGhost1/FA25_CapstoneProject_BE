using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using DomainMembership = CusomMapOSM_Domain.Entities.Memberships;

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
        })
        .WithName("GetActivePlans")
        .WithDescription("Get all active plans")
        .Produces<IReadOnlyList<DomainMembership.Plan>>();

        group.MapGet("/{id}", async (IMembershipPlanService planService, int id, CancellationToken ct) =>
        {
            var plan = await planService.GetPlanByIdAsync(id, ct);
            return Results.Ok(plan);
        })
        .WithName("GetPlanById")
        .WithDescription("Get plan by id")
        .Produces<DomainMembership.Plan>();
    }
}