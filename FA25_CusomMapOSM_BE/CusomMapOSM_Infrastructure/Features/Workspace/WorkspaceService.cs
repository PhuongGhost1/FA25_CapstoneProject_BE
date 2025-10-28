using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Workspace;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Workspace.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Workspace.Response;
using CusomMapOSM_Domain.Entities.Organizations.ErrorMessages;
using DommainWorkspaces = CusomMapOSM_Domain.Entities.Workspaces;
using CusomMapOSM_Domain.Entities.Workspaces.ErrorMessages;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Workspace;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.Workspace;

public class WorkspaceService : IWorkspaceService
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMapRepository _mapRepository;
    private readonly ICurrentUserService _currentUserService;

    public WorkspaceService(
        IWorkspaceRepository workspaceRepository,
        IOrganizationRepository organizationRepository,
        IMapRepository mapRepository,
        ICurrentUserService currentUserService)
    {
        _workspaceRepository = workspaceRepository;
        _organizationRepository = organizationRepository;
        _mapRepository = mapRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Option<WorkspaceResDto, Error>> Create(WorkspaceReqDto req)
    {
        var currentUserId = _currentUserService.GetUserId()!.Value;

        var organization = await _organizationRepository.GetOrganizationById(req.OrgId);
        if (organization == null)
        {
            return Option.None<WorkspaceResDto, Error>(Error.NotFound("Organization.NotFound", OrganizationErrors.OrganizationNotFound));
        }

        var workspace = new DommainWorkspaces.Workspace
        {
            WorkspaceId = Guid.NewGuid(),
            OrgId = req.OrgId,
            CreatedBy = currentUserId,
            WorkspaceName = req.WorkspaceName,
            Description = req.Description,
            Icon = req.Icon,
            Access = req.Access,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Organization = null!,
            Creator = null!
        };

        await _workspaceRepository.CreateAsync(workspace);

        return Option.Some<WorkspaceResDto, Error>(new WorkspaceResDto
        {
            Result = "Workspace created successfully"
        });
    }

    public async Task<Option<GetAllWorkspacesResDto, Error>> GetAll()
    {
        var workspaces = await _workspaceRepository.GetAllAsync();
        var workspaceDtos = new List<WorkspaceDetailDto>();
        
        foreach (var workspace in workspaces)
        {
            workspaceDtos.Add(await MapToWorkspaceDetailDto(workspace, _mapRepository));
        }

        return Option.Some<GetAllWorkspacesResDto, Error>(new GetAllWorkspacesResDto
        {
            Workspaces = workspaceDtos
        });
    }

    public async Task<Option<GetWorkspaceByIdResDto, Error>> GetById(Guid id)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(id);
        if (workspace == null)
        {
            return Option.None<GetWorkspaceByIdResDto, Error>(Error.NotFound(WorkspaceErrors.WorkspaceNotFound, WorkspaceErrors.WorkspaceNotFound));
        }

        var workspaceDto = await MapToWorkspaceDetailDto(workspace, _mapRepository);

        return Option.Some<GetWorkspaceByIdResDto, Error>(new GetWorkspaceByIdResDto
        {
            Workspace = workspaceDto
        });
    }

    public async Task<Option<UpdateWorkspaceResDto, Error>> Update(Guid id, WorkspaceReqDto req)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(id);
        if (workspace == null)
        {
            return Option.None<UpdateWorkspaceResDto, Error>(Error.NotFound("Workspace.NotFound", WorkspaceErrors.WorkspaceNotFound));
        }

        workspace.WorkspaceName = req.WorkspaceName;
        workspace.Description = req.Description;
        workspace.Icon = req.Icon;
        workspace.Access = req.Access;
        workspace.UpdatedAt = DateTime.UtcNow;

        await _workspaceRepository.UpdateAsync(workspace);

        return Option.Some<UpdateWorkspaceResDto, Error>(new UpdateWorkspaceResDto
        {
            Result = "Workspace updated successfully"
        });
    }

    public async Task<Option<DeleteWorkspaceResDto, Error>> Delete(Guid id)
    {
        var exists = await _workspaceRepository.ExistsAsync(id);
        if (!exists)
        {
            return Option.None<DeleteWorkspaceResDto, Error>(Error.NotFound("Workspace.NotFound", WorkspaceErrors.WorkspaceNotFound));
        }

        await _workspaceRepository.DeleteAsync(id);

        return Option.Some<DeleteWorkspaceResDto, Error>(new DeleteWorkspaceResDto
        {
            Result = "Workspace deleted successfully"
        });
    }

    public async Task<Option<GetWorkspacesByOrganizationResDto, Error>> GetByOrganization(Guid orgId)
    {
        var workspaces = await _workspaceRepository.GetByOrganizationIdAsync(orgId);
        var workspaceDtos = new List<WorkspaceDetailDto>();
        
        foreach (var workspace in workspaces)
        {
            workspaceDtos.Add(await MapToWorkspaceDetailDto(workspace, _mapRepository));
        }

        return Option.Some<GetWorkspacesByOrganizationResDto, Error>(new GetWorkspacesByOrganizationResDto
        {
            Workspaces = workspaceDtos
        });
    }

    public async Task<Option<GetMyWorkspacesResDto, Error>> GetMyWorkspaces()
    {
        var currentUserId = _currentUserService.GetUserId()!.Value;
        var workspaces = await _workspaceRepository.GetByUserIdAsync(currentUserId);
        var workspaceDtos = new List<WorkspaceDetailDto>();
        
        foreach (var workspace in workspaces)
        {
            workspaceDtos.Add(await MapToWorkspaceDetailDto(workspace, _mapRepository));
        }

        return Option.Some<GetMyWorkspacesResDto, Error>(new GetMyWorkspacesResDto
        {
            Workspaces = workspaceDtos
        });
    }

    public async Task<Option<GetWorkspaceMapsResDto, Error>> GetWorkspaceMaps(Guid workspaceId)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);
        if (workspace == null)
        {
            return Option.None<GetWorkspaceMapsResDto, Error>(Error.NotFound("Workspace.NotFound", WorkspaceErrors.WorkspaceNotFound));
        }

        // Get maps from MapRepository instead of using collection
        var maps = await _mapRepository.GetByWorkspaceIdAsync(workspaceId);
        var mapDtos = maps.Select(m => new MapInWorkspaceDto
        {
            MapId = m.MapId,
            MapName = m.MapName,
            Description = m.Description,
            PreviewImage = m.PreviewImage,
            IsPublic = m.IsPublic,
            IsActive = m.IsActive,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        }).ToList();

        return Option.Some<GetWorkspaceMapsResDto, Error>(new GetWorkspaceMapsResDto
        {
            Maps = mapDtos
        });
    }

    public async Task<Option<AddMapToWorkspaceResDto, Error>> AddMapToWorkspace(Guid workspaceId, AddMapToWorkspaceReqDto req)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);
        if (workspace == null)
        {
            return Option.None<AddMapToWorkspaceResDto, Error>(Error.NotFound("Workspace.NotFound", WorkspaceErrors.WorkspaceNotFound));
        }

        var map = await _mapRepository.GetMapById(req.MapId);
        if (map == null)
        {
            return Option.None<AddMapToWorkspaceResDto, Error>(Error.NotFound("Map.NotFound", "Map not found"));
        }

        map.WorkspaceId = workspaceId;
        await _mapRepository.UpdateMap(map);

        return Option.Some<AddMapToWorkspaceResDto, Error>(new AddMapToWorkspaceResDto
        {
            Result = "Map added to workspace successfully"
        });
    }

    public async Task<Option<RemoveMapFromWorkspaceResDto, Error>> RemoveMapFromWorkspace(Guid workspaceId, Guid mapId)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);
        if (workspace == null)
        {
            return Option.None<RemoveMapFromWorkspaceResDto, Error>(Error.NotFound("Workspace.NotFound", WorkspaceErrors.WorkspaceNotFound));
        }

        var map = await _mapRepository.GetMapById(mapId);
        if (map == null)
        {
            return Option.None<RemoveMapFromWorkspaceResDto, Error>(Error.NotFound("Map.NotFound", "Map not found"));
        }

        map.WorkspaceId = null;
        await _mapRepository.UpdateMap(map);

        return Option.Some<RemoveMapFromWorkspaceResDto, Error>(new RemoveMapFromWorkspaceResDto
        {
            Result = "Map removed from workspace successfully"
        });
    }

    private static async Task<WorkspaceDetailDto> MapToWorkspaceDetailDto(DommainWorkspaces.Workspace workspace, IMapRepository mapRepository)
    {
        var mapCount = await mapRepository.GetByWorkspaceIdAsync(workspace.WorkspaceId);
        
        return new WorkspaceDetailDto
        {
            WorkspaceId = workspace.WorkspaceId,
            OrgId = workspace.OrgId,
            OrgName = workspace.Organization?.OrgName ?? "",
            CreatedBy = workspace.CreatedBy,
            CreatorName = workspace.Creator?.FullName ?? "",
            WorkspaceName = workspace.WorkspaceName,
            Description = workspace.Description,
            Icon = workspace.Icon,
            Access = workspace.Access,
            IsActive = workspace.IsActive,
            CreatedAt = workspace.CreatedAt,
            UpdatedAt = workspace.UpdatedAt,
            MapCount = mapCount.Count
        };
    }
}
