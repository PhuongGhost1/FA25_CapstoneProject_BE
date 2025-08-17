using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace CusomMapOSM_API.Tests;

public class TestAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public TestAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if the test user ID header is present
        if (context.Request.Headers.TryGetValue("X-Test-User-Id", out var userIdHeader))
        {
            if (Guid.TryParse(userIdHeader.ToString(), out var userId))
            {
                // Create claims for the test user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim("userId", userId.ToString()),
                    new Claim(ClaimTypes.Name, "TestUser"),
                    new Claim(ClaimTypes.Email, "test@example.com")
                };

                var identity = new ClaimsIdentity(claims, "Test");
                var principal = new ClaimsPrincipal(identity);

                // Set the user principal
                context.User = principal;
            }
        }

        await _next(context);
    }
}

public static class TestAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseTestAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TestAuthenticationMiddleware>();
    }
}
