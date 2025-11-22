using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Groups.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Groups.Response;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Groups;

public interface IGroupCollaborationService
{
    Task<Option<GroupResponse, Error>> CreateGroup(CreateGroupRequest request);
    Task<Option<List<GroupResponse>, Error>> GetGroupsBySession(Guid sessionId);
    Task<Option<GroupResponse, Error>> GetGroupById(Guid groupId);
    Task<Option<bool, Error>> DeleteGroup(Guid groupId);
    Task<Option<GroupSubmissionResponse, Error>> SubmitGroupWork(SubmitGroupWorkRequest request);
    Task<Option<List<GroupSubmissionResponse>, Error>> GetGroupSubmissions(Guid groupId);
    Task<Option<List<GroupSubmissionResponse>, Error>> GetSessionSubmissions(Guid sessionId);
    Task<Option<GroupSubmissionResponse, Error>> GradeSubmission(GradeSubmissionRequest request);
}
