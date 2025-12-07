using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Services.Assets;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Assets;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Assets;

public class UserAssetEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.Assets)
            .WithTags("User Assets")
            .WithDescription("Manage user uploaded assets (images, audio)");

        group.MapGet(Routes.UserAssetEndpoints.GetAll, async (
            [FromQuery] string? type,
            [FromQuery] Guid? orgId,
            [FromServices] IUserAssetService assetService,
            [FromServices] ICurrentUserService currentUserService) =>
        {
            var userId = currentUserService.GetUserId();
            if (!userId.HasValue)
            {
                return Results.Unauthorized();
            }
            var result = await assetService.GetUserAssetsAsync(userId.Value, orgId, type);
            return Results.Ok(result);
        })
        .WithName("GetUserAssets")
        .RequireAuthorization()
        .Produces<List<UserAssetDto>>(200);

        group.MapPost(Routes.UserAssetEndpoints.Upload, async (
            IFormFile file,
            [FromForm] string? type,
            [FromServices] IUserAssetService assetService,
            [FromServices] ICurrentUserService currentUserService,
            CancellationToken ct) =>
        {
            
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest("No file uploaded");
            }

            var request = new UploadAssetRequest
            {
                File = file,
                Type = type ?? "image"
            };

            var userId = currentUserService.GetUserId();
            if (!userId.HasValue)
            {
                return Results.Unauthorized();
            }

            try
            {
                var result = await assetService.UploadAssetAsync(userId.Value, request);
                return Results.Created($"/api/v1/assets/{result.Id}", result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("UploadUserAsset")
        .RequireAuthorization()
        .DisableAntiforgery()
        .Accepts<IFormFile>("multipart/form-data")
        .Produces<UserAssetDto>(201);

        group.MapDelete(Routes.UserAssetEndpoints.Delete, async (
            [FromRoute] Guid id,
            [FromServices] IUserAssetService assetService,
            [FromServices] ICurrentUserService currentUserService,
            CancellationToken ct) =>
        {
            var userId = currentUserService.GetUserId();
            if (!userId.HasValue)
            {
                return Results.Unauthorized();
            }
            await assetService.DeleteAssetAsync(userId.Value, id);
            return Results.Ok();
        })
        .WithName("DeleteUserAsset")
        .RequireAuthorization()
        .Produces(200);
    }
}
