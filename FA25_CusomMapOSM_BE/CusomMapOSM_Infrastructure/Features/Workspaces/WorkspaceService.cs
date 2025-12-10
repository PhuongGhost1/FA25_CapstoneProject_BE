using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Common.Mappers;
using CusomMapOSM_Application.Interfaces.Features.Workspaces;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Workspace.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Workspace.Response;
using CusomMapOSM_Domain.Entities.Organizations.ErrorMessages;
using CusomMapOSM_Domain.Entities.Workspaces;
using CusomMapOSM_Domain.Entities.Workspaces.ErrorMessages;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Authentication;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Workspaces;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.QuestionBanks;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.Workspaces;

public class WorkspaceService : IWorkspaceService
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMapRepository _mapRepository;
    private readonly IQuestionBankRepository _questionBankRepository;
    private readonly ICurrentUserService _currentUserService;

    public WorkspaceService(
        IWorkspaceRepository workspaceRepository,
        IOrganizationRepository organizationRepository,
        IMapRepository mapRepository,
        IQuestionBankRepository questionBankRepository,
        ICurrentUserService currentUserService)
    {
        _workspaceRepository = workspaceRepository;
        _organizationRepository = organizationRepository;
        _mapRepository = mapRepository;
        _questionBankRepository = questionBankRepository;
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

        var workspace = new Workspace
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
        var currentUser = _currentUserService.GetUserId();
        var workspaces = await _workspaceRepository.GetByUserIdAsync(currentUser.Value);
        
        var workspaceDtos = new List<WorkspaceDetailDto>();
        foreach (var workspace in workspaces)
        {
            var mapCount = await _workspaceRepository.GetMapCountAsync(workspace.WorkspaceId);
            workspaceDtos.Add(workspace.ToDto(mapCount));
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

        var mapCount = await _workspaceRepository.GetMapCountAsync(workspace.WorkspaceId);
        var workspaceDto = workspace.ToDto(mapCount);

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

        // Check if workspace has active maps
        var maps = await _mapRepository.GetByWorkspaceIdAsync(id);
        var activeMaps = maps.Where(m => m.IsActive).ToList();
        if (activeMaps.Any())
        {
            return Option.None<DeleteWorkspaceResDto, Error>(
                Error.ValidationError("Workspace.HasActiveMaps", 
                    "Cannot delete workspace while it contains active maps. Please delete or move all maps first."));
        }

        // Check if workspace has question banks
        var questionBanks = await _questionBankRepository.GetQuestionBanksByWorkspaceId(id);
        var activeQuestionBanks = questionBanks.Where(qb => qb.IsActive).ToList();
        if (activeQuestionBanks.Any())
        {
            return Option.None<DeleteWorkspaceResDto, Error>(
                Error.ValidationError("Workspace.HasQuestionBanks", 
                    "Cannot delete workspace while it contains question banks. Please delete or move all question banks first."));
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
            var mapCount = await _workspaceRepository.GetMapCountAsync(workspace.WorkspaceId);
            workspaceDtos.Add(workspace.ToDto(mapCount));
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
            var mapCount = await _workspaceRepository.GetMapCountAsync(workspace.WorkspaceId);
            workspaceDtos.Add(workspace.ToDto(mapCount));
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

        var maps = await _mapRepository.GetByWorkspaceIdAsync(workspaceId);
        var mapDtos = maps.Select(m => new MapInWorkspaceDto
        {
            MapId = m.MapId,
            MapName = m.MapName,
            Description = m.Description,
            PreviewImage = m.PreviewImage,
            IsPublic = m.IsPublic,
            IsStoryMap = m.IsStoryMap,
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
}
