using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.SystemAdmin;
using UserEntity = CusomMapOSM_Domain.Entities.Users.User;
using OrganizationEntity = CusomMapOSM_Domain.Entities.Organizations.Organization;
using MembershipEntity = CusomMapOSM_Domain.Entities.Memberships.Membership;
using TransactionEntity = CusomMapOSM_Domain.Entities.Transactions.Transactions;
using CusomMapOSM_Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using CusomMapOSM_Domain.Entities.Users.Enums;
using CusomMapOSM_Domain.Entities.Organizations.Enums;
using CusomMapOSM_Domain.Entities.Memberships.Enums;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.SystemAdmin;

public class SystemAdminRepository : ISystemAdminRepository
{
    private readonly CustomMapOSMDbContext _context;

    public SystemAdminRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    // User Management
    public async Task<List<UserEntity>> GetAllUsersAsync(int page = 1, int pageSize = 20, string? search = null, string? status = null, CancellationToken ct = default)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(u => u.AccountStatus == Enum.Parse<AccountStatusEnum>(status));
        }

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> GetTotalUsersCountAsync(string? search = null, string? status = null, CancellationToken ct = default)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(u => u.AccountStatus == Enum.Parse<AccountStatusEnum>(status));
        }

        return await query.CountAsync(ct);
    }

    public async Task<UserEntity?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId, ct);
    }

    public async Task<bool> UpdateUserStatusAsync(Guid userId, string status, CancellationToken ct = default)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId, ct);
            if (user == null) return false;

            user.AccountStatus = Enum.Parse<AccountStatusEnum>(status);

            _context.Users.Update(user);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId, ct);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<UserEntity>> GetUsersByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.Users
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<int> GetActiveUsersCountAsync(CancellationToken ct = default)
    {
        return await _context.Users.CountAsync(u => u.AccountStatus == AccountStatusEnum.Active, ct);
    }

    public async Task<int> GetVerifiedUsersCountAsync(CancellationToken ct = default)
    {
        return await _context.Users.CountAsync(u => u.AccountStatus == AccountStatusEnum.PendingVerification, ct);
    }

    // Organization Management
    public async Task<List<OrganizationEntity>> GetAllOrganizationsAsync(int page = 1, int pageSize = 20, string? search = null, string? status = null, CancellationToken ct = default)
    {
        var query = _context.Organizations
            .Include(o => o.Owner)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(o => o.OrgName.Contains(search) || o.Description.Contains(search));
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status == Enum.Parse<OrganizationStatusEnum>(status));
        }

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> GetTotalOrganizationsCountAsync(string? search = null, string? status = null, CancellationToken ct = default)
    {
        var query = _context.Organizations.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(o => o.OrgName.Contains(search) || o.Description.Contains(search));
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status == Enum.Parse<OrganizationStatusEnum>(status));
        }

        return await query.CountAsync(ct);
    }

    public async Task<OrganizationEntity?> GetOrganizationByIdAsync(Guid orgId, CancellationToken ct = default)
    {
        return await _context.Organizations
            .Include(o => o.Owner)
            .FirstOrDefaultAsync(o => o.OrgId == orgId, ct);
    }

    public async Task<bool> UpdateOrganizationStatusAsync(Guid orgId, string status, CancellationToken ct = default)
    {
        try
        {
            var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.OrgId == orgId, ct);
            if (organization == null) return false;

            organization.Status = Enum.Parse<OrganizationStatusEnum>(status);
            organization.UpdatedAt = DateTime.UtcNow;

            _context.Organizations.Update(organization);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteOrganizationAsync(Guid orgId, CancellationToken ct = default)
    {
        try
        {
            var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.OrgId == orgId, ct);
            if (organization == null) return false;

            _context.Organizations.Remove(organization);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> TransferOrganizationOwnershipAsync(Guid orgId, Guid newOwnerId, CancellationToken ct = default)
    {
        try
        {
            var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.OrgId == orgId, ct);
            if (organization == null) return false;

            organization.OwnerUserId = newOwnerId;
            organization.UpdatedAt = DateTime.UtcNow;

            _context.Organizations.Update(organization);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<OrganizationEntity>> GetOrganizationsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.Organizations
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<int> GetActiveOrganizationsCountAsync(CancellationToken ct = default)
    {
        return await _context.Organizations.CountAsync(o => o.Status == OrganizationStatusEnum.Active, ct);
    }

    public async Task<int> GetOrganizationsWithActiveSubscriptionsCountAsync(CancellationToken ct = default)
    {
        return await _context.Memberships
            .Where(m => m.Status == MembershipStatusEnum.Active)
            .Select(m => m.OrgId)
            .Distinct()
            .CountAsync(ct);
    }

    // Subscription Plan Management
    public async Task<List<CusomMapOSM_Domain.Entities.Memberships.Plan>> GetAllSubscriptionPlansAsync(CancellationToken ct = default)
    {
        return await _context.Plans
            .OrderBy(p => p.PlanName)
            .ToListAsync(ct);
    }

    public async Task<CusomMapOSM_Domain.Entities.Memberships.Plan?> GetSubscriptionPlanByIdAsync(int planId, CancellationToken ct = default)
    {
        return await _context.Plans
            .FirstOrDefaultAsync(p => p.PlanId == planId, ct);
    }

    public async Task<bool> CreateSubscriptionPlanAsync(CusomMapOSM_Domain.Entities.Memberships.Plan plan, CancellationToken ct = default)
    {
        try
        {
            _context.Plans.Add(plan);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateSubscriptionPlanAsync(CusomMapOSM_Domain.Entities.Memberships.Plan plan, CancellationToken ct = default)
    {
        try
        {
            _context.Plans.Update(plan);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteSubscriptionPlanAsync(int planId, CancellationToken ct = default)
    {
        try
        {
            var plan = await _context.Plans.FirstOrDefaultAsync(p => p.PlanId == planId, ct);
            if (plan == null) return false;

            _context.Plans.Remove(plan);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ActivateSubscriptionPlanAsync(int planId, CancellationToken ct = default)
    {
        try
        {
            var plan = await _context.Plans.FirstOrDefaultAsync(p => p.PlanId == planId, ct);
            if (plan == null) return false;

            plan.IsActive = true;
            plan.UpdatedAt = DateTime.UtcNow;

            _context.Plans.Update(plan);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeactivateSubscriptionPlanAsync(int planId, CancellationToken ct = default)
    {
        try
        {
            var plan = await _context.Plans.FirstOrDefaultAsync(p => p.PlanId == planId, ct);
            if (plan == null) return false;

            plan.IsActive = false;
            plan.UpdatedAt = DateTime.UtcNow;

            _context.Plans.Update(plan);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<int> GetSubscribersCountByPlanAsync(int planId, CancellationToken ct = default)
    {
        return await _context.Memberships
            .CountAsync(m => m.PlanId == planId && m.Status == MembershipStatusEnum.Active, ct);
    }

    public async Task<decimal> GetRevenueByPlanAsync(int planId, CancellationToken ct = default)
    {
        return await _context.Transactions
            .Where(t => t.Membership != null && t.Membership.PlanId == planId && t.Status == "completed")
            .SumAsync(t => t.Amount, ct);
    }

    // Membership Management
    public async Task<List<MembershipEntity>> GetAllMembershipsAsync(CancellationToken ct = default)
    {
        return await _context.Memberships
            .Include(m => m.Plan)
            .Include(m => m.User)
            .Include(m => m.Organization)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<MembershipEntity>> GetActiveMembershipsAsync(CancellationToken ct = default)
    {
        return await _context.Memberships
            .Include(m => m.Plan)
            .Include(m => m.User)
            .Include(m => m.Organization)
            .Where(m => m.Status == MembershipStatusEnum.Active)
            .ToListAsync(ct);
    }

    public async Task<List<MembershipEntity>> GetExpiredMembershipsAsync(CancellationToken ct = default)
    {
        return await _context.Memberships
            .Include(m => m.Plan)
            .Include(m => m.User)
            .Include(m => m.Organization)
            .Where(m => m.Status == MembershipStatusEnum.Expired)
            .ToListAsync(ct);
    }

    public async Task<List<MembershipEntity>> GetCancelledMembershipsAsync(CancellationToken ct = default)
    {
        return await _context.Memberships
            .Include(m => m.Plan)
            .Include(m => m.User)
            .Include(m => m.Organization)
            .Where(m => m.Status == MembershipStatusEnum.Cancelled)
            .ToListAsync(ct);
    }

    public async Task<List<MembershipEntity>> GetMembershipsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.Memberships
            .Include(m => m.Plan)
            .Include(m => m.User)
            .Include(m => m.Organization)
            .Where(m => m.CreatedAt >= startDate && m.CreatedAt <= endDate)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<int> GetNewMembershipsCountAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.Memberships
            .CountAsync(m => m.CreatedAt >= startDate && m.CreatedAt <= endDate, ct);
    }

    public async Task<Dictionary<string, int>> GetMembershipsByPlanAsync(CancellationToken ct = default)
    {
        return await _context.Memberships
            .Include(m => m.Plan)
            .Where(m => m.Status == MembershipStatusEnum.Active)
            .GroupBy(m => m.Plan.PlanName)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), ct);
    }

    // Transaction Management
    public async Task<List<Transactions>> GetAllTransactionsAsync(CancellationToken ct = default)
    {
        return await _context.Transactions
            .Include(t => t.PaymentGateway)
            .Include(t => t.Membership)
            .ThenInclude(m => m.Plan)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(ct);
    }

    public async Task<List<Transactions>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.Transactions
            .Include(t => t.PaymentGateway)
            .Include(t => t.Membership)
            .ThenInclude(m => m.Plan)
            .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(ct);
    }

    public async Task<decimal> GetTotalRevenueAsync(CancellationToken ct = default)
    {
        return await _context.Transactions
            .Where(t => t.Status == "completed")
            .SumAsync(t => t.Amount, ct);
    }

    public async Task<decimal> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.Transactions
            .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate && t.Status == "completed")
            .SumAsync(t => t.Amount, ct);
    }

    public async Task<Dictionary<string, decimal>> GetRevenueByPaymentGatewayAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.Transactions
            .Include(t => t.PaymentGateway)
            .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate && t.Status == "completed")
            .GroupBy(t => t.PaymentGateway.Name)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(t => t.Amount), ct);
    }

    public async Task<Dictionary<string, decimal>> GetRevenueByPlanAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.Transactions
            .Include(t => t.Membership)
            .ThenInclude(m => m.Plan)
            .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate && t.Status == "completed")
            .GroupBy(t => t.Membership.Plan.PlanName)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(t => t.Amount), ct);
    }

    // Usage Statistics
    public async Task<Dictionary<string, int>> GetSystemUsageStatsAsync(CancellationToken ct = default)
    {
        var memberships = await _context.Memberships
            .Where(m => m.Status == MembershipStatusEnum.Active)
            .ToListAsync(ct);

        var totalMaps = 0;
        var totalExports = 0;
        var totalCustomLayers = 0;
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
                        totalMaps += usage.GetValueOrDefault("maps", 0);
                        totalExports += usage.GetValueOrDefault("exports", 0);
                        totalCustomLayers += usage.GetValueOrDefault("customLayers", 0);
                        totalTokens += usage.GetValueOrDefault("tokens", 0);
                    }
                }
                catch
                {
                    // Ignore JSON parsing errors
                }
            }
        }

        var stats = new Dictionary<string, int>
        {
            ["total_maps"] = totalMaps,
            ["total_exports"] = totalExports,
            ["total_custom_layers"] = totalCustomLayers,
            ["total_tokens"] = totalTokens
        };

        return stats;
    }

    public async Task<int> GetTotalMapsCreatedAsync(CancellationToken ct = default)
    {
        var memberships = await _context.Memberships
            .Where(m => m.Status == MembershipStatusEnum.Active)
            .ToListAsync(ct);

        var totalMaps = 0;
        foreach (var membership in memberships)
        {
            if (!string.IsNullOrEmpty(membership.CurrentUsage))
            {
                try
                {
                    var usage = JsonSerializer.Deserialize<Dictionary<string, int>>(membership.CurrentUsage);
                    if (usage != null)
                    {
                        totalMaps += usage.GetValueOrDefault("maps", 0);
                    }
                }
                catch
                {
                    // Ignore JSON parsing errors
                }
            }
        }

        return totalMaps;
    }

    public async Task<int> GetTotalExportsAsync(CancellationToken ct = default)
    {
        var memberships = await _context.Memberships
            .Where(m => m.Status == MembershipStatusEnum.Active)
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

    public async Task<int> GetTotalCustomLayersAsync(CancellationToken ct = default)
    {
        var memberships = await _context.Memberships
            .Where(m => m.Status == MembershipStatusEnum.Active)
            .ToListAsync(ct);

        var totalCustomLayers = 0;
        foreach (var membership in memberships)
        {
            if (!string.IsNullOrEmpty(membership.CurrentUsage))
            {
                try
                {
                    var usage = JsonSerializer.Deserialize<Dictionary<string, int>>(membership.CurrentUsage);
                    if (usage != null)
                    {
                        totalCustomLayers += usage.GetValueOrDefault("customLayers", 0);
                    }
                }
                catch
                {
                    // Ignore JSON parsing errors
                }
            }
        }

        return totalCustomLayers;
    }

    public async Task<int> GetTotalTokensUsedAsync(CancellationToken ct = default)
    {
        var memberships = await _context.Memberships
            .Where(m => m.Status == MembershipStatusEnum.Active)
            .ToListAsync(ct);

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

        return totalTokens;
    }

    // Support Ticket Management (placeholder implementations)
    public async Task<List<object>> GetAllSupportTicketsAsync(int page = 1, int pageSize = 20, string? status = null, string? priority = null, string? category = null, CancellationToken ct = default)
    {
        // Would need to implement support ticket entities
        return new List<object>();
    }

    public async Task<int> GetTotalSupportTicketsCountAsync(string? status = null, string? priority = null, string? category = null, CancellationToken ct = default)
    {
        // Would need to implement support ticket entities
        return 0;
    }

    public async Task<object?> GetSupportTicketByIdAsync(Guid ticketId, CancellationToken ct = default)
    {
        // Would need to implement support ticket entities
        return null;
    }

    public async Task<bool> UpdateSupportTicketAsync(Guid ticketId, Dictionary<string, object> updates, CancellationToken ct = default)
    {
        // Would need to implement support ticket entities
        return false;
    }

    public async Task<bool> CloseSupportTicketAsync(Guid ticketId, string resolution, CancellationToken ct = default)
    {
        // Would need to implement support ticket entities
        return false;
    }

    public async Task<bool> AssignSupportTicketAsync(Guid ticketId, Guid assignedToUserId, CancellationToken ct = default)
    {
        // Would need to implement support ticket entities
        return false;
    }

    // System Analytics
    public async Task<Dictionary<string, object>> GetSystemAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var analytics = new Dictionary<string, object>
        {
            ["total_users"] = await GetTotalUsersCountAsync(ct: ct),
            ["active_users"] = await GetActiveUsersCountAsync(ct),
            ["total_organizations"] = await GetTotalOrganizationsCountAsync(ct: ct),
            ["active_organizations"] = await GetActiveOrganizationsCountAsync(ct),
            ["total_revenue"] = await GetRevenueByDateRangeAsync(startDate, endDate, ct),
            ["total_transactions"] = await _context.Transactions.CountAsync(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate, ct)
        };

        return analytics;
    }

    public async Task<List<UserEntity>> GetTopUsersByActivityAsync(int count = 10, CancellationToken ct = default)
    {
        return await _context.Users
            .OrderByDescending(u => u.LastLogin)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<List<OrganizationEntity>> GetTopOrganizationsByActivityAsync(int count = 10, CancellationToken ct = default)
    {
        return await _context.Organizations
            .OrderByDescending(o => o.UpdatedAt)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<List<UserEntity>> GetTopUsersByRevenueAsync(int count = 10, CancellationToken ct = default)
    {
        // Get users with their total revenue from transactions
        var userRevenues = await _context.Transactions
            .Include(t => t.Membership)
            .ThenInclude(m => m.User)
            .Where(t => t.Membership != null && t.Status == "completed")
            .GroupBy(t => t.Membership.UserId)
            .Select(g => new { UserId = g.Key, TotalRevenue = g.Sum(t => t.Amount) })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(count)
            .ToListAsync(ct);

        var userIds = userRevenues.Select(x => x.UserId).ToList();
        return await _context.Users
            .Where(u => userIds.Contains(u.UserId))
            .ToListAsync(ct);
    }

    public async Task<List<OrganizationEntity>> GetTopOrganizationsByRevenueAsync(int count = 10, CancellationToken ct = default)
    {
        // Get organizations with their total revenue from transactions
        var orgRevenues = await _context.Transactions
            .Include(t => t.Membership)
            .ThenInclude(m => m.Organization)
            .Where(t => t.Membership != null && t.Status == "completed")
            .GroupBy(t => t.Membership.OrgId)
            .Select(g => new { OrgId = g.Key, TotalRevenue = g.Sum(t => t.Amount) })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(count)
            .ToListAsync(ct);

        var orgIds = orgRevenues.Select(x => x.OrgId).ToList();
        return await _context.Organizations
            .Where(o => orgIds.Contains(o.OrgId))
            .ToListAsync(ct);
    }

    // System Configuration
    public async Task<Dictionary<string, object>> GetSystemConfigurationAsync(CancellationToken ct = default)
    {
        // Would need to implement system configuration entities
        return new Dictionary<string, object>();
    }

    public async Task<bool> UpdateSystemConfigurationAsync(Dictionary<string, object> configuration, CancellationToken ct = default)
    {
        // Would need to implement system configuration entities
        return false;
    }

    public async Task<bool> ResetSystemConfigurationAsync(CancellationToken ct = default)
    {
        // Would need to implement system configuration entities
        return false;
    }

    // System Maintenance
    public async Task<bool> ClearSystemCacheAsync(CancellationToken ct = default)
    {
        // Would need to implement cache clearing logic
        return true;
    }

    public async Task<bool> BackupSystemDataAsync(CancellationToken ct = default)
    {
        // Would need to implement backup logic
        return true;
    }

    public async Task<bool> RestoreSystemDataAsync(string backupId, CancellationToken ct = default)
    {
        // Would need to implement restore logic
        return true;
    }

    public async Task<bool> PerformSystemMaintenanceAsync(string maintenanceType, CancellationToken ct = default)
    {
        // Would need to implement maintenance logic
        return true;
    }

    // Additional helper methods for statistics without navigation properties
    public async Task<int> GetUserActiveMembershipsCountAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Memberships
            .CountAsync(m => m.UserId == userId && m.Status == MembershipStatusEnum.Active, ct);
    }

    public async Task<int> GetUserTotalOrganizationsCountAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Memberships
            .Where(m => m.UserId == userId && m.Status == MembershipStatusEnum.Active)
            .Select(m => m.OrgId)
            .Distinct()
            .CountAsync(ct);
    }

    public async Task<int> GetOrganizationMembersCountAsync(Guid orgId, CancellationToken ct = default)
    {
        return await _context.OrganizationMembers
            .CountAsync(om => om.OrgId == orgId, ct);
    }

    public async Task<int> GetOrganizationActiveMembershipsCountAsync(Guid orgId, CancellationToken ct = default)
    {
        return await _context.Memberships
            .CountAsync(m => m.OrgId == orgId && m.Status == MembershipStatusEnum.Active, ct);
    }

    public async Task<decimal> GetOrganizationTotalRevenueAsync(Guid orgId, CancellationToken ct = default)
    {
        return await _context.Transactions
            .Include(t => t.Membership)
            .Where(t => t.Membership != null && t.Membership.OrgId == orgId && t.Status == "completed")
            .SumAsync(t => t.Amount, ct);
    }

    public async Task<string> GetOrganizationPrimaryPlanNameAsync(Guid orgId, CancellationToken ct = default)
    {
        var activeMembership = await _context.Memberships
            .Include(m => m.Plan)
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.Status == MembershipStatusEnum.Active, ct);

        return activeMembership?.Plan?.PlanName ?? "No Active Plan";
    }

    public async Task<int> GetUserTotalMapsAsync(Guid userId, CancellationToken ct = default)
    {
        var memberships = await _context.Memberships
            .Where(m => m.UserId == userId && m.Status == MembershipStatusEnum.Active)
            .ToListAsync(ct);

        var totalMaps = 0;
        foreach (var membership in memberships)
        {
            if (!string.IsNullOrEmpty(membership.CurrentUsage))
            {
                try
                {
                    var usage = JsonSerializer.Deserialize<Dictionary<string, int>>(membership.CurrentUsage);
                    if (usage != null)
                    {
                        totalMaps += usage.GetValueOrDefault("maps", 0);
                    }
                }
                catch
                {
                    // Ignore JSON parsing errors
                }
            }
        }
        return totalMaps;
    }

    public async Task<int> GetUserTotalExportsAsync(Guid userId, CancellationToken ct = default)
    {
        var memberships = await _context.Memberships
            .Where(m => m.UserId == userId && m.Status == MembershipStatusEnum.Active)
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

    public async Task<decimal> GetUserTotalSpentAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Transactions
            .Include(t => t.Membership)
            .Where(t => t.Membership != null && t.Membership.UserId == userId && t.Status == "completed")
            .SumAsync(t => t.Amount, ct);
    }

    public async Task<int> GetOrganizationTotalMapsAsync(Guid orgId, CancellationToken ct = default)
    {
        var memberships = await _context.Memberships
            .Where(m => m.OrgId == orgId && m.Status == MembershipStatusEnum.Active)
            .ToListAsync(ct);

        var totalMaps = 0;
        foreach (var membership in memberships)
        {
            if (!string.IsNullOrEmpty(membership.CurrentUsage))
            {
                try
                {
                    var usage = JsonSerializer.Deserialize<Dictionary<string, int>>(membership.CurrentUsage);
                    if (usage != null)
                    {
                        totalMaps += usage.GetValueOrDefault("maps", 0);
                    }
                }
                catch
                {
                    // Ignore JSON parsing errors
                }
            }
        }
        return totalMaps;
    }

    public async Task<int> GetOrganizationTotalExportsAsync(Guid orgId, CancellationToken ct = default)
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

    public async Task<decimal> GetOrganizationTotalSpentAsync(Guid orgId, CancellationToken ct = default)
    {
        return await _context.Transactions
            .Include(t => t.Membership)
            .Where(t => t.Membership != null && t.Membership.OrgId == orgId && t.Status == "completed")
            .SumAsync(t => t.Amount, ct);
    }
}
