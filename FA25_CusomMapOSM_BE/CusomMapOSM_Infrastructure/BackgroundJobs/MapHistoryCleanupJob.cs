using CusomMapOSM_Infrastructure.Databases;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CusomMapOSM_Domain.Entities.Maps;

namespace CusomMapOSM_Infrastructure.BackgroundJobs;

/// <summary>
/// Background job to clean up old map history records
/// Implements BR-11: Map history is retained for 1 year for audit purposes (FR-15)
/// Runs weekly to remove history older than 1 year
/// </summary>
public class MapHistoryCleanupJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MapHistoryCleanupJob> _logger;

    public MapHistoryCleanupJob(
        IServiceProvider serviceProvider,
        ILogger<MapHistoryCleanupJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [Queue("default")]
    [AutomaticRetry(Attempts = 3)]
    public async Task CleanupOldMapHistoryAsync()
    {
        try
        {
            _logger.LogInformation("Starting old map history cleanup");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();

            var expirationDate = DateTime.UtcNow.AddYears(-1); // 1 year ago

            // Assuming there's a MapHistory table - adjust based on actual schema
            var oldHistoryRecords = await dbContext.Set<MapHistory>()
                .Where(h => h.CreatedAt < expirationDate)
                .ToListAsync();

            var cleanedCount = oldHistoryRecords.Count;

            if (cleanedCount > 0)
            {
                dbContext.Set<MapHistory>().RemoveRange(oldHistoryRecords);
                await dbContext.SaveChangesAsync();
            }

            _logger.LogInformation(
                "Old map history cleanup completed. Cleaned {Count} records",
                cleanedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cleaning up old map history");
            throw;
        }
    }
}
