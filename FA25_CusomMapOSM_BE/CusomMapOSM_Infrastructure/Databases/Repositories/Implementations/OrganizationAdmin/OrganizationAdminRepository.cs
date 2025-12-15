using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.OrganizationAdmin;
using OrganizationEntity = CusomMapOSM_Domain.Entities.Organizations.Organization;
using UserEntity = CusomMapOSM_Domain.Entities.Users.User;
using MembershipEntity = CusomMapOSM_Domain.Entities.Memberships.Membership;
using TransactionEntity = CusomMapOSM_Domain.Entities.Transactions.Transactions;
using OrganizationMemberEntity = CusomMapOSM_Domain.Entities.Organizations.OrganizationMember;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using CusomMapOSM_Domain.Entities.Memberships.Enums;
using CusomMapOSM_Domain.Entities.Organizations.Enums;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.OrganizationAdmin;

public class OrganizationAdminRepository : IOrganizationAdminRepository
{
    private readonly CustomMapOSMDbContext _context;

    public OrganizationAdminRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    // Usage Data
    public async Task<Dictionary<string, int>> GetOrganizationUsageStatsAsync(Guid orgId, CancellationToken ct = default)
    {
        // Count actual maps created in all workspaces belonging to this organization
        var totalMaps = await _context.Maps
            .Where(m => m.Workspace != null && m.Workspace.OrgId == orgId && m.IsActive)
            .CountAsync(ct);

        // Count exports this month (if there's an export tracking table, use it)
        // For now, we'll use the membership usage data as fallback
        var totalExports = 0;
        var memberships = await _context.Memberships
            .Where(m => m.OrgId == orgId && m.Status == MembershipStatusEnum.Active)
            .ToListAsync(ct);

        foreach (var membership in memberships)
        {
            if (!string.IsNullOrEmpty(membership.CurrentUsage))
            {
                try
                {
                    var usage = JsonSerializer.Deserialize<Dictionary<string, int>>(membership.CurrentUsage);
                    if (usage != null)
                    {
                        totalExports += usage.GetValueOrDefault("exports", 0);
                    }
                }
                catch
                {
                    // Ignore JSON parsing errors
                }
            }
        }

        // Count custom layers (map layers/features belonging to organization's maps)
        var totalCustomLayers = await _context.MapFeatures
            .Where(mf => mf.Map != null && mf.Map.Workspace != null && mf.Map.Workspace.OrgId == orgId && mf.IsVisible)
            .CountAsync(ct);

        // Count question banks as well (useful for this platform)
        var totalQuestionBanks = await _context.QuestionBanks
            .Where(qb => qb.Workspace != null && qb.Workspace.OrgId == orgId && qb.IsActive)
            .CountAsync(ct);

        // Tokens usage from membership data
        var totalTokens = 0;
        foreach (var membership in memberships)
        {
            if (!string.IsNullOrEmpty(membership.CurrentUsage))
            {
                try
                {
                    var usage = JsonSerializer.Deserialize<Dictionary<string, int>>(membership.CurrentUsage);
                    if (usage != null)
                    {
                        totalTokens += usage.GetValueOrDefault("tokens", 0);
                    }
                }
                catch
                {
                    // Ignore JSON parsing errors
                }
            }
        }

        return new Dictionary<string, int>
        {
            { "maps", totalMaps },
            { "exports", totalExports },
            { "customLayers", totalCustomLayers },
            { "questionBanks", totalQuestionBanks },
            { "tokens", totalTokens }
        };
    }

    public async Task<int> GetTotalActiveUsersAsync(Guid orgId, CancellationToken ct = default)
    {
        return await _context.OrganizationMembers
            .Where(om => om.OrgId == orgId && om.Status == MemberStatus.Active)
            .CountAsync(ct);
    }

    public async Task<int> GetTotalMapsCreatedAsync(Guid orgId, CancellationToken ct = default)
    {
        // Count actual maps created in all workspaces belonging to this organization
        return await _context.Maps
            .Where(m => m.Workspace != null && m.Workspace.OrgId == orgId && m.IsActive)
            .CountAsync(ct);
    }

    public async Task<int> GetTotalExportsThisMonthAsync(Guid orgId, CancellationToken ct = default)
    {
        var memberships = await _context.Memberships
            .Where(m => m.OrgId == orgId && m.Status == MembershipStatusEnum.Active)
            .ToListAsync(ct);

        var totalExports = 0;
        foreach (var membership in memberships)
        {
            if (!string.IsNullOrEmpty(membership.CurrentUsage))
            {
                try
                {
                    var usage = JsonSerializer.Deserialize<Dictionary<string, int>>(membership.CurrentUsage);
                    if (usage != null)
                    {
                        totalExports += usage.GetValueOrDefault("exports", 0);
                    }
                }
                catch
                {
                    // Ignore JSON parsing errors
                }
            }
        }

        return totalExports;
    }

    // Membership Data
    public async Task<List<MembershipEntity>> GetOrganizationMembershipsAsync(Guid orgId, CancellationToken ct = default)
    {
        var memberships = await _context.Memberships
            .Where(m => m.OrgId == orgId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

        // Manually load related entities
        foreach (var membership in memberships)
        {
            if (membership.PlanId > 0)
            {
                membership.Plan = await _context.Plans
                    .FirstOrDefaultAsync(p => p.PlanId == membership.PlanId, ct);
            }

            if (membership.UserId != Guid.Empty)
            {
                membership.User = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == membership.UserId, ct);
            }

            if (membership.OrgId != Guid.Empty)
            {
                membership.Organization = await _context.Organizations
                    .FirstOrDefaultAsync(o => o.OrgId == membership.OrgId, ct);
            }
        }

        return memberships;
    }

    public async Task<List<MembershipEntity>> GetActiveMembershipsAsync(Guid orgId, CancellationToken ct = default)
    {
        var memberships = await _context.Memberships
            .Where(m => m.OrgId == orgId && m.Status == MembershipStatusEnum.Active)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

        // Manually load related entities
        foreach (var membership in memberships)
        {
            if (membership.PlanId > 0)
            {
                membership.Plan = await _context.Plans
                    .FirstOrDefaultAsync(p => p.PlanId == membership.PlanId, ct);
            }

            if (membership.UserId != Guid.Empty)
            {
                membership.User = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == membership.UserId, ct);
            }

            if (membership.OrgId != Guid.Empty)
            {
                membership.Organization = await _context.Organizations
                    .FirstOrDefaultAsync(o => o.OrgId == membership.OrgId, ct);
            }
        }

        return memberships;
    }

    public async Task<List<MembershipEntity>> GetExpiredMembershipsAsync(Guid orgId, CancellationToken ct = default)
    {
        var memberships = await _context.Memberships
            .Where(m => m.OrgId == orgId && m.Status == MembershipStatusEnum.Expired)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

        // Manually load related entities
        foreach (var membership in memberships)
        {
            if (membership.PlanId > 0)
            {
                membership.Plan = await _context.Plans
                    .FirstOrDefaultAsync(p => p.PlanId == membership.PlanId, ct);
            }

            if (membership.UserId != Guid.Empty)
            {
                membership.User = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == membership.UserId, ct);
            }

            if (membership.OrgId != Guid.Empty)
            {
                membership.Organization = await _context.Organizations
                    .FirstOrDefaultAsync(o => o.OrgId == membership.OrgId, ct);
            }
        }

        return memberships;
    }

    public async Task<MembershipEntity?> GetPrimaryMembershipAsync(Guid orgId, CancellationToken ct = default)
    {
        var membership = await _context.Memberships
            .Where(m => m.OrgId == orgId && m.Status == MembershipStatusEnum.Active)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (membership != null)
        {
            // Manually load related entities
            if (membership.PlanId > 0)
            {
                membership.Plan = await _context.Plans
                    .FirstOrDefaultAsync(p => p.PlanId == membership.PlanId, ct);
            }

            if (membership.UserId != Guid.Empty)
            {
                membership.User = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == membership.UserId, ct);
            }

            if (membership.OrgId != Guid.Empty)
            {
                membership.Organization = await _context.Organizations
                    .FirstOrDefaultAsync(o => o.OrgId == membership.OrgId, ct);
            }
        }

        return membership;
    }

    // Transaction Data
    public async Task<List<TransactionEntity>> GetOrganizationTransactionsAsync(Guid orgId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        // Get transactions that have MembershipId and are linked to the organization
        var membershipTransactions = await _context.Transactions
            .Where(t => t.MembershipId != null)
            .ToListAsync(ct);

        // Filter by organization through membership
        var filteredMembershipTransactions = new List<TransactionEntity>();
        foreach (var transaction in membershipTransactions)
        {
            if (transaction.MembershipId.HasValue)
            {
                var membership = await _context.Memberships
                    .FirstOrDefaultAsync(m => m.MembershipId == transaction.MembershipId.Value && m.OrgId == orgId, ct);
                if (membership != null)
                {
                    // Manually load related entities
                    transaction.Membership = membership;
                    transaction.PaymentGateway = await _context.PaymentGateways
                        .FirstOrDefaultAsync(pg => pg.GatewayId == transaction.PaymentGatewayId, ct);
                    filteredMembershipTransactions.Add(transaction);
                }
            }
        }

        // Get transactions that don't have MembershipId but have Purpose field with organization context
        var purposeTransactions = await _context.Transactions
            .Where(t => t.MembershipId == null && !string.IsNullOrEmpty(t.Purpose) && t.Purpose.Contains("|"))
            .ToListAsync(ct);

        // Filter purpose transactions by organization ID and load related entities
        var filteredPurposeTransactions = new List<TransactionEntity>();
        foreach (var transaction in purposeTransactions)
        {
            if (IsTransactionForOrganization(transaction, orgId))
            {
                // Manually load related entities
                transaction.PaymentGateway = await _context.PaymentGateways
                    .FirstOrDefaultAsync(pg => pg.GatewayId == transaction.PaymentGatewayId, ct);
                filteredPurposeTransactions.Add(transaction);
            }
        }

        // Combine and sort all transactions
        var allTransactions = filteredMembershipTransactions.Concat(filteredPurposeTransactions)
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return allTransactions;
    }

    public async Task<List<TransactionEntity>> GetTransactionsByDateRangeAsync(Guid orgId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        // Get transactions that have MembershipId and are linked to the organization
        var membershipTransactions = await _context.Transactions
            .Where(t => t.MembershipId != null &&
                       t.TransactionDate >= startDate &&
                       t.TransactionDate <= endDate)
            .ToListAsync(ct);

        // Filter by organization through membership and load related entities
        var filteredMembershipTransactions = new List<TransactionEntity>();
        foreach (var transaction in membershipTransactions)
        {
            if (transaction.MembershipId.HasValue)
            {
                var membership = await _context.Memberships
                    .FirstOrDefaultAsync(m => m.MembershipId == transaction.MembershipId.Value && m.OrgId == orgId, ct);
                if (membership != null)
                {
                    // Manually load related entities
                    transaction.Membership = membership;
                    transaction.PaymentGateway = await _context.PaymentGateways
                        .FirstOrDefaultAsync(pg => pg.GatewayId == transaction.PaymentGatewayId, ct);
                    filteredMembershipTransactions.Add(transaction);
                }
            }
        }

        // Get transactions that don't have MembershipId but have Purpose field with organization context
        var purposeTransactions = await _context.Transactions
            .Where(t => t.MembershipId == null &&
                       !string.IsNullOrEmpty(t.Purpose) &&
                       t.Purpose.Contains("|") &&
                       t.TransactionDate >= startDate &&
                       t.TransactionDate <= endDate)
            .ToListAsync(ct);

        // Filter purpose transactions by organization ID and load related entities
        var filteredPurposeTransactions = new List<TransactionEntity>();
        foreach (var transaction in purposeTransactions)
        {
            if (IsTransactionForOrganization(transaction, orgId))
            {
                // Manually load related entities
                transaction.PaymentGateway = await _context.PaymentGateways
                    .FirstOrDefaultAsync(pg => pg.GatewayId == transaction.PaymentGatewayId, ct);
                filteredPurposeTransactions.Add(transaction);
            }
        }

        // Combine and sort all transactions
        var allTransactions = filteredMembershipTransactions.Concat(filteredPurposeTransactions)
            .OrderByDescending(t => t.TransactionDate)
            .ToList();

        return allTransactions;
    }

    public async Task<decimal> GetTotalSpentInPeriodAsync(Guid orgId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        // Get transactions that have MembershipId and are linked to the organization
        var membershipTransactions = await _context.Transactions
            .Where(t => t.MembershipId != null &&
                       t.TransactionDate >= startDate &&
                       t.TransactionDate <= endDate &&
                       t.Status == "completed")
            .ToListAsync(ct);

        // Filter by organization through membership
        var filteredMembershipTransactions = new List<TransactionEntity>();
        foreach (var transaction in membershipTransactions)
        {
            if (transaction.MembershipId.HasValue)
            {
                var membership = await _context.Memberships
                    .FirstOrDefaultAsync(m => m.MembershipId == transaction.MembershipId.Value && m.OrgId == orgId, ct);
                if (membership != null)
                {
                    filteredMembershipTransactions.Add(transaction);
                }
            }
        }

        // Get transactions that don't have MembershipId but have Purpose field with organization context
        var purposeTransactions = await _context.Transactions
            .Where(t => t.MembershipId == null &&
                       !string.IsNullOrEmpty(t.Purpose) &&
                       t.Purpose.Contains("|") &&
                       t.TransactionDate >= startDate &&
                       t.TransactionDate <= endDate &&
                       t.Status == "completed")
            .ToListAsync(ct);

        // Filter purpose transactions by organization ID
        var filteredPurposeTransactions = purposeTransactions
            .Where(t => IsTransactionForOrganization(t, orgId))
            .ToList();

        // Calculate total from both sources
        var membershipTotal = filteredMembershipTransactions.Sum(t => t.Amount);
        var purposeTotal = filteredPurposeTransactions.Sum(t => t.Amount);

        return membershipTotal + purposeTotal;
    }

    // Organization Data
    public async Task<OrganizationEntity?> GetOrganizationByIdAsync(Guid orgId, CancellationToken ct = default)
    {
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.OrgId == orgId, ct);

        if (organization != null && organization.OwnerUserId != Guid.Empty)
        {
            // Manually load related entities
            organization.Owner = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == organization.OwnerUserId, ct);
        }

        return organization;
    }

    public async Task<bool> IsUserOrganizationAdminAsync(Guid userId, Guid orgId, CancellationToken ct = default)
    {
        // Check if user is an admin or owner of the organization
        var isOwner = await _context.Organizations
            .AnyAsync(o => o.OrgId == orgId && o.OwnerUserId == userId, ct);

        if (isOwner) return true;

        // Check if user is an admin member of the organization
        var organizationMembers = await _context.OrganizationMembers
            .Where(om => om.OrgId == orgId &&
                        om.UserId == userId &&
                        om.Status == MemberStatus.Active)
            .ToListAsync(ct);

        foreach (var member in organizationMembers)
        {
            if (member.Role == OrganizationMemberTypeEnum.Admin || 
                member.Role == OrganizationMemberTypeEnum.Owner)
            {
                return true;
            }
        }

        return false;
    }

    public async Task<bool> IsUserOrganizationOwnerAsync(Guid userId, Guid orgId, CancellationToken ct = default)
    {
        return await _context.Organizations
            .AnyAsync(o => o.OrgId == orgId && o.OwnerUserId == userId, ct);
    }

    private bool IsTransactionForOrganization(TransactionEntity transaction, Guid orgId)
    {
        try
        {
            if (string.IsNullOrEmpty(transaction.Purpose) || !transaction.Purpose.Contains("|"))
                return false;

            var parts = transaction.Purpose.Split('|', 2);
            if (parts.Length != 2)
                return false;

            var contextJson = parts[1];
            var context = JsonSerializer.Deserialize<Dictionary<string, object>>(contextJson);

            if (context != null && context.ContainsKey("OrgId"))
            {
                var contextOrgId = context["OrgId"].ToString();
                return Guid.TryParse(contextOrgId, out var parsedOrgId) && parsedOrgId == orgId;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}