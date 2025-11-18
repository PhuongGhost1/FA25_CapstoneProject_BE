using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.StoryMaps;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.StoryMaps;

public class RouteAnimationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.StoryMap)
            .WithTags(Tags.StoryMaps)
            .WithDescription("Route Animation Management")
            .RequireAuthorization();

        MapRouteAnimationEndpoints(group);
    }

    private static void MapRouteAnimationEndpoints(RouteGroupBuilder group)
    {
        // GET route animations by segment
        group.MapGet(Routes.StoryMapEndpoints.GetRouteAnimations, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.GetRouteAnimationsBySegmentAsync(segmentId, ct);
                return result.Match<IResult>(
                    animations => Results.Ok(animations),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetRouteAnimations")
            .WithDescription("Get all route animations for a segment")
            .WithTags(Tags.StoryMaps)
            .Produces<IEnumerable<RouteAnimationDto>>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // GET route animation by ID
        group.MapGet(Routes.StoryMapEndpoints.GetRouteAnimation, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromRoute] Guid routeAnimationId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.GetRouteAnimationAsync(routeAnimationId, ct);
                return result.Match<IResult>(
                    animation => Results.Ok(animation),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetRouteAnimation")
            .WithDescription("Get a route animation by ID")
            .WithTags(Tags.StoryMaps)
            .Produces<RouteAnimationDto>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // POST create route animation
        group.MapPost(Routes.StoryMapEndpoints.CreateRouteAnimation, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromBody] CreateRouteAnimationRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                // Ensure segmentId matches route parameter
                var createRequest = request with { SegmentId = segmentId };
                var result = await service.CreateRouteAnimationAsync(createRequest, ct);
                return result.Match<IResult>(
                    animation => Results.Created($"/api/v1/storymaps/{mapId}/segments/{segmentId}/route-animations/{animation.RouteAnimationId}", animation),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("CreateRouteAnimation")
            .WithDescription("Create a new route animation")
            .WithTags(Tags.StoryMaps)
            .Produces<RouteAnimationDto>(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // PUT update route animation
        group.MapPut(Routes.StoryMapEndpoints.UpdateRouteAnimation, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromRoute] Guid routeAnimationId,
                [FromBody] UpdateRouteAnimationRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.UpdateRouteAnimationAsync(routeAnimationId, request, ct);
                return result.Match<IResult>(
                    animation => Results.Ok(animation),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("UpdateRouteAnimation")
            .WithDescription("Update a route animation")
            .WithTags(Tags.StoryMaps)
            .Produces<RouteAnimationDto>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // DELETE route animation
        group.MapDelete(Routes.StoryMapEndpoints.DeleteRouteAnimation, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromRoute] Guid routeAnimationId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.DeleteRouteAnimationAsync(routeAnimationId, ct);
                return result.Match<IResult>(
                    _ => Results.NoContent(),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("DeleteRouteAnimation")
            .WithDescription("Delete a route animation")
            .WithTags(Tags.StoryMaps)
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);
    }
}

