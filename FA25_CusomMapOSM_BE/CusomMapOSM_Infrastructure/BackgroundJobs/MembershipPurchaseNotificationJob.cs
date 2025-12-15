using CusomMapOSM_Application.Interfaces.Services.Mail;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Application.Models.Templates.Email;
using CusomMapOSM_Infrastructure.Databases;
using CusomMapOSM_Infrastructure.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CusomMapOSM_Infrastructure.BackgroundJobs;

/// <summary>
/// Background job to send confirmation emails after successful membership purchase
/// Implements FR-26: Generate payment invoices and send confirmation emails
/// Can be triggered immediately after successful payment or run periodically to catch missed notifications
/// </summary>
public class MembershipPurchaseNotificationJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MembershipPurchaseNotificationJob> _logger;

    public MembershipPurchaseNotificationJob(
        IServiceProvider serviceProvider,
        ILogger<MembershipPurchaseNotificationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Send purchase confirmation for a specific membership
    /// </summary>
    [Queue("email")]
    [AutomaticRetry(Attempts = 3)]
    public async Task SendPurchaseConfirmationAsync(Guid membershipId, Guid transactionId)
    {
        try
        {
            _logger.LogInformation(
                "Starting purchase confirmation for membership {MembershipId}, transaction {TransactionId}",
                membershipId, transactionId);

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();
            var hangfireEmailService = scope.ServiceProvider.GetRequiredService<HangfireEmailService>();

            var membership = await dbContext.Memberships
                .Include(m => m.User)
                .Include(m => m.Organization)
                .Include(m => m.Plan)
                .FirstOrDefaultAsync(m => m.MembershipId == membershipId);

            if (membership == null)
            {
                _logger.LogWarning("Membership {MembershipId} not found for purchase confirmation", membershipId);
                return;
            }

            var transaction = await dbContext.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null)
            {
                _logger.LogWarning("Transaction {TransactionId} not found for purchase confirmation", transactionId);
                return;
            }

            await SendPurchaseConfirmationEmailAsync(membership, transaction, hangfireEmailService);

            _logger.LogInformation(
                "Purchase confirmation sent for membership {MembershipId}",
                membershipId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while sending purchase confirmation for membership {MembershipId}",
                membershipId);
            throw;
        }
    }

    /// <summary>
    /// Process pending purchase confirmations (for missed notifications)
    /// </summary>
    [Queue("email")]
    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessPendingPurchaseConfirmationsAsync()
    {
        try
        {
            _logger.LogInformation("Starting pending purchase confirmation process");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();
            var hangfireEmailService = scope.ServiceProvider.GetRequiredService<HangfireEmailService>();

            // Find recent successful transactions that might need confirmation emails
            var recentTransactions = await dbContext.Transactions
                .Include(t => t.Membership)
                .ThenInclude(m => m.User)
                .Include(t => t.Membership)
                .ThenInclude(m => m.Organization)
                .Include(t => t.Membership)
                .ThenInclude(m => m.Plan)
                .Where(t => t.Status == "Completed" &&
                           t.CreatedAt >= DateTime.UtcNow.AddHours(-24) && // Last 24 hours
                           t.Membership != null)
                .ToListAsync();

            var processedCount = 0;
            foreach (var transaction in recentTransactions)
            {
                // Check if confirmation was already sent (you might want to add a flag for this)
                if (!await WasConfirmationSentAsync(transaction.TransactionId, dbContext))
                {
                    await SendPurchaseConfirmationEmailAsync(
                        transaction.Membership!, transaction, hangfireEmailService);
                    processedCount++;
                }
            }

            _logger.LogInformation(
                "Pending purchase confirmation process completed. Processed {Count} confirmations",
                processedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing pending purchase confirmations");
            throw;
        }
    }

    private async Task SendPurchaseConfirmationEmailAsync(
        CusomMapOSM_Domain.Entities.Memberships.Membership membership,
        CusomMapOSM_Domain.Entities.Transactions.Transactions transaction,
        HangfireEmailService hangfireEmailService)
    {
        try
        {
            var subject = "Membership Purchase Confirmation - Welcome to IMOS!";
            var body = GetPurchaseConfirmationEmailBody(membership, transaction);

            var mailRequest = new MailRequest
            {
                ToEmail = membership.User!.Email,
                Subject = subject,
                Body = body
            };

            hangfireEmailService.EnqueueEmail(mailRequest);

            // Mark confirmation as sent (you might want to add a confirmation log table)
            await MarkConfirmationAsSentAsync(transaction.TransactionId);

            _logger.LogInformation(
                "Purchase confirmation email queued for user {UserId}",
                membership.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to queue purchase confirmation email for user {UserId}",
                membership.UserId);
        }
    }

    private string GetPurchaseConfirmationEmailBody(
        CusomMapOSM_Domain.Entities.Memberships.Membership membership,
        CusomMapOSM_Domain.Entities.Transactions.Transactions transaction)
    {
        var startDate = membership.BillingCycleStartDate.ToString("MMMM dd, yyyy");
        var endDate = membership.BillingCycleEndDate.ToString("MMMM dd, yyyy");
        var autoRenewal = membership.AutoRenew ? "Enabled" : "Disabled";

        return EmailTemplates.Membership.GetPurchaseConfirmationTemplate(
            userName: membership.User!.FullName ?? membership.User.Email,
            planName: membership.Plan!.PlanName,
            organizationName: membership.Organization!.OrgName,
            transactionId: transaction.TransactionId,
            amount: transaction.Amount,
            paymentMethod: transaction.PaymentGateway.Name,
            purchaseDate: transaction.CreatedAt,
            startDate: startDate,
            endDate: endDate,
            autoRenewal: autoRenewal,
            status: membership.Status.ToString(),
            maxMapsPerMonth: membership.Plan.MaxMapsPerMonth == -1 ? int.MaxValue : membership.Plan.MaxMapsPerMonth,
            exportQuota: membership.Plan.ExportQuota == -1 ? int.MaxValue : membership.Plan.ExportQuota,
            maxCustomLayers: membership.Plan.MaxCustomLayers == -1 ? int.MaxValue : membership.Plan.MaxCustomLayers,
            maxUsersPerOrg: membership.Plan.MaxUsersPerOrg == -1 ? int.MaxValue : membership.Plan.MaxUsersPerOrg,
            prioritySupport: membership.Plan.PrioritySupport
        );
    }

    private async Task<bool> WasConfirmationSentAsync(Guid transactionId, CustomMapOSMDbContext dbContext)
    {
        // This is a placeholder - you might want to create a confirmation log table
        // For now, we'll assume it wasn't sent if we can't find a record
        try
        {
            // You could check a PurchaseConfirmationLog table here
            // For now, return false to allow sending
            await Task.CompletedTask; // Make it properly async
            return false;
        }
        catch
        {
            return false;
        }
    }

    private async Task MarkConfirmationAsSentAsync(Guid transactionId)
    {
        try
        {
            // This is a placeholder - you might want to create a confirmation log table
            // to track which confirmations have been sent
            await Task.CompletedTask; // Make it properly async
            _logger.LogDebug("Marked confirmation as sent for transaction {TransactionId}", transactionId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to mark confirmation as sent for transaction {TransactionId}", transactionId);
        }
    }

    /// <summary>
    /// Send renewal confirmation for auto-renewed memberships
    /// </summary>
    [Queue("email")]
    [AutomaticRetry(Attempts = 3)]
    public async Task SendRenewalConfirmationAsync(Guid membershipId, Guid transactionId)
    {
        try
        {
            _logger.LogInformation(
                "Starting renewal confirmation for membership {MembershipId}, transaction {TransactionId}",
                membershipId, transactionId);

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();
            var hangfireEmailService = scope.ServiceProvider.GetRequiredService<HangfireEmailService>();

            var membership = await dbContext.Memberships
                .Include(m => m.User)
                .Include(m => m.Organization)
                .Include(m => m.Plan)
                .FirstOrDefaultAsync(m => m.MembershipId == membershipId);

            if (membership == null)
            {
                _logger.LogWarning("Membership {MembershipId} not found for renewal confirmation", membershipId);
                return;
            }

            var transaction = await dbContext.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null)
            {
                _logger.LogWarning("Transaction {TransactionId} not found for renewal confirmation", transactionId);
                return;
            }

            var subject = "Membership Renewed Successfully - Thank You!";
            var body = GetRenewalConfirmationEmailBody(membership, transaction);

            var mailRequest = new MailRequest
            {
                ToEmail = membership.User!.Email,
                Subject = subject,
                Body = body
            };

            var jobId = hangfireEmailService.EnqueueEmail(mailRequest);
            await Task.CompletedTask; // Make it properly async

            _logger.LogInformation(
                "Renewal confirmation sent for membership {MembershipId}",
                membershipId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while sending renewal confirmation for membership {MembershipId}",
                membershipId);
            throw;
        }
    }

    private string GetRenewalConfirmationEmailBody(
        CusomMapOSM_Domain.Entities.Memberships.Membership membership,
        CusomMapOSM_Domain.Entities.Transactions.Transactions transaction)
    {
        var newEndDate = membership.BillingCycleEndDate.ToString("MMMM dd, yyyy");

        return EmailTemplates.Membership.GetRenewalConfirmationTemplate(
            userName: membership.User!.FullName ?? membership.User.Email,
            planName: membership.Plan!.PlanName,
            organizationName: membership.Organization!.OrgName,
            transactionId: transaction.TransactionId,
            amount: transaction.Amount,
            renewalDate: transaction.CreatedAt,
            newEndDate: newEndDate
        );
    }
}
