using CusomMapOSM_Application.Interfaces.Features.OrganizationAdmin;
using CusomMapOSM_Application.Models.DTOs.Features.OrganizationAdmin;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.OrganizationAdmin;
using CusomMapOSM_Domain.Entities.Memberships.Enums;
using Optional;
using System.Text.Json;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;

namespace CusomMapOSM_Infrastructure.Features.OrganizationAdmin;

public class OrganizationAdminService : IOrganizationAdminService
{
    private readonly IOrganizationAdminRepository _organizationAdminRepository;
    private readonly IOrganizationRepository _organizationRepository;
    public OrganizationAdminService(IOrganizationAdminRepository organizationAdminRepository, IOrganizationRepository organizationRepository)
    {
        _organizationAdminRepository = organizationAdminRepository;
        _organizationRepository = organizationRepository;
    }

    public async Task<Option<OrganizationUsageResponse, Error>> GetOrganizationUsageAsync(Guid orgId, CancellationToken ct = default)
    {
        try
        {
            var organization = await _organizationAdminRepository.GetOrganizationByIdAsync(orgId, ct);
            var usageStats = await _organizationAdminRepository.GetOrganizationUsageStatsAsync(orgId, ct);
            var totalActiveUsers = await _organizationAdminRepository.GetTotalActiveUsersAsync(orgId, ct);
            var totalMapsCreated = await _organizationAdminRepository.GetTotalMapsCreatedAsync(orgId, ct);
            var totalExportsThisMonth = await _organizationAdminRepository.GetTotalExportsThisMonthAsync(orgId, ct);

            // Convert usage stats to quota DTOs
            var aggregatedQuotas = usageStats.Select(kvp => new UsageQuotaDto
            {
                ResourceType = kvp.Key,
                CurrentUsage = kvp.Value,
                Limit = -1 // Unlimited for now
            }).ToList();

            // Create empty user summaries for now
            var userUsageSummaries = new List<UserUsageSummaryDto>();

            var response = new OrganizationUsageResponse
            {
                OrgId = orgId,
                OrganizationName = organization?.OrgName ?? "Unknown Organization",
                AggregatedQuotas = aggregatedQuotas,
                UserUsageSummaries = userUsageSummaries,
                TotalActiveUsers = totalActiveUsers,
                TotalMapsCreated = totalMapsCreated,
                TotalExportsThisMonth = totalExportsThisMonth,
                LastResetDate = DateTime.UtcNow.AddDays(-30), // Placeholder
                NextResetDate = DateTime.UtcNow.AddDays(1) // Placeholder
            };

            return Option.Some<OrganizationUsageResponse, Error>(response);
        }
        catch (Exception ex)
        {
            return Option.None<OrganizationUsageResponse, Error>(Error.Failure("Failed to get organization usage", ex.Message));
        }
    }

    public async Task<Option<OrganizationSubscriptionResponse, Error>> GetOrganizationSubscriptionAsync(Guid orgId, CancellationToken ct = default)
    {
        try
        {
            var organization = await _organizationAdminRepository.GetOrganizationByIdAsync(orgId, ct);
            var allMemberships = await _organizationAdminRepository.GetOrganizationMembershipsAsync(orgId, ct);
            var activeMemberships = await _organizationAdminRepository.GetActiveMembershipsAsync(orgId, ct);
            var expiredMemberships = await _organizationAdminRepository.GetExpiredMembershipsAsync(orgId, ct);
            var primaryMembership = await _organizationAdminRepository.GetPrimaryMembershipAsync(orgId, ct);

            // Convert memberships to summary DTOs
            var activeMembershipSummaries = activeMemberships.Select(m => new MembershipSummaryDto
            {
                MembershipId = m.MembershipId,
                UserId = m.UserId,
                UserName = m.User?.FullName ?? "Unknown User",
                UserEmail = m.User?.Email ?? "unknown@example.com",
                PlanId = m.PlanId,
                PlanName = m.Plan?.PlanName ?? "Unknown",
                Status = m.Status.ToString(),
                StartDate = m.StartDate,
                EndDate = m.EndDate ?? DateTime.UtcNow.AddDays(30),
                AutoRenew = m.AutoRenew,
                MonthlyCost = m.Plan?.PriceMonthly ?? 0,
                CreatedAt = m.CreatedAt
            }).ToList();

            var expiredMembershipSummaries = expiredMemberships.Select(m => new MembershipSummaryDto
            {
                MembershipId = m.MembershipId,
                UserId = m.UserId,
                UserName = m.User?.FullName ?? "Unknown User",
                UserEmail = m.User?.Email ?? "unknown@example.com",
                PlanId = m.PlanId,
                PlanName = m.Plan?.PlanName ?? "Unknown",
                Status = m.Status.ToString(),
                StartDate = m.StartDate,
                EndDate = m.EndDate ?? DateTime.UtcNow.AddDays(30),
                AutoRenew = m.AutoRenew,
                MonthlyCost = m.Plan?.PriceMonthly ?? 0,
                CreatedAt = m.CreatedAt
            }).ToList();

            var response = new OrganizationSubscriptionResponse
            {
                OrgId = orgId,
                OrganizationName = organization?.OrgName ?? "Unknown Organization",
                ActiveMemberships = activeMembershipSummaries,
                PendingMemberships = new List<MembershipSummaryDto>(), // No pending memberships for now
                ExpiredMemberships = expiredMembershipSummaries,
                NextBillingDate = primaryMembership?.EndDate ?? DateTime.UtcNow.AddDays(30),
                TotalMonthlyCost = activeMemberships.Sum(m => m.Plan?.PriceMonthly ?? 0),
                PrimaryPlanName = primaryMembership?.Plan?.PlanName ?? "No Plan",
                HasActiveSubscription = activeMemberships.Any()
            };

            return Option.Some<OrganizationSubscriptionResponse, Error>(response);
        }
        catch (Exception ex)
        {
            return Option.None<OrganizationSubscriptionResponse, Error>(Error.Failure("Failed to get organization subscription", ex.Message));
        }
    }

    public async Task<Option<OrganizationBillingResponse, Error>> GetOrganizationBillingAsync(Guid orgId, CancellationToken ct = default)
    {
        try
        {
            var organization = await _organizationAdminRepository.GetOrganizationByIdAsync(orgId, ct);
            var recentTransactions = await _organizationAdminRepository.GetOrganizationTransactionsAsync(orgId, 1, 10, ct);
            var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);
            var currentMonthSpending = await _organizationAdminRepository.GetTotalSpentInPeriodAsync(orgId, currentMonthStart, currentMonthEnd, ct);

            var billingTransactions = recentTransactions.Select(t => new BillingTransactionDto
            {
                TransactionId = t.TransactionId,
                Amount = t.Amount,
                Status = t.Status,
                TransactionDate = t.TransactionDate,
                PaymentGateway = t.PaymentGateway?.Name ?? "Unknown",
                Description = "Transaction", // Placeholder since Transactions entity doesn't have Description
                Currency = "USD", // Placeholder
                GatewayTransactionId = t.TransactionReference
            }).ToList();

            var response = new OrganizationBillingResponse
            {
                OrgId = orgId,
                OrganizationName = organization?.OrgName ?? "Unknown Organization",
                RecentTransactions = billingTransactions,
                RecentInvoices = new List<BillingInvoiceDto>(), // No invoices for now
                TotalSpentThisMonth = currentMonthSpending,
                TotalSpentLastMonth = 0, // Placeholder
                OutstandingBalance = 0, // Placeholder
                PaymentMethod = "Credit Card", // Placeholder
                NextBillingDate = DateTime.UtcNow.AddDays(30), // Placeholder
                HasPaymentMethodOnFile = true // Placeholder
            };

            return Option.Some<OrganizationBillingResponse, Error>(response);
        }
        catch (Exception ex)
        {
            return Option.None<OrganizationBillingResponse, Error>(Error.Failure("Failed to get organization billing", ex.Message));
        }
    }

    public async Task<Option<bool, Error>> IsUserOrganizationAdminAsync(Guid userId, Guid orgId, CancellationToken ct = default)
    {
        try
        {
            var isAdmin = await _organizationAdminRepository.IsUserOrganizationAdminAsync(userId, orgId, ct);
            return Option.Some<bool, Error>(isAdmin);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("Failed to check organization admin status", ex.Message));
        }
    }

    public async Task<Option<bool, Error>> IsUserOrganizationOwnerAsync(Guid userId, Guid orgId, CancellationToken ct = default)
    {
        try
        {
            var isOwner = await _organizationAdminRepository.IsUserOrganizationOwnerAsync(userId, orgId, ct);
            return Option.Some<bool, Error>(isOwner);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("Failed to check organization ownership", ex.Message));
        }
    }
}