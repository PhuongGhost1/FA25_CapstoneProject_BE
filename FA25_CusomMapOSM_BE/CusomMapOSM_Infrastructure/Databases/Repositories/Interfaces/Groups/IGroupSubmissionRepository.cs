using CusomMapOSM_Domain.Entities.Groups;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Groups;

public interface IGroupSubmissionRepository
{
    Task<GroupSubmission?> GetSubmissionById(Guid submissionId);
    Task<List<GroupSubmission>> GetSubmissionsByGroup(Guid groupId);
    Task<List<GroupSubmission>> GetSubmissionsBySession(Guid sessionId);
    Task<bool> CreateSubmission(GroupSubmission submission);
    Task<bool> UpdateSubmission(GroupSubmission submission);
    Task<bool> DeleteSubmission(Guid submissionId);
}
