using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Comments;
using CusomMapOSM_Application.Models.DTOs.Features.Comments;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Comments;

public class CommentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.Comments)
            .WithTags(Tags.Comments)
            .WithDescription("Comment management endpoints")
            .RequireAuthorization();

        // Create comment
        group.MapPost("/", async (
                [FromBody] CreateCommentRequest request,
                [FromServices] ICommentService commentService) =>
            {
                var result = await commentService.CreateComment(request);
                return result.Match(
                    success => Results.Created($"/api/comments/{success.CommentId}", success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("CreateComment")
            .WithDescription("Create a new comment on a map or layer")
            .Produces<CommentDto>(201)
            .ProducesValidationProblem();

        // Get comment by ID
        group.MapGet("/{commentId:int}", async (
                [FromRoute] int commentId,
                [FromServices] ICommentService commentService) =>
            {
                var result = await commentService.GetCommentById(commentId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetCommentById")
            .WithDescription("Get a comment by ID")
            .Produces<CommentDto>(200)
            .Produces(404);

        // Get comments by map ID
        group.MapGet("/map/{mapId:guid}", async (
                [FromRoute] Guid mapId,
                [FromServices] ICommentService commentService) =>
            {
                var result = await commentService.GetCommentsByMapId(mapId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetCommentsByMapId")
            .WithDescription("Get all comments for a map")
            .Produces<List<CommentDto>>(200);

        // Get comments by layer ID
        group.MapGet("/layer/{layerId:guid}", async (
                [FromRoute] Guid layerId,
                [FromServices] ICommentService commentService) =>
            {
                var result = await commentService.GetCommentsByLayerId(layerId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetCommentsByLayerId")
            .WithDescription("Get all comments for a layer")
            .Produces<List<CommentDto>>(200);

        // Update comment
        group.MapPut("/{commentId:int}", async (
                [FromRoute] int commentId,
                [FromBody] UpdateCommentRequest request,
                [FromServices] ICommentService commentService) =>
            {
                var result = await commentService.UpdateComment(commentId, request);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("UpdateComment")
            .WithDescription("Update a comment")
            .Produces<CommentDto>(200)
            .ProducesValidationProblem();

        // Delete comment
        group.MapDelete("/{commentId:int}", async (
                [FromRoute] int commentId,
                [FromServices] ICommentService commentService) =>
            {
                var result = await commentService.DeleteComment(commentId);
                return result.Match(
                    success => success ? Results.Ok(new { message = "Comment deleted successfully" }) : Results.BadRequest(),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("DeleteComment")
            .WithDescription("Delete a comment")
            .Produces(200)
            .Produces(404);
    }
}

