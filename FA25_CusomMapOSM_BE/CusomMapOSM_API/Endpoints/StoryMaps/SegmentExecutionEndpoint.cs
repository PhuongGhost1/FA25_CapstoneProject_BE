using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.StoryMaps;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.StoryMaps;

public class SegmentExecutionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.StoryMap)
            .WithTags(Tags.StoryMaps)
            .WithDescription("Segment Execution")
            .RequireAuthorization();

        group.MapPost("/{mapId:guid}/segments/{segmentId:guid}/execute", async (
                [FromRoute] Guid mapId,
                [FromRoute] Guid segmentId,
                [FromBody] SegmentExecutionOptions? options,
                [FromServices] ISegmentExecutor segmentExecutor,
                [FromServices] IStoryMapService storyMapService,
                CancellationToken ct) =>
            {
                var segmentsResult = await storyMapService.GetSegmentsAsync(mapId, ct);
                
                var segment = segmentsResult.ValueOr(Array.Empty<SegmentDto>()).FirstOrDefault(s => s.SegmentId == segmentId);
                if (segment == null)
                {
                    return Results.NotFound("Segment not found");
                }
                
                var result = await segmentExecutor.ExecuteSegmentAsync(segment, options, ct);

                var response = new ExecuteSegmentResponse
                {
                    Success = result.IsSuccess,
                    Message = result.IsSuccess ? "Segment executed successfully" : result.ErrorMessage,
                    Duration = result.Duration.TotalMilliseconds,
                    ExecutedComponents = result.ExecutedComponents.Select(c => new ExecutedComponentDto
                    {
                        Type = c.Type.ToString(),
                        Name = c.Name,
                        Order = c.Order,
                        Duration = c.Duration.TotalMilliseconds,
                        IsSuccess = c.IsSuccess,
                        ErrorMessage = c.ErrorMessage
                    }).ToList()
                };

                return Results.Ok(response);
            })
            .WithName("ExecuteSegment")
            .WithDescription("Execute a segment with all its components")
            .WithTags(Tags.StoryMaps)
            .Accepts<SegmentExecutionOptions>("application/json")
            .Produces<ExecuteSegmentResponse>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);

        group.MapPost("/{mapId:guid}/segments/execute-all", async (
                [FromRoute] Guid mapId,
                [FromBody] SegmentExecutionOptions? options,
                [FromServices] ISegmentExecutor segmentExecutor,
                [FromServices] IStoryMapService storyMapService,
                CancellationToken ct) =>
            {
                var segmentsResult = await storyMapService.GetSegmentsAsync(mapId, ct);
                if (!segmentsResult.HasValue)
                {
                    return Results.NotFound("Map not found");
                }

                var segments = segmentsResult.ValueOr(Array.Empty<SegmentDto>()).ToList();
                if (segments.Count == 0)
                {
                    var emptyResponse = new ExecuteAllSegmentsResponse
                    {
                        Success = true,
                        Message = "No segments to execute",
                        TotalSegments = 0,
                        TotalDuration = 0,
                        Results = Array.Empty<SegmentExecutionSummaryDto>()
                    };
                    return Results.Ok(emptyResponse);
                }

                var executionOptions = options ?? new SegmentExecutionOptions();
                var results = await segmentExecutor.ExecuteSegmentsAsync(segments, executionOptions, ct);

                var response = new ExecuteAllSegmentsResponse
                {
                    Success = results.All(r => r.IsSuccess),
                    Message = "All segments executed",
                    TotalSegments = results.Count,
                    TotalDuration = results.Sum(r => r.Duration.TotalMilliseconds),
                    Results = results.Select(r => new SegmentExecutionSummaryDto
                    {
                        SegmentId = r.Segment.SegmentId,
                        SegmentName = r.Segment.Name,
                        Success = r.IsSuccess,
                        Duration = r.Duration.TotalMilliseconds,
                        ErrorMessage = r.ErrorMessage,
                        ExecutedComponents = r.ExecutedComponents.Select(c => new ExecutedComponentDto
                        {
                            Type = c.Type.ToString(),
                            Name = c.Name,
                            Order = c.Order,
                            Duration = c.Duration.TotalMilliseconds,
                            IsSuccess = c.IsSuccess,
                            ErrorMessage = c.ErrorMessage
                        }).ToList()
                    }).ToList()
                };

                return Results.Ok(response);
            })
            .WithName("ExecuteAllSegments")
            .WithDescription("Execute all segments in a map")
            .WithTags(Tags.StoryMaps)
            .Accepts<SegmentExecutionOptions>("application/json")
            .Produces<ExecuteAllSegmentsResponse>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);

        // GET /story-maps/{mapId}/execution/checkpoint
        group.MapGet("/{mapId:guid}/execution/checkpoint", (
                [FromRoute] Guid mapId,
                [FromServices] ISegmentExecutionStateStore stateStore) =>
            {
                var cp = stateStore.Get(mapId);
                return cp != null ? Results.Ok(cp) : Results.NotFound();
            })
            .WithName("GetExecutionCheckpoint")
            .WithDescription("Get current execution checkpoint for a map")
            .WithTags(Tags.StoryMaps)
            .Produces<SegmentExecutionCheckpoint>(200)
            .ProducesProblem(404);

        // POST /story-maps/{mapId}/execution/checkpoint/reset
        group.MapPost("/{mapId:guid}/execution/checkpoint/reset", (
                [FromRoute] Guid mapId,
                [FromServices] ISegmentExecutionStateStore stateStore) =>
            {
                stateStore.Reset(mapId);
                return Results.Ok(new { success = true });
            })
            .WithName("ResetExecutionCheckpoint")
            .WithDescription("Reset execution checkpoint for a map")
            .WithTags(Tags.StoryMaps)
            .Produces(200);

        // Execution control endpoints
        group.MapGet("/execution/status", async (
                [FromServices] ISegmentExecutor segmentExecutor,
                [FromServices] ISegmentExecutionStateStore stateStore) =>
            {
                var status = segmentExecutor.GetExecutionStatus();
                var checkpoint = stateStore.Get(Guid.Empty);
                var response = new ExecutionStatusDto
                {
                    Status = status.ToString(),
                    IsRunning = status == SegmentExecutionStatus.Running,
                    IsPaused = status == SegmentExecutionStatus.Paused,
                    IsIdle = status == SegmentExecutionStatus.Idle
                };
                return Results.Ok(response);
            })
            .WithName("GetExecutionStatus")
            .WithDescription("Get current segment execution status")
            .WithTags(Tags.StoryMaps)
            .Produces<ExecutionStatusDto>(200);

        group.MapPost("/execution/stop", async (
                [FromServices] ISegmentExecutor segmentExecutor,
                [FromServices] ISegmentExecutionStateStore stateStore) =>
            {
                segmentExecutor.StopExecution();
                // Optionally keep checkpoint for resume later, or reset here
                var response = new ExecutionControlResponse
                {
                    Success = true,
                    Message = "Execution stopped"
                };
                return Results.Ok(response);
            })
            .WithName("StopExecution")
            .WithDescription("Stop current segment execution")
            .WithTags(Tags.StoryMaps)
            .Produces<ExecutionControlResponse>(200);

        group.MapPost("/execution/pause", async (
                [FromServices] ISegmentExecutor segmentExecutor,
                [FromServices] ISegmentExecutionStateStore stateStore) =>
            {
                segmentExecutor.PauseExecution();
                var response = new ExecutionControlResponse
                {
                    Success = true,
                    Message = "Execution paused"
                };
                return Results.Ok(response);
            })
            .WithName("PauseExecution")
            .WithDescription("Pause current segment execution")
            .WithTags(Tags.StoryMaps)
            .Produces<ExecutionControlResponse>(200);

        group.MapPost("/execution/resume", async (
                [FromServices] ISegmentExecutor segmentExecutor,
                [FromServices] ISegmentExecutionStateStore stateStore) =>
            {
                segmentExecutor.ResumeExecution();
                var response = new ExecutionControlResponse
                {
                    Success = true,
                    Message = "Execution resumed"
                };
                return Results.Ok(response);
            })
            .WithName("ResumeExecution")
            .WithDescription("Resume paused segment execution")
            .WithTags(Tags.StoryMaps)
            .Produces<ExecutionControlResponse>(200);
    }
}
