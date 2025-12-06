using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Bookmarks;
using CusomMapOSM_Application.Models.DTOs.Features.Bookmarks;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Bookmarks;

public class BookmarkEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.Bookmarks)
            .WithTags(Tags.Bookmarks)
            .WithDescription("Bookmark management endpoints")
            .RequireAuthorization();

        // Create bookmark
        group.MapPost("/", async (
                [FromBody] CreateBookmarkRequest request,
                [FromServices] IBookmarkService bookmarkService) =>
            {
                var result = await bookmarkService.CreateBookmark(request);
                return result.Match(
                    success => Results.Created($"/api/bookmarks/{success.BookmarkId}", success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("CreateBookmark")
            .WithDescription("Create a new bookmark for a map")
            .Produces<BookmarkDto>(201)
            .ProducesValidationProblem();

        // Get my bookmarks
        group.MapGet("/my", async (
                [FromServices] IBookmarkService bookmarkService) =>
            {
                var result = await bookmarkService.GetMyBookmarks();
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetMyBookmarks")
            .WithDescription("Get all bookmarks for the current user")
            .Produces<List<BookmarkDto>>(200);

        // Get bookmark by ID
        group.MapGet("/{bookmarkId:int}", async (
                [FromRoute] int bookmarkId,
                [FromServices] IBookmarkService bookmarkService) =>
            {
                var result = await bookmarkService.GetBookmarkById(bookmarkId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetBookmarkById")
            .WithDescription("Get a bookmark by ID")
            .Produces<BookmarkDto>(200)
            .Produces(404);

        // Get bookmarks by map ID
        group.MapGet("/map/{mapId:guid}", async (
                [FromRoute] Guid mapId,
                [FromServices] IBookmarkService bookmarkService) =>
            {
                var result = await bookmarkService.GetBookmarksByMapId(mapId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetBookmarksByMapId")
            .WithDescription("Get all bookmarks for a map")
            .Produces<List<BookmarkDto>>(200);

        // Update bookmark
        group.MapPut("/{bookmarkId:int}", async (
                [FromRoute] int bookmarkId,
                [FromBody] UpdateBookmarkRequest request,
                [FromServices] IBookmarkService bookmarkService) =>
            {
                var result = await bookmarkService.UpdateBookmark(bookmarkId, request);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("UpdateBookmark")
            .WithDescription("Update a bookmark")
            .Produces<BookmarkDto>(200)
            .ProducesValidationProblem();

        // Delete bookmark
        group.MapDelete("/{bookmarkId:int}", async (
                [FromRoute] int bookmarkId,
                [FromServices] IBookmarkService bookmarkService) =>
            {
                var result = await bookmarkService.DeleteBookmark(bookmarkId);
                return result.Match(
                    success => success ? Results.Ok(new { message = "Bookmark deleted successfully" }) : Results.BadRequest(),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("DeleteBookmark")
            .WithDescription("Delete a bookmark")
            .Produces(200)
            .Produces(404);
    }
}

