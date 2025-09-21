using CusomMapOSM_Infrastructure.Databases;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CusomMapOSM_Infrastructure.BackgroundJobs;

/// <summary>
/// Background job to clean up expired organization invitations
/// Implements BR-06: Organization invitations expire after 7 days if not accepted (FR-07, FR-08)
/// Runs daily to remove expired invitations
/// </summary>
public class OrganizationInvitationCleanupJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrganizationInvitationCleanupJob> _logger;

    public OrganizationInvitationCleanupJob(
        IServiceProvider serviceProvider,
        ILogger<OrganizationInvitationCleanupJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [Queue("default")]
    [AutomaticRetry(Attempts = 3)]
    public async Task CleanupExpiredInvitationsAsync()
    {
        try
        {
            _logger.LogInformation("Starting expired organization invitation cleanup");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();

            var expirationDate = DateTime.UtcNow.AddDays(-7); // 7 days ago

            // Find expired invitations that haven't been accepted
            var expiredInvitations = await dbContext.OrganizationInvitations
                .Include(oi => oi.Organization)
                .Include(oi => oi.Inviter)
                .Where(oi => oi.InvitedAt < expirationDate &&
                           !oi.IsAccepted) // Not accepted yet
                .ToListAsync();

            var cleanedCount = 0;
            foreach (var invitation in expiredInvitations)
            {
                await ProcessExpiredInvitationAsync(invitation, dbContext);
                cleanedCount++;
            }

            await dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Expired organization invitation cleanup completed. Cleaned {Count} invitations",
                cleanedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cleaning up expired invitations");
            throw;
        }
    }

    private async Task ProcessExpiredInvitationAsync(
        CusomMapOSM_Domain.Entities.Organizations.OrganizationInvitation invitation,
        CustomMapOSMDbContext dbContext)
    {
        try
        {
            // Log the cleanup action
            _logger.LogInformation(
                "Removing expired invitation for email {Email} to organization {OrgId}, created on {CreatedAt}",
                invitation.Email, invitation.OrgId, invitation.InvitedAt);

            // Remove the expired invitation
            dbContext.OrganizationInvitations.Remove(invitation);

            // Log the cleanup event for audit purposes
            await LogInvitationCleanupEventAsync(invitation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process expired invitation {InvitationId}",
                invitation.InvitationId);
            throw;
        }
    }

    private async Task LogInvitationCleanupEventAsync(CusomMapOSM_Domain.Entities.Organizations.OrganizationInvitation invitation)
    {
        try
        {
            var logEntry = new
            {
                EventType = "InvitationCleanup",
                InvitationId = invitation.InvitationId,
                OrgId = invitation.OrgId,
                InvitedEmail = invitation.Email,
                OrganizationName = invitation.Organization?.OrgName,
                CreatedAt = invitation.InvitedAt,
                CleanupDate = DateTime.UtcNow,
                Reason = "Expired after 7 days"
            };

            _logger.LogInformation(
                "Invitation cleanup event: {LogEntry}",
                System.Text.Json.JsonSerializer.Serialize(logEntry));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log invitation cleanup event for invitation {InvitationId}", invitation.InvitationId);
        }
    }

    /// <summary>
    /// Manual cleanup of specific expired invitation (can be called from admin interface)
    /// </summary>
    [Queue("default")]
    public async Task CleanupSpecificInvitationAsync(Guid invitationId)
    {
        try
        {
            _logger.LogInformation("Starting manual cleanup for invitation {InvitationId}", invitationId);

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();

            var invitation = await dbContext.OrganizationInvitations
                .Include(oi => oi.Organization)
                .Include(oi => oi.Inviter)
                .FirstOrDefaultAsync(oi => oi.InvitationId == invitationId);

            if (invitation == null)
            {
                _logger.LogWarning("Invitation {InvitationId} not found for cleanup", invitationId);
                return;
            }

            if (invitation.IsAccepted)
            {
                _logger.LogWarning("Cannot cleanup accepted invitation {InvitationId}", invitationId);
                return;
            }

            await ProcessExpiredInvitationAsync(invitation, dbContext);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Manual invitation cleanup completed for {InvitationId}", invitationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during manual invitation cleanup for {InvitationId}", invitationId);
            throw;
        }
    }

    /// <summary>
    /// Get statistics about pending invitations (useful for monitoring)
    /// </summary>
    [Queue("default")]
    public async Task<InvitationStatistics> GetInvitationStatisticsAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();

            var now = DateTime.UtcNow;
            var sevenDaysAgo = now.AddDays(-7);
            var threeDaysAgo = now.AddDays(-3);

            var totalPending = await dbContext.OrganizationInvitations
                .CountAsync(oi => !oi.IsAccepted);

            var expiringSoon = await dbContext.OrganizationInvitations
                .CountAsync(oi => !oi.IsAccepted && oi.InvitedAt < threeDaysAgo);

            var expired = await dbContext.OrganizationInvitations
                .CountAsync(oi => !oi.IsAccepted && oi.InvitedAt < sevenDaysAgo);

            var statistics = new InvitationStatistics
            {
                TotalPending = totalPending,
                ExpiringSoon = expiringSoon,
                Expired = expired,
                GeneratedAt = now
            };

            _logger.LogInformation(
                "Invitation statistics: Total Pending={Total}, Expiring Soon={ExpiringSoon}, Expired={Expired}",
                totalPending, expiringSoon, expired);

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting invitation statistics");
            throw;
        }
    }

    public class InvitationStatistics
    {
        public int TotalPending { get; set; }
        public int ExpiringSoon { get; set; }
        public int Expired { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
