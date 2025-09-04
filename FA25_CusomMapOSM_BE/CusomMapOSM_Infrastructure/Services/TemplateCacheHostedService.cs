using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CusomMapOSM_Infrastructure.Services;

namespace CusomMapOSM_Infrastructure.Services;

public class TemplateCacheHostedService : IHostedService
{
    private readonly TemplateCacheManager _cacheManager;
    private readonly ILogger<TemplateCacheHostedService> _logger;
    private Timer? _timer;

    public TemplateCacheHostedService(
        TemplateCacheManager cacheManager,
        ILogger<TemplateCacheHostedService> logger)
    {
        _cacheManager = cacheManager;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Template Cache Hosted Service starting...");

        // Warmup cache on startup
        await _cacheManager.WarmupCacheAsync();

        // Setup timer to refresh popular templates every 24 hours
        _timer = new Timer(
            async state => await RefreshPopularTemplatesAsync(),
            null,
            TimeSpan.FromHours(24), // First run after 24 hours
            TimeSpan.FromHours(24)  // Repeat every 24 hours
        );

        _logger.LogInformation("Template Cache Hosted Service started");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Template Cache Hosted Service stopping...");

        _timer?.Change(Timeout.Infinite, 0);
        _timer?.Dispose();

        _logger.LogInformation("Template Cache Hosted Service stopped");
        return Task.CompletedTask;
    }

    private async Task RefreshPopularTemplatesAsync()
    {
        try
        {
            await _cacheManager.RefreshPopularTemplatesAsync();
            _logger.LogInformation("Popular templates cache refreshed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh popular templates cache");
        }
    }
}
