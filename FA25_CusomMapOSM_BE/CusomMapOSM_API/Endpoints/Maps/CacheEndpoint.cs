using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Services.Cache;
using CusomMapOSM_Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Maps;

public class CacheEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var cacheGroup = app.MapGroup("/cache")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        cacheGroup.MapPost("/warmup", async (
                [FromServices] TemplateCacheManager cacheManager) =>
            {
                await cacheManager.WarmupCacheAsync();
                return Results.Ok(new { message = "Cache warmup completed" });
            })
            .WithName("WarmupTemplateCache")
            .WithDescription("Manually trigger template cache warmup");

        cacheGroup.MapPost("/clear", async (
                [FromServices] TemplateCacheManager cacheManager) =>
            {
                await cacheManager.ClearTemplateCacheAsync();
                return Results.Ok(new { message = "Template cache cleared" });
            })
            .WithName("ClearTemplateCache")
            .WithDescription("Clear all template cache");

        cacheGroup.MapGet("/status", async (
                [FromServices] ICacheService cacheService) =>
            {
                var cacheKeys = new[]
                {
                    "templates:all",
                    "templates:featured",
                    "templates:popular"
                };

                var status = new Dictionary<string, bool>();
                foreach (var key in cacheKeys)
                {
                    status[key] = await cacheService.ExistsAsync(key);
                }

                return Results.Ok(new
                {
                    cacheStatus = status,
                    timestamp = DateTime.UtcNow
                });
            })
            .WithName("GetCacheStatus")
            .WithDescription("Get template cache status");
    }
}
