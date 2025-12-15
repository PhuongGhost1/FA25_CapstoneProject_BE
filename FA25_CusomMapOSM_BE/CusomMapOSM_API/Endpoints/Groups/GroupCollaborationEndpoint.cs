using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Groups;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Groups;

public class GroupCollaborationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/groups")
            .WithTags("Group Collaboration")
            .WithDescription("Group collaboration for team-based learning");

        // Get Groups by Session (Query endpoint - kept for initial load)
        group.MapGet("/sessions/{sessionId:guid}", async (
                [FromRoute] Guid sessionId,
                [FromServices] IGroupCollaborationService groupService) =>
            {
                var result = await groupService.GetGroupsBySession(sessionId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetGroupsBySession")
            .WithDescription("Get all groups in a session (for initial load)")
            .Produces(200)
            .Produces(404);

        // Get Group by ID (Query endpoint - kept for details view)
        group.MapGet("/{groupId:guid}", async (
                [FromRoute] Guid groupId,
                [FromServices] IGroupCollaborationService groupService) =>
            {
                var result = await groupService.GetGroupById(groupId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetGroupById")
            .WithDescription("Get group details with members")
            .Produces(200)
            .Produces(404);

        // Delete Group (Management endpoint - kept)
        group.MapDelete("/{groupId:guid}", async (
                [FromRoute] Guid groupId,
                [FromServices] IGroupCollaborationService groupService) =>
            {
                var result = await groupService.DeleteGroup(groupId);
                return result.Match(
                    success => Results.Ok(new { message = "Group deleted successfully" }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("DeleteGroup")
            .WithDescription("Delete a group (teacher only)")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // Get Group Submissions (Query endpoint - kept for history view)
        group.MapGet("/{groupId:guid}/submissions", async (
                [FromRoute] Guid groupId,
                [FromServices] IGroupCollaborationService groupService) =>
            {
                var result = await groupService.GetGroupSubmissions(groupId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetGroupSubmissions")
            .WithDescription("Get all submissions for a group")
            .Produces(200)
            .Produces(404);

        // Get Session Submissions (Query endpoint - kept for teacher dashboard)
        group.MapGet("/sessions/{sessionId:guid}/submissions", async (
                [FromRoute] Guid sessionId,
                [FromServices] IGroupCollaborationService groupService) =>
            {
                var result = await groupService.GetSessionSubmissions(sessionId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetSessionSubmissions")
            .WithDescription("Get all group submissions in a session")
            .Produces(200)
            .Produces(404);

        // Note: CreateGroup, SubmitGroupWork, and GradeSubmission moved to SignalR Hub
        // for real-time notifications to all participants
    }
}
