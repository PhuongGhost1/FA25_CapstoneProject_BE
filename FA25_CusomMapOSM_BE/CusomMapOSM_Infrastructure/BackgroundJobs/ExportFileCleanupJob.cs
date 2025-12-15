using CusomMapOSM_Infrastructure.Databases;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CusomMapOSM_Infrastructure.BackgroundJobs;

/// <summary>
/// Background job to clean up expired export files
/// Implements BR-20: Export files are retained for 30 days before automatic deletion (FR-31)
/// Runs daily to remove files older than 30 days
/// </summary>
public class ExportFileCleanupJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExportFileCleanupJob> _logger;

    public ExportFileCleanupJob(
        IServiceProvider serviceProvider,
        ILogger<ExportFileCleanupJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [Queue("default")]
    [AutomaticRetry(Attempts = 3)]
    public async Task CleanupExpiredExportFilesAsync()
    {
        try
        {
            _logger.LogInformation("Starting expired export file cleanup");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();

            var expirationDate = DateTime.UtcNow.AddDays(-30); // 30 days ago

            var expiredExports = await dbContext.Exports
                .Where(e => e.CreatedAt < expirationDate)
                .ToListAsync();

            var cleanedCount = 0;
            foreach (var export in expiredExports)
            {
                await ProcessExpiredExportAsync(export, dbContext);
                cleanedCount++;
            }

            await dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Expired export file cleanup completed. Cleaned {Count} files",
                cleanedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cleaning up expired export files");
            throw;
        }
    }

    private async Task ProcessExpiredExportAsync(
        CusomMapOSM_Domain.Entities.Exports.Export export,
        CustomMapOSMDbContext dbContext)
    {
        try
        {
            // Delete physical file if it exists
            if (!string.IsNullOrEmpty(export.FilePath) && File.Exists(export.FilePath))
            {
                File.Delete(export.FilePath);
                _logger.LogDebug("Deleted physical file: {FilePath}", export.FilePath);
            }

            // Remove database record
            dbContext.Exports.Remove(export);

            _logger.LogInformation(
                "Removed expired export {ExportId} created on {CreatedAt}",
                export.ExportId, export.CreatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process expired export {ExportId}",
                export.ExportId);
            throw;
        }
    }
}
