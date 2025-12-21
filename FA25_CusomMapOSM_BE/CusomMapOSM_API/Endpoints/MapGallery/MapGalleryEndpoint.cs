using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.MapGallery;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.Documents;
using CusomMapOSM_Application.Models.DTOs.Features.MapGallery;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.MapGallery;

public class MapGalleryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("map-gallery")
            .WithTags("MapGallery")
            .WithDescription("Community map gallery and submissions");

        // Public APIs - Get published maps
        group.MapGet("/maps", async (
                [FromServices] IMapGalleryService service,
                [FromQuery] string? category,
                [FromQuery] string? search,
                [FromQuery] bool? featured,
                CancellationToken ct) =>
            {
                MapTemplateCategoryEnum? categoryEnum = null;
                if (!string.IsNullOrEmpty(category) && Enum.TryParse<MapTemplateCategoryEnum>(category, true, out var parsed))
                {
                    categoryEnum = parsed;
                }

                var result = await service.GetPublishedMapsAsync(categoryEnum, search, featured, ct);
                return Results.Ok(result);
            })
            .WithName("GetPublishedMaps")
            .WithDescription("Lấy danh sách bản đồ đã được duyệt và publish")
            .AllowAnonymous()
            .Produces<List<MapGallerySummaryResponse>>(200);

        group.MapGet("/maps/{id}", async (
                [FromServices] IMapGalleryService service,
                [FromRoute] string id,
                CancellationToken ct) =>
            {
                var result = await service.GetPublishedMapByIdAsync(id, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetPublishedMapById")
            .WithDescription("Lấy chi tiết bản đồ đã được duyệt")
            .AllowAnonymous()
            .Produces<MapGalleryDetailResponse>(200)
            .ProducesProblem(404);

        group.MapGet("/maps/by-map-id/{mapId:guid}", async (
                [FromServices] IMapGalleryService service,
                [FromRoute] Guid mapId,
                CancellationToken ct) =>
            {
                var result = await service.GetPublishedMapByMapIdAsync(mapId, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetPublishedMapByMapId")
            .WithDescription("Lấy chi tiết bản đồ theo MapId")
            .AllowAnonymous()
            .Produces<MapGalleryDetailResponse>(200)
            .ProducesProblem(404);

        // User APIs - Submit map
        var userGroup = group.MapGroup("")
            .RequireAuthorization();

        userGroup.MapPost("/submit", async (
                [FromServices] IMapGalleryService service,
                [FromServices] ICurrentUserService currentUserService,
                [FromBody] MapGallerySubmitRequest request,
                CancellationToken ct) =>
            {
                var userId = currentUserService.GetUserId();
                if (userId == null)
                {
                    return Results.Unauthorized();
                }

                var result = await service.SubmitMapAsync(userId.Value, request, ct);
                return result.Match(
                    success => Results.Created($"/map-gallery/maps/{success.Id}", success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("SubmitMap")
            .WithDescription("Submit bản đồ để đăng lên gallery")
            .Produces<MapGalleryDetailResponse>(201)
            .ProducesProblem(400)
            .ProducesProblem(403)
            .ProducesProblem(404)
            .ProducesProblem(409);

        userGroup.MapGet("/my-submission/{mapId:guid}", async (
                [FromServices] IMapGalleryService service,
                [FromServices] ICurrentUserService currentUserService,
                [FromRoute] Guid mapId,
                CancellationToken ct) =>
            {
                var userId = currentUserService.GetUserId();
                if (userId == null)
                {
                    return Results.Unauthorized();
                }

                var result = await service.GetMySubmissionAsync(userId.Value, mapId, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("GetMySubmission")
            .WithDescription("Lấy submission của user cho một map")
            .Produces<MapGalleryDetailResponse>(200)
            .ProducesProblem(404);

        userGroup.MapPut("/my-submission/{id}", async (
                [FromServices] IMapGalleryService service,
                [FromServices] ICurrentUserService currentUserService,
                [FromRoute] string id,
                [FromBody] MapGalleryUpdateRequest request,
                CancellationToken ct) =>
            {
                var userId = currentUserService.GetUserId();
                if (userId == null)
                {
                    return Results.Unauthorized();
                }

                var result = await service.UpdateMySubmissionAsync(userId.Value, id, request, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("UpdateMySubmission")
            .WithDescription("Cập nhật submission của user")
            .Produces<MapGalleryDetailResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(403)
            .ProducesProblem(404);

        userGroup.MapPost("/maps/{galleryId}/duplicate", async (
                [FromServices] IMapGalleryService service,
                [FromServices] ICurrentUserService currentUserService,
                [FromRoute] string galleryId,
                [FromBody] MapGalleryDuplicateRequest request,
                CancellationToken ct) =>
            {
                var userId = currentUserService.GetUserId();
                if (userId == null)
                {
                    return Results.Unauthorized();
                }

                var result = await service.DuplicateMapFromGalleryAsync(userId.Value, galleryId, request, ct);
                return result.Match(
                    success => Results.Created($"/maps/{success.MapId}", success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("DuplicateMapFromGallery")
            .WithDescription("Duplicate map từ gallery")
            .Produces<MapGalleryDuplicateResponse>(201)
            .ProducesProblem(400)
            .ProducesProblem(404);

        // View and Like APIs
        group.MapPost("/maps/{id}/view", async (
                [FromServices] IMapGalleryService service,
                [FromRoute] string id,
                CancellationToken ct) =>
            {
                var result = await service.IncrementViewCountAsync(id, ct);
                return result.Match(
                    _ => Results.Ok(new { success = true }),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("IncrementMapView")
            .WithDescription("Tăng view count cho map")
            .AllowAnonymous()
            .Produces(200)
            .ProducesProblem(404);

        userGroup.MapPost("/maps/{id}/like", async (
                [FromServices] IMapGalleryService service,
                [FromServices] ICurrentUserService currentUserService,
                [FromRoute] string id,
                CancellationToken ct) =>
            {
                var userId = currentUserService.GetUserId();
                if (userId == null)
                {
                    return Results.Unauthorized();
                }

                var result = await service.ToggleLikeAsync(id, userId.Value, ct);
                return result.Match(
                    isLiked => Results.Ok(new { success = true, isLiked }),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("ToggleMapLike")
            .WithDescription("Like/Unlike map")
            .Produces(200)
            .ProducesProblem(404);

        // Admin APIs
        var adminGroup = group.MapGroup("/admin")
            .RequireAuthorization();

        adminGroup.MapGet("/submissions", async (
                [FromServices] IMapGalleryService service,
                [FromQuery] string? status,
                CancellationToken ct) =>
            {
                MapGalleryStatusEnum? statusEnum = null;
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<MapGalleryStatusEnum>(status, true, out var parsed))
                {
                    statusEnum = parsed;
                }

                var result = await service.AdminGetAllSubmissionsAsync(statusEnum, ct);
                return Results.Ok(result);
            })
            .WithName("AdminGetAllSubmissions")
            .WithDescription("Lấy tất cả submissions (admin)")
            .Produces<List<MapGallerySummaryResponse>>(200);

        adminGroup.MapGet("/submissions/{id}", async (
                [FromServices] IMapGalleryService service,
                [FromRoute] string id,
                CancellationToken ct) =>
            {
                var result = await service.AdminGetSubmissionByIdAsync(id, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("AdminGetSubmissionById")
            .WithDescription("Lấy chi tiết submission (admin)")
            .Produces<MapGalleryDetailResponse>(200)
            .ProducesProblem(404);

        adminGroup.MapPut("/submissions/{id}/approve", async (
                [FromServices] IMapGalleryService service,
                [FromServices] ICurrentUserService currentUserService,
                [FromRoute] string id,
                [FromBody] MapGalleryApprovalRequest request,
                CancellationToken ct) =>
            {
                var reviewerId = currentUserService.GetUserId();
                if (reviewerId == null)
                {
                    return Results.Unauthorized();
                }

                var result = await service.AdminApproveOrRejectAsync(id, reviewerId.Value, request, ct);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("AdminApproveOrRejectSubmission")
            .WithDescription("Duyệt hoặc từ chối submission (admin)")
            .Produces<MapGalleryDetailResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(404);

        adminGroup.MapDelete("/submissions/{id}", async (
                [FromServices] IMapGalleryService service,
                [FromRoute] string id,
                CancellationToken ct) =>
            {
                var result = await service.AdminDeleteSubmissionAsync(id, ct);
                return result.Match(
                    _ => Results.NoContent(),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("AdminDeleteSubmission")
            .WithDescription("Xóa submission (admin)")
            .Produces(204)
            .ProducesProblem(404);
    }
}

