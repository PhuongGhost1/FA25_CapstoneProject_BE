using CusomMapOSM_Application.Interfaces.Features.Usage;
using Optional.Unsafe;
using System.Security.Claims;
using System.Text.Json;

namespace CusomMapOSM_API.Middlewares;

public class QuotaCheckingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<QuotaCheckingMiddleware> _logger;

    public QuotaCheckingMiddleware(RequestDelegate next, ILogger<QuotaCheckingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUsageService usageService)
    {
        // Skip quota checking for certain endpoints
        if (ShouldSkipQuotaCheck(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Get user and organization info from claims
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier) ?? context.User.FindFirst("userId");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            await _next(context);
            return;
        }

        // Try to get organization ID from route or query parameters
        var orgId = GetOrganizationIdFromRequest(context);
        if (orgId == null)
        {
            await _next(context);
            return;
        }

        // Determine resource type and amount based on the endpoint
        var quotaInfo = GetQuotaInfoFromRequest(context);
        if (quotaInfo == null)
        {
            await _next(context);
            return;
        }

        // Check quota
        var quotaResult = await usageService.CheckUserQuotaAsync(userId, orgId.Value, quotaInfo.Value.ResourceType, quotaInfo.Value.Amount);
        if (!quotaResult.HasValue)
        {
            _logger.LogWarning("Failed to check quota for user {UserId}, org {OrgId}", userId, orgId);
            await _next(context);
            return;
        }

        var quota = quotaResult.ValueOrDefault();
        if (!quota.IsAllowed)
        {
            _logger.LogWarning("Quota exceeded for user {UserId}, org {OrgId}, resource {ResourceType}", userId, orgId, quotaInfo.Value.ResourceType);

            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = "QuotaExceeded",
                message = quota.Message,
                details = new
                {
                    resourceType = quotaInfo.Value.ResourceType,
                    currentUsage = quota.CurrentUsage,
                    limit = quota.Limit,
                    requestedAmount = quotaInfo.Value.Amount,
                    remainingQuota = quota.RemainingQuota
                }
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
            return;
        }

        // Quota check passed, continue to next middleware
        await _next(context);
    }

    private static bool ShouldSkipQuotaCheck(PathString path)
    {
        var skipPaths = new[]
        {
            "/api/auth",
            "/api/faqs",
            "/api/membership-plans",
            "/api/notifications",
            "/api/usage",
            "/api/health"
        };

        return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath));
    }

    private static Guid? GetOrganizationIdFromRequest(HttpContext context)
    {
        // Try to get from route parameters
        if (context.Request.RouteValues.TryGetValue("orgId", out var orgIdValue) &&
            Guid.TryParse(orgIdValue?.ToString(), out var orgIdFromRoute))
        {
            return orgIdFromRoute;
        }

        // Try to get from query parameters
        if (context.Request.Query.TryGetValue("orgId", out var orgIdQuery) &&
            Guid.TryParse(orgIdQuery.FirstOrDefault(), out var orgIdFromQuery))
        {
            return orgIdFromQuery;
        }

        return null;
    }

    private static (string ResourceType, int Amount)? GetQuotaInfoFromRequest(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var method = context.Request.Method.ToUpper();

        // Map creation
        if (path.Contains("/maps") && method == "POST")
        {
            return ("maps", 1);
        }

        // Map export
        if (path.Contains("/export") && method == "POST")
        {
            return ("exports", 1);
        }

        // User invitation
        if (path.Contains("/invite") && method == "POST")
        {
            return ("users", 1);
        }

        // Add more quota checks as needed
        return null;
    }
}
