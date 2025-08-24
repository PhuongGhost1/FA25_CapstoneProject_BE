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

        group.MapPost("/create-or-renew", async (IMembershipService membershipService, CreateMembershipRequest request, CancellationToken ct) =>
        {
            if (request.UserId == Guid.Empty || request.OrgId == Guid.Empty || request.PlanId <= 0)
                return Results.BadRequest("Invalid membership request");

            var membership = await membershipService.CreateOrRenewMembershipAsync(request.UserId, request.OrgId, request.PlanId, request.AutoRenew, ct);
            return membership.Match(
                some: membership => Results.Ok(new CreateMembershipResponse(membership.MembershipId)),
                none: err => err.ToProblemDetailsResult()
            );
        })
        .WithName("CreateOrRenew")
        .WithDescription("Create or renew subscription plan")
        .WithTags(Tags.Membership)
        .Produces<CreateMembershipResponse>();

        group.MapPost("/change-plan", async (IMembershipService membershipService, ChangeSubscriptionPlanRequest request, CancellationToken ct) =>
        {
            if (request.UserId == Guid.Empty || request.OrgId == Guid.Empty || request.NewPlanId <= 0)
                return Results.BadRequest("Invalid plan change request");

            var membership = await membershipService.ChangeSubscriptionPlanAsync(request.UserId, request.OrgId, request.NewPlanId, request.AutoRenew, ct);
            return membership.Match(
                some: membership => Results.Ok(new ChangeSubscriptionPlanResponse(
                    membership.MembershipId,
                    "Plan changed successfully",
                    null, // Pro-rated amount would be calculated by billing system
                    DateTime.UtcNow)),
                none: err => err.ToProblemDetailsResult()
            );
        })
        .WithName("ChangeSubscriptionPlan")
        .WithDescription("Change subscription plan for existing membership")
        .WithTags(Tags.Membership)
        .Produces<ChangeSubscriptionPlanResponse>();

        group.MapPost("/track-usage", async (IMembershipService membershipService, TrackUsageRequest request, CancellationToken ct) =>
        {
            if (request.MembershipId == Guid.Empty || request.OrgId == Guid.Empty || request.Amount <= 0)
                return Results.BadRequest("Invalid usage request");

            var ok = await membershipService.TryConsumeQuotaAsync(request.MembershipId, request.OrgId, request.ResourceKey, request.Amount, ct);
            return ok.Match(
                some: ok => Results.Ok(new TrackUsageResponse(true)),
                none: err => err.ToProblemDetailsResult()
            );
        })
        .WithName("TrackUsage")
        .WithDescription("Track a usage of membership plan from user")
        .WithTags(Tags.Membership);

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

        group.MapPost("/purchase-addon", async (IMembershipService membershipService, PurchaseAddonRequest request, CancellationToken ct) =>
        {
            if (request.MembershipId == Guid.Empty || request.OrgId == Guid.Empty || string.IsNullOrWhiteSpace(request.AddonKey))
                return Results.BadRequest("Invalid addon request");

            var addon = await membershipService.AddAddonAsync(request.MembershipId, request.OrgId, request.AddonKey, request.Quantity, request.EffectiveImmediately, ct);
            return addon.Match(
                some: addon => Results.Ok(new PurchaseAddonResponse(addon.AddonId, "created")),
                none: err => err.ToProblemDetailsResult()
            );
        })
        .WithName("PurchaseAddOn")
        .WithDescription("Purchase add-on more for features")
        .WithTags(Tags.Membership);
    }
}