using CusomMapOSM_Domain.Entities.Workspaces.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.Workspace.Request;

public record WorkspaceReqDto
{
    public required Guid OrgId { get; set; }
    public required string WorkspaceName { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public WorkspaceAccessEnum Access { get; set; } = WorkspaceAccessEnum.AllMembers;
}

public record UpdateWorkspaceReqDto
{
    public required Guid WorkspaceId { get; set; }
    public required string WorkspaceName { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public WorkspaceAccessEnum Access { get; set; }
}

public record GetWorkspaceByIdReqDto
{
    public required Guid WorkspaceId { get; set; }
}

public record DeleteWorkspaceReqDto
{
    public required Guid WorkspaceId { get; set; }
}

public record GetWorkspacesByOrganizationReqDto
{
    public required Guid OrgId { get; set; }
}

public record AddMapToWorkspaceReqDto
{
    public required Guid MapId { get; set; }
}

public record RemoveMapFromWorkspaceReqDto
{
    public required Guid WorkspaceId { get; set; }
    public required Guid MapId { get; set; }
}
