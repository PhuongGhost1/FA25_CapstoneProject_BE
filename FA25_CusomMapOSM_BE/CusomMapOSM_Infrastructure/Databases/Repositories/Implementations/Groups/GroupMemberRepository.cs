using CusomMapOSM_Domain.Entities.Groups;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Groups;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Groups;

public class GroupMemberRepository : IGroupMemberRepository
{
    private readonly CustomMapOSMDbContext _context;

    public GroupMemberRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<SessionGroupMember?> GetGroupMemberById(Guid groupMemberId)
    {
        return await _context.SessionGroupMembers
            .FirstOrDefaultAsync(gm => gm.GroupMemberId == groupMemberId);
    }

    public async Task<List<SessionGroupMember>> GetGroupMembersByGroup(Guid groupId)
    {
        return await _context.SessionGroupMembers
            .Where(gm => gm.GroupId == groupId)
            .ToListAsync();
    }

    public async Task<bool> CreateGroupMember(SessionGroupMember groupMember)
    {
        _context.SessionGroupMembers.Add(groupMember);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateGroupMember(SessionGroupMember groupMember)
    {
        _context.SessionGroupMembers.Update(groupMember);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteGroupMember(Guid groupMemberId)
    {
        var groupMember = await _context.SessionGroupMembers.FindAsync(groupMemberId);
        if (groupMember == null) return false;

        _context.SessionGroupMembers.Remove(groupMember);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> CheckGroupMemberExists(Guid groupMemberId)
    {
        return await _context.SessionGroupMembers.AnyAsync(gm => gm.GroupMemberId == groupMemberId);
    }

    public async Task<List<SessionGroupMember>> GetGroupMembersByGroup(List<Guid> groupIds)
    {
        return await _context.SessionGroupMembers
            .Where(gm => groupIds.Contains(gm.GroupId))
            .ToListAsync();
    }
}