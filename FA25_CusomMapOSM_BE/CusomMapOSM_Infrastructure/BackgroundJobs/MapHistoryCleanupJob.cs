using CusomMapOSM_Application.Interfaces.Services.Maps;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CusomMapOSM_Infrastructure.BackgroundJobs;

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
            var store = scope.ServiceProvider.GetRequiredService<IMapHistoryStore>();

            var expirationDate = DateTime.UtcNow.AddYears(-1); // 1 year ago

            var cleanedCount = await store.DeleteOlderThanAsync(expirationDate);

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
