using CusomMapOSM_Domain.Entities.Workspaces.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.Workspace.Response;

public record WorkspaceResDto
{
    public required string Result { get; set; }
}

public record WorkspaceDetailDto
{
    public required Guid WorkspaceId { get; set; }
    public Guid? OrgId { get; set; }
    public required string OrgName { get; set; }
    public required Guid CreatedBy { get; set; }
    public required string CreatorName { get; set; }
    public required string WorkspaceName { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public required WorkspaceAccessEnum Access { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public required int MapCount { get; set; }
}

public record GetAllWorkspacesResDto
{
    public required List<WorkspaceDetailDto> Workspaces { get; set; }
}

public record GetWorkspaceByIdResDto
{
    public required WorkspaceDetailDto Workspace { get; set; }
}

public record UpdateWorkspaceResDto
{
    public required string Result { get; set; }
}

public record DeleteWorkspaceResDto
{
    public required string Result { get; set; }
}

public record GetWorkspacesByOrganizationResDto
{
    public required List<WorkspaceDetailDto> Workspaces { get; set; }
}

public record GetMyWorkspacesResDto
{
    public required List<WorkspaceDetailDto> Workspaces { get; set; }
}

public record MapInWorkspaceDto
{
    public required Guid MapId { get; set; }
    public required string MapName { get; set; }
    public string? Description { get; set; }
    public string? PreviewImage { get; set; }
    public required bool IsPublic { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record GetWorkspaceMapsResDto
{
    public required List<MapInWorkspaceDto> Maps { get; set; }
}

public record AddMapToWorkspaceResDto
{
    public required string Result { get; set; }
}

public record RemoveMapFromWorkspaceResDto
{
    public required string Result { get; set; }
}
