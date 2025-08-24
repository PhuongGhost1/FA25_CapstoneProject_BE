using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;

namespace CusomMapOSM_Infrastructure.Extensions;

public static class HangfireExtensions
{
    public static IApplicationBuilder UseHangfireDashboard(this IApplicationBuilder app, string pathMatch = "/hangfire")
    {
        return app.UseHangfireDashboard(pathMatch, new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() }
        });
    }
}

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // TODO: Implement proper authorization
        // For now, allow all access in development
        return true;
    }
}
