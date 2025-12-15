using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CusomMapOSM_Infrastructure.BackgroundJobs;

/// <summary>
/// Central scheduler for all background jobs in the system
/// Registers recurring jobs with Hangfire based on business rules and requirements
/// </summary>
public class BackgroundJobScheduler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundJobScheduler> _logger;

    public BackgroundJobScheduler(
        IServiceProvider serviceProvider,
        ILogger<BackgroundJobScheduler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Register all recurring background jobs
    /// This method should be called during application startup
    /// </summary>
    public void RegisterAllRecurringJobs()
    {
        try
        {
            _logger.LogInformation("Starting registration of all recurring background jobs");

            // Membership-related jobs
            RegisterMembershipJobs();

            // Organization-related jobs
            RegisterOrganizationJobs();

            // Payment-related jobs
            RegisterPaymentJobs();

            // Cleanup jobs
            RegisterCleanupJobs();

            // System maintenance jobs
            RegisterSystemJobs();

            _logger.LogInformation("All recurring background jobs registered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while registering recurring background jobs");
            throw;
        }
    }

    private void RegisterMembershipJobs()
    {
        // BR-24, FR-24: Membership expiration notifications (7, 3, 1 days before)
        RecurringJob.AddOrUpdate(
            "membership-expiration-notifications",
            () => _serviceProvider.GetRequiredService<MembershipExpirationNotificationJob>()
                .CheckAndNotifyExpiringMembershipsAsync(),
            "0 9 * * *", // Daily at 9 AM UTC
            TimeZoneInfo.Utc);

        // BR-13, FR-21: Monthly quota reset on anniversary dates
        RecurringJob.AddOrUpdate(
            "membership-quota-reset",
            () => _serviceProvider.GetRequiredService<MembershipQuotaResetJob>()
                .ResetMonthlyQuotasAsync(),
            "0 1 * * *", // Daily at 1 AM UTC
            TimeZoneInfo.Utc);

        // FR-21, BR-21: Usage tracking and quota monitoring
        RecurringJob.AddOrUpdate(
            "membership-usage-tracking",
            () => _serviceProvider.GetRequiredService<MembershipUsageTrackingJob>()
                .UpdateUsageStatisticsAsync(),
            "0 */6 * * *", // Every 6 hours
            TimeZoneInfo.Utc);

        // Auto-downgrade expired memberships to Free Plan
        RecurringJob.AddOrUpdate(
            "membership-auto-downgrade",
            () => _serviceProvider.GetRequiredService<MembershipAutoDowngradeJob>()
                .AutoDowngradeExpiredMembershipsAsync(),
            "0 2 * * *", // Daily at 2 AM UTC
            TimeZoneInfo.Utc);

        _logger.LogInformation("Membership-related jobs registered");
    }

    private void RegisterOrganizationJobs()
    {
        // BR-06, FR-07: Organization invitation cleanup (7 days)
        RecurringJob.AddOrUpdate(
            "organization-invitation-cleanup",
            () => _serviceProvider.GetRequiredService<OrganizationInvitationCleanupJob>()
                .CleanupExpiredInvitationsAsync(),
            "0 2 * * *", // Daily at 2 AM UTC
            TimeZoneInfo.Utc);

        _logger.LogInformation("Organization-related jobs registered");
    }

    private void RegisterPaymentJobs()
    {
        // BR-19, FR-25: Payment failure handling (3 days)
        RecurringJob.AddOrUpdate(
            "payment-failure-handling",
            () => _serviceProvider.GetRequiredService<PaymentFailureHandlingJob>()
                .HandleFailedPaymentsAsync(),
            "0 3 * * *", // Daily at 3 AM UTC
            TimeZoneInfo.Utc);

        // Payment failure warnings (1 day before suspension)
        RecurringJob.AddOrUpdate(
            "payment-failure-warnings",
            () => _serviceProvider.GetRequiredService<PaymentFailureHandlingJob>()
                .SendPaymentFailureWarningsAsync(),
            "0 10 * * *", // Daily at 10 AM UTC
            TimeZoneInfo.Utc);

        // Note: Purchase confirmations are now handled immediately in TransactionService
        // No background job needed for immediate payment confirmations

        _logger.LogInformation("Payment-related jobs registered");
    }

    private void RegisterCleanupJobs()
    {
        // BR-20, FR-31: Export file cleanup (30 days)
        RecurringJob.AddOrUpdate(
            "export-file-cleanup",
            () => _serviceProvider.GetRequiredService<ExportFileCleanupJob>()
                .CleanupExpiredExportFilesAsync(),
            "0 4 * * *", // Daily at 4 AM UTC
            TimeZoneInfo.Utc);

        // BR-11, FR-15: Map history cleanup (1 year)
        RecurringJob.AddOrUpdate(
            "map-history-cleanup",
            () => _serviceProvider.GetRequiredService<MapHistoryCleanupJob>()
                .CleanupOldMapHistoryAsync(),
            "0 5 * * 0", // Weekly on Sunday at 5 AM UTC
            TimeZoneInfo.Utc);

        // BR-28, FR-01: User account deactivation (2 years inactivity)
        RecurringJob.AddOrUpdate(
            "user-account-deactivation",
            () => _serviceProvider.GetRequiredService<UserAccountDeactivationJob>()
                .DeactivateInactiveAccountsAsync(),
            "0 6 * * 0", // Weekly on Sunday at 6 AM UTC
            TimeZoneInfo.Utc);
        
        RecurringJob.AddOrUpdate(

            "map-selection-cleanup",

            () => _serviceProvider.GetRequiredService<MapSelectionCleanupJob>()

                .CleanupAllStaleSelectionsAsync(),

            "*/5 * * * *", // Every 5 minutes

            TimeZoneInfo.Utc);
        
        _logger.LogInformation("Cleanup jobs registered");
    }

    private void RegisterSystemJobs()
    {
        // BR-27, NFR-11: System log cleanup (1 year)
        RecurringJob.AddOrUpdate(
            "system-log-cleanup",
            () => _serviceProvider.GetRequiredService<SystemLogCleanupJob>()
                .CleanupOldSystemLogsAsync(),
            "0 8 * * 0", // Weekly on Sunday at 8 AM UTC
            TimeZoneInfo.Utc);

        _logger.LogInformation("System maintenance jobs registered");
    }

    /// <summary>
    /// Remove all recurring jobs (useful for testing or maintenance)
    /// </summary>
    public void RemoveAllRecurringJobs()
    {
        try
        {
            _logger.LogInformation("Removing all recurring background jobs");

            var jobIds = new[]
            {
                "membership-expiration-notifications",
                "membership-quota-reset",
                "membership-usage-tracking",
                "membership-auto-downgrade",
                "organization-invitation-cleanup",
                "payment-failure-handling",
                "payment-failure-warnings",
                "export-file-cleanup",
                "map-history-cleanup",
                "user-account-deactivation",
                "map-selection-cleanup",
                "collaboration-invitation-cleanup",
                "system-log-cleanup"
            };

            foreach (var jobId in jobIds)
            {
                RecurringJob.RemoveIfExists(jobId);
            }

            _logger.LogInformation("All recurring background jobs removed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing recurring background jobs");
            throw;
        }
    }

    /// <summary>
    /// Get status of all recurring jobs
    /// </summary>
    public Dictionary<string, object> GetJobStatuses()
    {
        try
        {
            var jobIds = new[]
            {
                "membership-expiration-notifications",
                "membership-quota-reset",
                "membership-usage-tracking",
                "membership-auto-downgrade",
                "organization-invitation-cleanup",
                "payment-failure-handling",
                "payment-failure-warnings",
                "export-file-cleanup",
                "map-history-cleanup",
                "user-account-deactivation",
                "collaboration-invitation-cleanup",
                "system-log-cleanup"
            };

            var statuses = new Dictionary<string, object>();

            var recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
            foreach (var jobId in jobIds)
            {
                var job = recurringJobs.FirstOrDefault(j => j.Id == jobId);
                statuses[jobId] = new
                {
                    Exists = job != null,
                    NextExecution = job?.NextExecution,
                    Cron = job?.Cron,
                    LastExecution = job?.LastExecution
                };
            }

            return statuses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting job statuses");
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Trigger a specific job manually (useful for testing or immediate execution)
    /// </summary>
    public void TriggerJob(string jobName)
    {
        try
        {
            _logger.LogInformation("Manually triggering job: {JobName}", jobName);

            switch (jobName.ToLower())
            {
                case "membership-expiration-notifications":
                    BackgroundJob.Enqueue(() => _serviceProvider.GetRequiredService<MembershipExpirationNotificationJob>()
                        .CheckAndNotifyExpiringMembershipsAsync());
                    break;

                case "membership-quota-reset":
                    BackgroundJob.Enqueue(() => _serviceProvider.GetRequiredService<MembershipQuotaResetJob>()
                        .ResetMonthlyQuotasAsync());
                    break;

                case "membership-usage-tracking":
                    BackgroundJob.Enqueue(() => _serviceProvider.GetRequiredService<MembershipUsageTrackingJob>()
                        .UpdateUsageStatisticsAsync());
                    break;

                case "membership-auto-downgrade":
                    BackgroundJob.Enqueue(() => _serviceProvider.GetRequiredService<MembershipAutoDowngradeJob>()
                        .AutoDowngradeExpiredMembershipsAsync());
                    break;

                case "organization-invitation-cleanup":
                    BackgroundJob.Enqueue(() => _serviceProvider.GetRequiredService<OrganizationInvitationCleanupJob>()
                        .CleanupExpiredInvitationsAsync());
                    break;

                case "payment-failure-handling":
                    BackgroundJob.Enqueue(() => _serviceProvider.GetRequiredService<PaymentFailureHandlingJob>()
                        .HandleFailedPaymentsAsync());
                    break;

                case "payment-failure-warnings":
                    BackgroundJob.Enqueue(() => _serviceProvider.GetRequiredService<PaymentFailureHandlingJob>()
                        .SendPaymentFailureWarningsAsync());
                    break;
                case "map-selection-cleanup":
                    BackgroundJob.Enqueue(() => _serviceProvider.GetRequiredService<MapSelectionCleanupJob>()
                        .CleanupAllStaleSelectionsAsync());
                    break;


                default:
                    _logger.LogWarning("Unknown job name: {JobName}", jobName);
                    break;
            }

            _logger.LogInformation("Job {JobName} triggered successfully", jobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while triggering job {JobName}", jobName);
            throw;
        }
    }
}
