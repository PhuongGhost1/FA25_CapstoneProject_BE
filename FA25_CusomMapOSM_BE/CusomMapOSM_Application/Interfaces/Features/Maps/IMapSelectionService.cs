using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using Optional;


namespace CusomMapOSM_Application.Interfaces.Features.Maps;

public interface IMapSelectionService

{

    Task<Option<MapSelectionResponse, Error>> UpdateSelection(Guid mapId, Guid userId, UpdateSelectionRequest request);

    Task<Option<bool, Error>> ClearSelection(Guid mapId, Guid userId);

    Task<Option<List<ActiveMapUserResponse>, Error>> GetActiveUsers(Guid mapId);

    Task<Option<MapSelectionResponse, Error>> GetUserSelection(Guid mapId, Guid userId);

    Task CleanupInactiveSelections(Guid mapId);

    Task<Option<bool, Error>> UserJoinMap(Guid mapId, Guid userId, string connectionId);

    Task<Option<bool, Error>> UserLeaveMap(Guid mapId, Guid userId);

    Task<Option<Guid, Error>> GetMapIdFromConnection(string connectionId);

    Task RemoveConnectionMapping(string connectionId);

    Task<Option<string, Error>> GetUserHighlightColor(Guid userId);

}