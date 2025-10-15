using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Application.Models.DTOs.Features.Membership;
using CusomMapOSM_API.Extensions;

namespace CusomMapOSM_API.Endpoints.Memberships;

public class MembershipEndpoint : IEndpoint
{
    private const string API_PREFIX = "membership";
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(API_PREFIX).RequireAuthorization();

        // These endpoints are now internal and handled by the payment service
        // The public APIs have been moved to /api/payment/* endpoints

        group.MapGet("/{membershipId:guid}/org/{orgId:guid}/feature/{featureKey}", async (IMembershipService membershipService, Guid membershipId, Guid orgId, string featureKey, CancellationToken ct) =>
        {
            var has = await membershipService.HasFeatureAsync(membershipId, orgId, featureKey, ct);
            return has.Match(
                some: has => Results.Ok(new FeatureCheckResponse(has)),
                none: err => err.ToProblemDetailsResult()
            );
        })
        .WithName("CheckFeatures")
        .WithDescription("Check features of an organization")
        .WithTags(Tags.Membership);

    }
}