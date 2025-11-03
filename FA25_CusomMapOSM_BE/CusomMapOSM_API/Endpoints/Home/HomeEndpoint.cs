using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Home;
using CusomMapOSM_Application.Models.DTOs.Features.Home;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Home;

public class HomeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("home")
            .WithTags(Tags.Home)
            .WithDescription("Home page statistics and public information");

        group.MapGet("/stats", async (
                [FromServices] IHomeService homeService) =>
            {
                var result = await homeService.GetHomeStats();
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetHomeStats")
            .WithDescription("Get aggregated statistics for home page (organizations, templates, total maps, monthly exports)")
            .AllowAnonymous()
            .Produces<HomeStatsResponse>(200)
            .ProducesProblem(500);
    }
}
