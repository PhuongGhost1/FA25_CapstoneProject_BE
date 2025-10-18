using CusomMapOSM_Infrastructure.Databases;
using CusomMapOSM_Domain.Entities.Organizations.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CusomMapOSM_Domain.Entities.Organizations;

namespace CusomMapOSM_Infrastructure.BackgroundJobs;

/// <summary>
/// Background job to clean up expired collaboration invitations
/// Implements BR-24: Collaboration invitations expire after 14 days (FR-32)
/// Runs daily to remove expired collaboration invitations
/// </summary>
public class CollaborationInvitationCleanupJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CollaborationInvitationCleanupJob> _logger;

    public CollaborationInvitationCleanupJob(
        IServiceProvider serviceProvider,
        ILogger<CollaborationInvitationCleanupJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [Queue("default")]
    [AutomaticRetry(Attempts = 3)]
    public async Task CleanupExpiredCollaborationInvitationsAsync()
    {
        try
        {
            _logger.LogInformation("Starting expired collaboration invitation cleanup");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();

            var expirationDate = DateTime.UtcNow.AddDays(-14); // 14 days ago

            // Assuming there's a CollaborationInvitation table - adjust based on actual schema
            var expiredInvitations = await dbContext.Set<OrganizationInvitation>()
                .Where(ci => ci.InvitedAt < expirationDate && ci.Status == InvitationStatus.Accepted == false)
                .ToListAsync();

            var cleanedCount = expiredInvitations.Count;

            if (cleanedCount > 0)
            {
                dbContext.Set<OrganizationInvitation>().RemoveRange(expiredInvitations);
                await dbContext.SaveChangesAsync();
            }

            _logger.LogInformation(
                "Expired collaboration invitation cleanup completed. Cleaned {Count} invitations",
                cleanedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cleaning up expired collaboration invitations");
            throw;
        }
    }
}
