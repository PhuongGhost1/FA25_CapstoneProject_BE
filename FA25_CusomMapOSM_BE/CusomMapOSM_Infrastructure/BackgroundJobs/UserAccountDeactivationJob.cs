using CusomMapOSM_Infrastructure.Databases;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CusomMapOSM_Infrastructure.BackgroundJobs;

/// <summary>
/// Background job to deactivate inactive user accounts
/// Implements BR-28: User accounts are automatically deactivated after 2 years of inactivity (FR-01, FR-02)
/// Runs weekly to check for inactive accounts
/// </summary>
public class UserAccountDeactivationJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserAccountDeactivationJob> _logger;

    public UserAccountDeactivationJob(
        IServiceProvider serviceProvider,
        ILogger<UserAccountDeactivationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [Queue("default")]
    [AutomaticRetry(Attempts = 3)]
    public async Task DeactivateInactiveAccountsAsync()
    {
        try
        {
            _logger.LogInformation("Starting inactive user account deactivation");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();

            var cutoffDate = DateTime.UtcNow.AddYears(-2); // 2 years ago

            var inactiveUsers = await dbContext.Users
                .Where(u => u.LastLogin < cutoffDate && u.AccountStatus!.Name == "Active")
                .ToListAsync();

            var deactivatedCount = 0;
            foreach (var user in inactiveUsers)
            {
                await DeactivateUserAccountAsync(user, dbContext);
                deactivatedCount++;
            }

            await dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Inactive user account deactivation completed. Deactivated {Count} accounts",
                deactivatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deactivating inactive accounts");
            throw;
        }
    }

    private async Task DeactivateUserAccountAsync(
        CusomMapOSM_Domain.Entities.Users.User user,
        CustomMapOSMDbContext dbContext)
    {
        try
        {
            // Update account status to inactive
            var inactiveStatus = await dbContext.AccountStatuses
                .FirstOrDefaultAsync(ast => ast.Name == "Inactive");

            if (inactiveStatus != null)
            {
                user.AccountStatusId = inactiveStatus.StatusId;
            }

            _logger.LogInformation(
                "Deactivated inactive user account {UserId} (last login: {LastLogin})",
                user.UserId, user.LastLogin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to deactivate user account {UserId}",
                user.UserId);
            throw;
        }
    }
}
