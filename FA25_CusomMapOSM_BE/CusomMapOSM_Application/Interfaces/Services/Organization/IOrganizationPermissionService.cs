using CusomMapOSM_Domain.Entities.Maps;

namespace CusomMapOSM_Application.Interfaces.Services.Organization;

public interface IOrganizationPermissionService
{
    Task<bool> CanEditMap(Guid mapId, Guid userId);
    Task<bool> CanEditMap(Map map, Guid userId);
    Task<bool> HasOrganizationAccess(Guid currentUserId, Guid mapOwnerId);
}

