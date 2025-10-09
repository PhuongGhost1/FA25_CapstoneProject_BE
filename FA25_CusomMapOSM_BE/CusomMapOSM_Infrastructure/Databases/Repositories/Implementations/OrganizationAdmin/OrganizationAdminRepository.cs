using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.OrganizationAdmin;
using OrganizationEntity = CusomMapOSM_Domain.Entities.Organizations.Organization;
using UserEntity = CusomMapOSM_Domain.Entities.Users.User;
using MembershipEntity = CusomMapOSM_Domain.Entities.Memberships.Membership;
using TransactionEntity = CusomMapOSM_Domain.Entities.Transactions.Transactions;
using OrganizationMemberEntity = CusomMapOSM_Domain.Entities.Organizations.OrganizationMember;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using CusomMapOSM_Domain.Entities.Memberships.Enums;

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
        var memberships = await _context.Memberships
            .Where(m => m.OrgId == orgId && m.Status == MembershipStatusEnum.Active)
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

        return new Dictionary<string, int>
        {
            { "maps", totalMaps },
            { "exports", totalExports },
            { "customLayers", totalCustomLayers },
            { "tokens", totalTokens }
        };
    }

    public async Task<int> GetTotalActiveUsersAsync(Guid orgId, CancellationToken ct = default)
    {
        return await _context.OrganizationMembers
            .Where(om => om.OrgId == orgId && om.IsActive)
            .CountAsync(ct);
    }

    public async Task<int> GetTotalMapsCreatedAsync(Guid orgId, CancellationToken ct = default)
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
        return await _context.Memberships
            .Include(m => m.Plan)
            .Where(m => m.OrgId == orgId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<MembershipEntity>> GetActiveMembershipsAsync(Guid orgId, CancellationToken ct = default)
    {
        return await _context.Memberships
            .Include(m => m.Plan)
            .Where(m => m.OrgId == orgId && m.Status == MembershipStatusEnum.Active)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<MembershipEntity>> GetExpiredMembershipsAsync(Guid orgId, CancellationToken ct = default)
    {
        return await _context.Memberships
            .Include(m => m.Plan)
            .Where(m => m.OrgId == orgId && m.Status == MembershipStatusEnum.Expired)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<MembershipEntity?> GetPrimaryMembershipAsync(Guid orgId, CancellationToken ct = default)
    {
        return await _context.Memberships
            .Include(m => m.Plan)
            .Where(m => m.OrgId == orgId && m.Status == MembershipStatusEnum.Active)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    // Transaction Data
    public async Task<List<TransactionEntity>> GetOrganizationTransactionsAsync(Guid orgId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        return await _context.Transactions
            .Include(t => t.PaymentGateway)
            .Where(t => t.Membership != null && t.Membership.OrgId == orgId)
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<List<TransactionEntity>> GetTransactionsByDateRangeAsync(Guid orgId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.Transactions
            .Include(t => t.PaymentGateway)
            .Where(t => t.Membership != null &&
                       t.Membership.OrgId == orgId &&
                       t.TransactionDate >= startDate &&
                       t.TransactionDate <= endDate)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(ct);
    }

    public async Task<decimal> GetTotalSpentInPeriodAsync(Guid orgId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.Transactions
            .Where(t => t.Membership != null &&
                       t.Membership.OrgId == orgId &&
                       t.TransactionDate >= startDate &&
                       t.TransactionDate <= endDate &&
                       t.Status == "completed")
            .SumAsync(t => t.Amount, ct);
    }
}