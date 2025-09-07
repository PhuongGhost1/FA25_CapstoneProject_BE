using System.Security.Claims;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Application.Models.DTOs.Features.User;

namespace CusomMapOSM_API.Endpoints.User;

public class UserEndpoint : IEndpoint
{
    private const string API_PREFIX = "user";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(API_PREFIX).RequireAuthorization();

        group.MapGet("/me", async (
            ClaimsPrincipal user,
            IUserService userService,
            CancellationToken ct) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Results.BadRequest("Invalid user ID");

            var userResult = await userService.GetUserByIdAsync(userId, ct);
            return userResult.Match(
                some: userEntity => Results.Ok(new GetUserInfoResponse
                {
                    User = new UserInfoDto
                    {
                        UserId = userEntity.UserId,
                        Email = userEntity.Email,
                        FullName = userEntity.FullName,
                        Phone = userEntity.Phone,
                        Role = userEntity.Role?.Name ?? "Unknown",
                        AccountStatus = userEntity.AccountStatus?.Name ?? "Unknown",
                        CreatedAt = userEntity.CreatedAt,
                        LastLogin = userEntity.LastLogin
                    }
                }),
                none: err => err.ToProblemDetailsResult()
            );
        })
        .WithName("GetUserInfo")
        .WithDescription("Get current user information")
        .WithTags(Tags.User)
        .Produces<GetUserInfoResponse>();

        group.MapGet("/me/membership/{orgId:guid}", async (
            ClaimsPrincipal user,
            Guid orgId,
            IMembershipService membershipService,
            CancellationToken ct) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Results.BadRequest("Invalid user ID");

            var membershipResult = await membershipService.GetCurrentMembershipWithIncludesAsync(userId, orgId, ct);
            return membershipResult.Match(
                some: membership => Results.Ok(new GetCurrentMembershipResponse
                {
                    Membership = new CurrentMembershipDto
                    {
                        MembershipId = membership.MembershipId,
                        UserId = membership.UserId,
                        OrgId = membership.OrgId,
                        OrgName = membership.Organization?.OrgName ?? "Unknown",
                        PlanId = membership.PlanId,
                        PlanName = membership.Plan?.PlanName ?? "Unknown",
                        StartDate = membership.StartDate,
                        EndDate = membership.EndDate,
                        Status = membership.Status?.Name ?? "Unknown",
                        AutoRenew = membership.AutoRenew,
                        LastResetDate = membership.LastResetDate,
                        CreatedAt = membership.CreatedAt,
                        UpdatedAt = membership.UpdatedAt
                    }
                }),
                none: err => err.ToProblemDetailsResult()
            );
        })
        .WithName("GetCurrentMembership")
        .WithDescription("Get current membership for user in specific organization")
        .WithTags(Tags.User)
        .Produces<GetCurrentMembershipResponse>();
    }
}
