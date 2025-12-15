using CusomMapOSM_Infrastructure.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CusomMapOSM_Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring background jobs during application startup
/// </summary>
public static class BackgroundJobExtensions
{
    /// <summary>
    /// Initialize and register all background jobs with Hangfire
    /// This should be called during application startup
    /// </summary>
    public static IHost InitializeBackgroundJobs(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<BackgroundJobScheduler>>();
        var scheduler = scope.ServiceProvider.GetRequiredService<BackgroundJobScheduler>();

        try
        {
            logger.LogInformation("Initializing background jobs...");
            scheduler.RegisterAllRecurringJobs();
            logger.LogInformation("Background jobs initialized successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize background jobs");
            throw;
        }

        return host;
    }

    /// <summary>
    /// Remove all background jobs (useful for testing or maintenance)
    /// </summary>
    public static IHost RemoveBackgroundJobs(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<BackgroundJobScheduler>>();
        var scheduler = scope.ServiceProvider.GetRequiredService<BackgroundJobScheduler>();

        try
        {
            logger.LogInformation("Removing all background jobs...");
            scheduler.RemoveAllRecurringJobs();
            logger.LogInformation("Background jobs removed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove background jobs");
            throw;
        }

        return host;
    }

    /// <summary>
    /// Get status of all background jobs
    /// </summary>
    public static Dictionary<string, object> GetBackgroundJobStatuses(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var scheduler = scope.ServiceProvider.GetRequiredService<BackgroundJobScheduler>();

        return scheduler.GetJobStatuses();
    }
}
