using CusomMapOSM_Application.Models.DTOs.Features.Workspace.Response;
using CusomMapOSM_Domain.Entities.Workspaces;

namespace CusomMapOSM_Application.Common.Mappers;

public static class WorkSpaceMapping
{
    public static WorkspaceDetailDto ToDto(this Workspace workspace, int mapCount)
        => new WorkspaceDetailDto
        {
            WorkspaceId = workspace.WorkspaceId,
            OrgId = workspace.OrgId,
            OrgName = workspace.Organization?.OrgName ?? "Unknown Organization",
            CreatedBy = workspace.CreatedBy,
            CreatorName = workspace.Creator.FullName ?? workspace.Creator.Email ?? "Unknown User",
            WorkspaceName = workspace.WorkspaceName,
            Description = workspace.Description,
            Icon = workspace.Icon,
            Access = workspace.Access,
            IsActive = workspace.IsActive,
            CreatedAt = workspace.CreatedAt,
            UpdatedAt = workspace.UpdatedAt,
            MapCount = mapCount
        };
}