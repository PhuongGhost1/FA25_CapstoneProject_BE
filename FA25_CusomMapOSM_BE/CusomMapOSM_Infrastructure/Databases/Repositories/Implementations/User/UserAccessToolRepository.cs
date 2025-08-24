using CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.User;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.User;

public class UserAccessToolRepository : IUserAccessToolRepository
{
    private readonly CustomMapOSMDbContext _context;

    public UserAccessToolRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<UserAccessTool?> GetByIdAsync(int userAccessToolId, CancellationToken ct)
    {
        return await _context.UserAccessTools
            .Include(uat => uat.AccessTool)
            .FirstOrDefaultAsync(uat => uat.UserAccessToolId == userAccessToolId, ct);
    }

    public async Task<IReadOnlyList<UserAccessTool>> GetByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return await _context.UserAccessTools
            .Include(uat => uat.AccessTool)
            .Where(uat => uat.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<UserAccessTool>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        return await _context.UserAccessTools
            .Include(uat => uat.AccessTool)
            .Where(uat => uat.UserId == userId && uat.ExpiredAt > now)
            .ToListAsync(ct);
    }

    public async Task<UserAccessTool?> GetByUserAndToolAsync(Guid userId, int accessToolId, CancellationToken ct)
    {
        return await _context.UserAccessTools
            .Include(uat => uat.AccessTool)
            .FirstOrDefaultAsync(uat => uat.UserId == userId && uat.AccessToolId == accessToolId, ct);
    }

    public async Task<UserAccessTool> CreateAsync(UserAccessTool userAccessTool, CancellationToken ct)
    {
        await _context.UserAccessTools.AddAsync(userAccessTool, ct);
        await _context.SaveChangesAsync(ct);
        return userAccessTool;
    }

    public async Task<UserAccessTool> UpdateAsync(UserAccessTool userAccessTool, CancellationToken ct)
    {
        _context.UserAccessTools.Update(userAccessTool);
        await _context.SaveChangesAsync(ct);
        return userAccessTool;
    }

    public async Task<bool> DeleteAsync(int userAccessToolId, CancellationToken ct)
    {
        var userAccessTool = await _context.UserAccessTools.FindAsync(userAccessToolId);
        if (userAccessTool == null)
            return false;

        _context.UserAccessTools.Remove(userAccessTool);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteByUserAndToolAsync(Guid userId, int accessToolId, CancellationToken ct)
    {
        var userAccessTool = await GetByUserAndToolAsync(userId, accessToolId, ct);
        if (userAccessTool == null)
            return false;

        _context.UserAccessTools.Remove(userAccessTool);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> HasAccessAsync(Guid userId, int accessToolId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        return await _context.UserAccessTools
            .AnyAsync(uat => uat.UserId == userId &&
                           uat.AccessToolId == accessToolId &&
                           uat.ExpiredAt > now, ct);
    }
}
