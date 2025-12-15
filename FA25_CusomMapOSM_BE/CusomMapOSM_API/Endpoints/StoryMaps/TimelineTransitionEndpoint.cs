using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.StoryMaps;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.StoryMaps;

public class TimelineTransitionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.StoryMap)
            .WithTags(Tags.StoryMaps)
            .WithDescription("Timeline Transition Management")
            .RequireAuthorization();

        MapTimelineTransitionEndpoints(group);
    }

    private static void MapTimelineTransitionEndpoints(RouteGroupBuilder group)
    {
        // GET all timeline transitions for a map
        group.MapGet(Routes.StoryMapEndpoints.GetTimelineTransitions, async (
                [FromRoute] Guid mapId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.GetTimelineTransitionsAsync(mapId, ct);
                return result.Match<IResult>(
                    transitions => Results.Ok(transitions),
                    err => err.ToProblemDetailsResult());
            })
            .AllowAnonymous()
            .WithName("GetTimelineTransitions")
            .WithDescription("Retrieve all timeline transitions for a map")
            .WithTags(Tags.StoryMaps)
            .Produces<IEnumerable<TimelineTransitionDto>>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // GET timeline transition by ID
        group.MapGet(Routes.StoryMapEndpoints.GetTimelineTransition, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid transitionId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.GetTimelineTransitionAsync(transitionId, ct);
                return result.Match<IResult>(
                    transition => Results.Ok(transition),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GetTimelineTransition")
            .WithDescription("Retrieve a specific timeline transition")
            .WithTags(Tags.StoryMaps)
            .Produces<TimelineTransitionDto>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // POST create timeline transition
        group.MapPost(Routes.StoryMapEndpoints.CreateTimelineTransition, async (
                [FromRoute] Guid mapId,
                [FromBody] CreateTimelineTransitionRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var enrichedRequest = request with { MapId = mapId };
                var result = await service.CreateTimelineTransitionAsync(enrichedRequest, ct);
                return result.Match<IResult>(
                    transition => Results.Created($"{Routes.Prefix.StoryMap}/{mapId}/timeline-transitions/{transition.TimelineTransitionId}", transition),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("CreateTimelineTransition")
            .WithDescription("Create a new timeline transition between segments")
            .WithTags(Tags.StoryMaps)
            .Produces<TimelineTransitionDto>(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // PUT update timeline transition
        group.MapPut(Routes.StoryMapEndpoints.UpdateTimelineTransition, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid transitionId,
                [FromBody] UpdateTimelineTransitionRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.UpdateTimelineTransitionAsync(transitionId, request, ct);
                return result.Match<IResult>(
                    transition => Results.Ok(transition),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("UpdateTimelineTransition")
            .WithDescription("Update an existing timeline transition")
            .WithTags(Tags.StoryMaps)
            .Produces<TimelineTransitionDto>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // DELETE timeline transition
        group.MapDelete(Routes.StoryMapEndpoints.DeleteTimelineTransition, async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid transitionId,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.DeleteTimelineTransitionAsync(transitionId, ct);
                return result.Match<IResult>(
                    _ => Results.NoContent(),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("DeleteTimelineTransition")
            .WithDescription("Delete a timeline transition")
            .WithTags(Tags.StoryMaps)
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // POST generate smart transition
        group.MapPost(Routes.StoryMapEndpoints.GenerateTransition, async (
                [FromRoute] Guid mapId,
                [FromBody] GenerateTimelineTransitionRequest request,
                [FromServices] IStoryMapService service,
                CancellationToken ct) =>
            {
                var result = await service.GenerateTimelineTransitionAsync(mapId, request, ct);
                return result.Match<IResult>(
                    transition => Results.Created($"{Routes.Prefix.StoryMap}/{mapId}/timeline-transitions/{transition.TimelineTransitionId}", transition),
                    err => err.ToProblemDetailsResult());
            })
            .WithName("GenerateTimelineTransition")
            .WithDescription("Auto-generate a smart timeline transition between two segments")
            .WithTags(Tags.StoryMaps)
            .Produces<TimelineTransitionDto>(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409)
            .ProducesProblem(500);
    }
}
