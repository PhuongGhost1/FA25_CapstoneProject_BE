using CusomMapOSM_Domain.Entities.Groups;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Groups;

public interface IGroupMemberRepository
{
    Task<SessionGroupMember?> GetGroupMemberById(Guid groupMemberId);
    Task<List<SessionGroupMember>> GetGroupMembersByGroup(Guid groupId);
    Task<bool> CreateGroupMember(SessionGroupMember groupMember);
    Task<bool> UpdateGroupMember(SessionGroupMember groupMember);
    Task<bool> DeleteGroupMember(Guid groupMemberId);
    Task<bool> CheckGroupMemberExists(Guid groupMemberId);
    Task<List<SessionGroupMember>> GetGroupMembersByGroup(List<Guid> groupIds);
    
}