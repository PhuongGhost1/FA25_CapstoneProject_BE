using System.Text.Json;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using Hangfire;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CusomMapOSM_Infrastructure.BackgroundJobs;

public class MapSelectionCleanupJob
{
    private readonly IMapSelectionService _selectionService;
    private readonly IDistributedCache _redis;
    private readonly ILogger<MapSelectionCleanupJob> _logger;
    
    public MapSelectionCleanupJob(
        IMapSelectionService selectionService,
        IDistributedCache redis,
        ILogger<MapSelectionCleanupJob> logger)
    {
        _selectionService = selectionService;

        _redis = redis;

        _logger = logger;
    }
    [AutomaticRetry(Attempts = 3)]
    public async Task CleanupAllStaleSelectionsAsync()
    {
        try
        {
            _logger.LogInformation("Starting map selection cleanup job");
            // Get all map IDs with active users
            var mapIds = await GetAllActiveMaps();
            if (!mapIds.Any())
            {
                _logger.LogInformation("No active maps found to cleanup");

                return;
            }
            var totalCleaned = 0;
            foreach (var mapId in mapIds)
            {
                try
                {
                    await _selectionService.CleanupInactiveSelections(mapId);

                    totalCleaned++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning up map {MapId}", mapId);
                }
            }
            _logger.LogInformation(
                "Map selection cleanup completed. Processed {Count} maps",
                totalCleaned);
        }
        catch (Exception ex)

        {
            _logger.LogError(ex, "Error during map selection cleanup job");
            throw; // Re-throw for Hangfire retry
        }
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task CleanupMapSelectionsAsync(Guid mapId)

    {
        try

        {
            _logger.LogInformation("Cleaning up selections for map {MapId}", mapId);
            await _selectionService.CleanupInactiveSelections(mapId);
            _logger.LogInformation("Cleanup completed for map {MapId}", mapId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up map {MapId}", mapId);
            throw;
        }
    }
    
    public async Task<SelectionStatistics> GetSelectionStatisticsAsync()
    {
        try
        {
            var stats = new SelectionStatistics();
            var mapIds = await GetAllActiveMaps();
            stats.TotalActiveMaps = mapIds.Count;
            foreach (var mapId in mapIds)
            {
                var activeUsersResult = await _selectionService.GetActiveUsers(mapId);
                await activeUsersResult.Match(
                    users =>
                    {
                        stats.TotalActiveUsers += users.Count;
                        stats.TotalActiveSelections += users.Count(u => u.CurrentSelection != null);
                        return Task.CompletedTask;
                    },
                    error => Task.CompletedTask
                );
            }
            _logger.LogInformation(
                "Selection statistics: {ActiveMaps} maps, {ActiveUsers} users, {Selections} selections",
                stats.TotalActiveMaps, stats.TotalActiveUsers, stats.TotalActiveSelections);
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting selection statistics");

            return new SelectionStatistics();
        }
    }
    
    #region Private Helpers
    private async Task<List<Guid>> GetAllActiveMaps()

    {
        var mapIds = new List<Guid>();
        try
        {
            var activeMapsKey = "maps:active";
            var json = await _redis.GetStringAsync(activeMapsKey);
            if (!string.IsNullOrEmpty(json))
            {
                mapIds = JsonSerializer.Deserialize<List<Guid>>(json) ?? new List<Guid>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting active maps list");
        }
        return mapIds;
    }
    #endregion
}

public class SelectionStatistics

{
    public int TotalActiveMaps { get; set; }

    public int TotalActiveUsers { get; set; }

    public int TotalActiveSelections { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}