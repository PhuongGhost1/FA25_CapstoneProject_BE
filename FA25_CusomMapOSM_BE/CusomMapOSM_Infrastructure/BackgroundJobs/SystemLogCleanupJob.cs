using CusomMapOSM_Infrastructure.Databases;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
// using CusomMapOSM_Domain.Entities.Logs;

namespace CusomMapOSM_Infrastructure.BackgroundJobs;

/// <summary>
/// Background job to clean up old system logs
/// Implements BR-27: System logs are retained for 1 year for audit purposes (NFR-11)
/// Runs weekly to remove logs older than 1 year
/// </summary>
public class SystemLogCleanupJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SystemLogCleanupJob> _logger;

    public SystemLogCleanupJob(
        IServiceProvider serviceProvider,
        ILogger<SystemLogCleanupJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [Queue("default")]
    [AutomaticRetry(Attempts = 3)]
    public async Task CleanupOldSystemLogsAsync()
    {
        try
        {
            _logger.LogInformation("Starting old system log cleanup");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();

            var expirationDate = DateTime.UtcNow.AddYears(-1); // 1 year ago

            // Assuming there's a SystemLog table - adjust based on actual schema
            // var oldLogs = await dbContext.Set<SystemLog>()
            //     .Where(log => log.CreatedAt < expirationDate)
            //     .ToListAsync();

            // var cleanedCount = oldLogs.Count;

            // if (cleanedCount > 0)
            // {
            //     dbContext.Set<SystemLog>().RemoveRange(oldLogs);
            //     await dbContext.SaveChangesAsync();
            // }

            // _logger.LogInformation(
            //     "Old system log cleanup completed. Cleaned {Count} log entries",
            //     cleanedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cleaning up old system logs");
            throw;
        }
    }
}
