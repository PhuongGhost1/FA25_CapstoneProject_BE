using DomainUser = CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Memberships;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.User;
using CusomMapOSM_Infrastructure.Databases;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.User;

public class UserRepository : IUserRepository
{
    private readonly CustomMapOSMDbContext _context;

    public UserRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<DomainUser.User?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId, ct);
    }

    public async Task<DomainUser.User?> GetUserByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<bool> UpdateUserAsync(DomainUser.User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> UpdateUserTokenUsageAsync(Guid userId, int tokenUsage, CancellationToken ct = default)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.MonthlyTokenUsage = tokenUsage;
        user.LastTokenReset = DateTime.UtcNow;

        _context.Users.Update(user);
        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> ResetUserMonthlyTokensAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.MonthlyTokenUsage = 0;
        user.LastTokenReset = DateTime.UtcNow;

        _context.Users.Update(user);
        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<MembershipUsage?> GetUserMembershipUsageAsync(Guid userId, CancellationToken ct = default)
    {
        // Get the user's active membership first
        var membership = await _context.Memberships
            .Include(m => m.User)
            .Include(m => m.Organization)
            .Include(m => m.Plan)
            .FirstOrDefaultAsync(m => m.UserId == userId &&
                                    m.Status == CusomMapOSM_Domain.Entities.Memberships.Enums.MembershipStatusEnum.Active, ct);

        if (membership == null) return null;

        // Get the membership usage
        return await _context.MembershipUsages
            .FirstOrDefaultAsync(mu => mu.MembershipId == membership.MembershipId, ct);
    }

    public async Task<bool> UpdateMembershipUsageAsync(MembershipUsage usage, CancellationToken ct = default)
    {
        _context.MembershipUsages.Update(usage);
        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<int> GetUserTokenUsageAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _context.Users.FindAsync(userId);
        return user?.MonthlyTokenUsage ?? 0;
    }

    public async Task<int> GetUserTotalTokensAsync(Guid userId, CancellationToken ct = default)
    {
        // Get user's active membership and plan
        var membership = await _context.Memberships
            .Include(m => m.Plan)
            .FirstOrDefaultAsync(m => m.UserId == userId &&
                                    m.Status == CusomMapOSM_Domain.Entities.Memberships.Enums.MembershipStatusEnum.Active, ct);

        return membership?.Plan?.MonthlyTokens ?? 0;
    }
}
