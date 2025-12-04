using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Community;
using CusomMapOSM_Application.Models.DTOs.Features.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Community;

public class CommunityEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("community")
            .WithTags("Community")
            .WithDescription("Community posts and updates");

        // Public APIs
        group.MapGet("/posts", async (
                [FromServices] ICommunityService service,
                [FromQuery] string? topic,
                CancellationToken ct) =>
            {
                var result = await service.GetPublishedPostsAsync(topic, ct);
                return Results.Ok(result);
            })
            .WithName("GetCommunityPosts")
            .WithDescription("Danh sách bài viết cộng đồng đã publish")
            .AllowAnonymous()
            .Produces<List<CommunityPostSummaryResponse>>(200);

        group.MapGet("/posts/{slug}", async (
                [FromServices] ICommunityService service,
                [FromRoute] string slug,
                CancellationToken ct) =>
            {
                var result = await service.GetPostBySlugAsync(slug, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetCommunityPostBySlug")
            .WithDescription("Chi tiết bài viết cộng đồng (theo slug)")
            .AllowAnonymous()
            .Produces<CommunityPostDetailResponse>(200)
            .ProducesProblem(404);

        // Admin APIs
        var admin = group.MapGroup("/admin")
            .RequireAuthorization(); // tuỳ hệ thống role, có thể thêm policy/role sau

        admin.MapGet("/posts", async (
                [FromServices] ICommunityService service,
                CancellationToken ct) =>
            {
                var result = await service.AdminGetPostsAsync(ct);
                return Results.Ok(result);
            })
            .WithName("AdminGetCommunityPosts")
            .Produces<List<CommunityPostSummaryResponse>>(200);

        admin.MapGet("/posts/{id}", async (
                [FromServices] ICommunityService service,
                [FromRoute] string id,
                CancellationToken ct) =>
            {
                var result = await service.AdminGetPostByIdAsync(id, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("AdminGetCommunityPostById")
            .Produces<CommunityPostDetailResponse>(200)
            .ProducesProblem(404);

        admin.MapPost("/posts", async (
                [FromServices] ICommunityService service,
                [FromBody] CommunityPostCreateRequest request,
                CancellationToken ct) =>
            {
                var result = await service.AdminCreatePostAsync(request, ct);
                return result.Match(
                    success => Results.Created($"/community/admin/posts/{success.Id}", success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("AdminCreateCommunityPost")
            .Produces<CommunityPostDetailResponse>(201)
            .ProducesProblem(400)
            .ProducesProblem(409);

        admin.MapPut("/posts/{id}", async (
                [FromServices] ICommunityService service,
                [FromRoute] string id,
                [FromBody] CommunityPostUpdateRequest request,
                CancellationToken ct) =>
            {
                var result = await service.AdminUpdatePostAsync(id, request, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("AdminUpdateCommunityPost")
            .Produces<CommunityPostDetailResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(404);

        admin.MapDelete("/posts/{id}", async (
                [FromServices] ICommunityService service,
                [FromRoute] string id,
                CancellationToken ct) =>
            {
                var result = await service.AdminDeletePostAsync(id, ct);
                return result.Match(
                    _ => Results.NoContent(),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("AdminDeleteCommunityPost")
            .Produces(204)
            .ProducesProblem(404);
    }
}


