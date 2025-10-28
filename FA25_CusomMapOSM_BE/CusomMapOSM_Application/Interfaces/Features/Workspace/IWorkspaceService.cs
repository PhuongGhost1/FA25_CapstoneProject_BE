using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Workspace.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Workspace.Response;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Workspace;

public interface IWorkspaceService
{
    Task<Option<WorkspaceResDto, Error>> Create(WorkspaceReqDto req);
    Task<Option<GetAllWorkspacesResDto, Error>> GetAll();
    Task<Option<GetWorkspaceByIdResDto, Error>> GetById(Guid id);
    Task<Option<UpdateWorkspaceResDto, Error>> Update(Guid id, WorkspaceReqDto req);
    Task<Option<DeleteWorkspaceResDto, Error>> Delete(Guid id);
    Task<Option<GetWorkspacesByOrganizationResDto, Error>> GetByOrganization(Guid orgId);
    Task<Option<GetMyWorkspacesResDto, Error>> GetMyWorkspaces();
    Task<Option<GetWorkspaceMapsResDto, Error>> GetWorkspaceMaps(Guid workspaceId);
    Task<Option<AddMapToWorkspaceResDto, Error>> AddMapToWorkspace(Guid workspaceId, AddMapToWorkspaceReqDto req);
    Task<Option<RemoveMapFromWorkspaceResDto, Error>> RemoveMapFromWorkspace(Guid workspaceId, Guid mapId);
}
