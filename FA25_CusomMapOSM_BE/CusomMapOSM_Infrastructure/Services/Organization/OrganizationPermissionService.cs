using System.Linq;
using CusomMapOSM_Application.Interfaces.Services.Organization;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Organizations.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Workspaces;

namespace CusomMapOSM_Infrastructure.Services.Organization;

public class OrganizationPermissionService : IOrganizationPermissionService
{
    private readonly IMapRepository _mapRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IOrganizationRepository _organizationRepository;

    public OrganizationPermissionService(
        IMapRepository mapRepository,
        IWorkspaceRepository workspaceRepository,
        IOrganizationRepository organizationRepository)
    {
        _mapRepository = mapRepository;
        _workspaceRepository = workspaceRepository;
        _organizationRepository = organizationRepository;
    }

    public async Task<bool> CanEditMap(Guid mapId, Guid userId)
    {
        var map = await _mapRepository.GetMapById(mapId);
        if (map is null)
        {
            return false;
        }

        return await CanEditMap(map, userId);
    }

    public async Task<bool> CanEditMap(Map map, Guid userId)
    {
        if (map.UserId == userId)
        {
            return true;
        }

        if (await HasWorkspaceEditPermission(map.WorkspaceId, userId))
        {
            return true;
        }

        return await HasOrganizationAccess(userId, map.UserId);
    }

    public async Task<bool> HasOrganizationAccess(Guid currentUserId, Guid mapOwnerId)
    {
        var ownerOrganizations = await _organizationRepository.GetUserOrganizations(mapOwnerId);
        if (ownerOrganizations.Count == 0)
        {
            return false;
        }

        var userOrganizations = await _organizationRepository.GetUserOrganizations(currentUserId);
        if (userOrganizations.Count == 0)
        {
            return false;
        }

        var ownerOrgIds = ownerOrganizations.Select(o => o.OrgId).ToHashSet();
        var commonOrganizations = userOrganizations
            .Where(uo => ownerOrgIds.Contains(uo.OrgId))
            .ToList();

        if (commonOrganizations.Count == 0)
        {
            return false;
        }

        return commonOrganizations.Any(member => member.Role <= OrganizationMemberTypeEnum.Member);
    }

    private async Task<bool> HasWorkspaceEditPermission(Guid? workspaceId, Guid userId)
    {
        if (!workspaceId.HasValue)
        {
            return false;
        }

        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId.Value);
        if (workspace is null || !workspace.IsActive)
        {
            return false;
        }

        if (workspace.CreatedBy == userId)
        {
            return true;
        }

        if (!workspace.OrgId.HasValue)
        {
            return false;
        }

        var member = await _organizationRepository
            .GetOrganizationMemberByUserAndOrg(userId, workspace.OrgId.Value);

        return member is not null && member.Role < OrganizationMemberTypeEnum.Member;
    }
}

