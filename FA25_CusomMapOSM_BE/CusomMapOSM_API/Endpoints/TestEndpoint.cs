using CusomMapOSM_API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CusomMapOSM_API.Endpoints;

public class TestEndpoint : IEndpoint
{
    private const string API_PREFIX = "test";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(API_PREFIX);

        // Public endpoint - no authorization required
        group.MapGet("/public", () =>
        {
            return Results.Ok(new { message = "This is a public endpoint - no authentication required" });
        })
        .WithName("PublicTest")
        .WithDescription("Public test endpoint - no authentication required")
        .WithTags("Test");

        // Protected endpoint - authorization required
        group.MapGet("/protected", (ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            return Results.Ok(new
            {
                message = "This is a protected endpoint - authentication required",
                userId = userId,
                email = email,
                authenticated = user.Identity?.IsAuthenticated ?? false
            });
        })
        .RequireAuthorization()
        .WithName("ProtectedTest")
        .WithDescription("Protected test endpoint - authentication required")
        .WithTags("Test");

        // Admin endpoint - specific role required
        group.MapGet("/admin", (ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            return Results.Ok(new
            {
                message = "This is an admin endpoint - admin role required",
                userId = userId,
                email = email,
                authenticated = user.Identity?.IsAuthenticated ?? false
            });
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("AdminTest")
        .WithDescription("Admin test endpoint - admin role required")
        .WithTags("Test");
    }
}
