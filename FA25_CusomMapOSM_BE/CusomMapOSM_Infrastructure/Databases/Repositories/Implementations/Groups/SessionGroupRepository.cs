using CusomMapOSM_Domain.Entities.Groups;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Groups;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Groups;

public class SessionGroupRepository : ISessionGroupRepository
{
    private readonly CustomMapOSMDbContext _context;

    public SessionGroupRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<SessionGroup?> GetGroupById(Guid groupId)
    {
        return await _context.SessionGroups
            .FirstOrDefaultAsync(g => g.GroupId == groupId);
    }

    public async Task<List<SessionGroup>> GetGroupsBySession(Guid sessionId)
    {
        return await _context.SessionGroups
            .Where(g => g.SessionId == sessionId)
            .OrderBy(g => g.GroupName)
            .ToListAsync();
    }

    public async Task<bool> CreateGroup(SessionGroup group)
    {
        _context.SessionGroups.Add(group);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateGroup(SessionGroup group)
    {
        _context.SessionGroups.Update(group);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteGroup(Guid groupId)
    {
        var group = await _context.SessionGroups.FindAsync(groupId);
        if (group == null) return false;

        _context.SessionGroups.Remove(group);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> CheckGroupExists(Guid groupId)
    {
        return await _context.SessionGroups.AnyAsync(g => g.GroupId == groupId);
    }
}
