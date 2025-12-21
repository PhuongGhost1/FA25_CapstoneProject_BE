using CusomMapOSM_Application.Interfaces.Features.OrganizationAdmin;
using CusomMapOSM_Application.Models.DTOs.Features.OrganizationAdmin;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.OrganizationAdmin;
using CusomMapOSM_Domain.Entities.Memberships.Enums;
using Optional;
using System.Text.Json;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using Optional.Unsafe;

namespace CusomMapOSM_Infrastructure.Features.OrganizationAdmin;

public class OrganizationAdminService : IOrganizationAdminService
{
    private readonly IOrganizationAdminRepository _organizationAdminRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipService _membershipService;
    
    public OrganizationAdminService(
        IOrganizationAdminRepository organizationAdminRepository, 
        IOrganizationRepository organizationRepository,
        IMembershipService membershipService)
    {
        _organizationAdminRepository = organizationAdminRepository;
        _organizationRepository = organizationRepository;
        _membershipService = membershipService;
    }

    public async Task<Option<OrganizationUsageResponse, Error>> GetOrganizationUsageAsync(Guid orgId, CancellationToken ct = default)
    {
        try
        {
            var organization = await _organizationAdminRepository.GetOrganizationByIdAsync(orgId, ct);
            if (organization == null)
            {
                return Option.None<OrganizationUsageResponse, Error>(Error.NotFound("Organization.NotFound", "Organization not found"));
            }

            // Get primary membership for the organization
            var primaryMembership = await _organizationAdminRepository.GetPrimaryMembershipAsync(orgId, ct);
            if (primaryMembership == null)
            {
                return Option.None<OrganizationUsageResponse, Error>(Error.NotFound("Membership.NotFound", "No active membership found for organization"));
            }

            // Get usage for the primary membership
            var usageResult = await _membershipService.GetOrCreateUsageAsync(primaryMembership.MembershipId, orgId, ct);
            if (!usageResult.HasValue)
            {
                return Option.None<OrganizationUsageResponse, Error>(Error.Failure("Usage.GetFailed", "Failed to get usage information"));
            }

            var usage = usageResult.ValueOrDefault();
            var plan = primaryMembership.Plan;

            // Get aggregated usage stats for active users count
            var totalActiveUsers = await _organizationAdminRepository.GetTotalActiveUsersAsync(orgId, ct);

            // Convert usage stats to quota DTOs with actual plan limits
            // IMPORTANT: Use usage.MapsCreatedThisCycle and usage.ExportsThisCycle from MembershipUsage table
            // NOT from Memberships.CurrentUsage JSON field which may be stale after upgrades
            var aggregatedQuotas = new List<UsageQuotaDto>();

            if (plan != null)
            {
                // Maps quota - use MembershipUsage.MapsCreatedThisCycle
                aggregatedQuotas.Add(new UsageQuotaDto
                {
                    ResourceType = "maps",
                    CurrentUsage = usage.MapsCreatedThisCycle,
                    Limit = plan.MapQuota == -1 ? int.MaxValue : plan.MapQuota
                });

                // Exports quota - use MembershipUsage.ExportsThisCycle
                aggregatedQuotas.Add(new UsageQuotaDto
                {
                    ResourceType = "exports",
                    CurrentUsage = usage.ExportsThisCycle,
                    Limit = plan.ExportQuota == -1 ? int.MaxValue : plan.ExportQuota
                });

                // Users quota
                aggregatedQuotas.Add(new UsageQuotaDto
                {
                    ResourceType = "users",
                    CurrentUsage = totalActiveUsers,
                    Limit = plan.MaxUsersPerOrg == -1 ? int.MaxValue : plan.MaxUsersPerOrg
                });

                // Custom Layers quota - currently not tracked in MembershipUsage
                aggregatedQuotas.Add(new UsageQuotaDto
                {
                    ResourceType = "customLayers",
                    CurrentUsage = 0, // TODO: Track custom layers in MembershipUsage
                    Limit = plan.MaxCustomLayers == -1 ? int.MaxValue : plan.MaxCustomLayers
                });

                // Tokens quota - currently not tracked in MembershipUsage
                aggregatedQuotas.Add(new UsageQuotaDto
                {
                    ResourceType = "tokens",
                    CurrentUsage = 0, // TODO: Track tokens in MembershipUsage
                    Limit = plan.MonthlyTokens == -1 ? int.MaxValue : plan.MonthlyTokens
                });
            }

            // Create empty user summaries for now
            var userUsageSummaries = new List<UserUsageSummaryDto>();

            var response = new OrganizationUsageResponse
            {
                OrgId = orgId,
                OrganizationName = organization.OrgName,
                AggregatedQuotas = aggregatedQuotas,
                UserUsageSummaries = userUsageSummaries,
                TotalActiveUsers = totalActiveUsers,
                TotalMapsCreated = usage.MapsCreatedThisCycle,
                TotalExportsThisMonth = usage.ExportsThisCycle,
                LastResetDate = usage.CycleStartDate,
                NextResetDate = usage.CycleEndDate
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
                BillingCycleStartDate = m.BillingCycleStartDate,
                BillingCycleEndDate = m.BillingCycleEndDate,
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
                BillingCycleStartDate = m.BillingCycleStartDate,
                BillingCycleEndDate = m.BillingCycleEndDate,
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
                NextBillingDate = primaryMembership?.BillingCycleEndDate ?? DateTime.UtcNow.AddDays(30),
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