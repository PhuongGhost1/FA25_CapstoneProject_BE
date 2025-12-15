using CusomMapOSM_Domain.Entities.Groups;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Groups;

public interface ISessionGroupRepository
{
    Task<SessionGroup?> GetGroupById(Guid groupId);
    Task<List<SessionGroup>> GetGroupsBySession(Guid sessionId);
    Task<bool> CreateGroup(SessionGroup group);
    Task<bool> UpdateGroup(SessionGroup group);
    Task<bool> DeleteGroup(Guid groupId);
    Task<bool> CheckGroupExists(Guid groupId);
}
