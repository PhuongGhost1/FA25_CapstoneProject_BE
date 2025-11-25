using System.Linq;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Maps;
using CusomMapOSM_Application.Interfaces.Services.Maps;
using CusomMapOSM_Application.Interfaces.Services.Organization;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Domain.Entities.Maps;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.Maps;

public class MapHistoryService : IMapHistoryService
{
    private const int MaxHistory = 10;
    private readonly IMapHistoryStore _store;
    private readonly IOrganizationPermissionService _organizationPermissionService;
    private readonly ICurrentUserService _currentUserService;

    public MapHistoryService(IMapHistoryStore store, IOrganizationPermissionService organizationPermissionService, ICurrentUserService currentUserService)
    {
        _store = store;
        _organizationPermissionService = organizationPermissionService;
        _currentUserService = currentUserService;
    }

    public async Task<Option<bool, Error>> RecordSnapshot(Guid mapId, Guid userId, string snapshotJson, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(snapshotJson))
        {
            return Option.None<bool, Error>(Error.ValidationError("History.InvalidSnapshot", "Snapshot is empty"));
        }
        var history = new MapHistory
        {
            MapId = mapId,
            UserId = userId,
            SnapshotData = snapshotJson,
            CreatedAt = DateTime.UtcNow
        };
        await _store.AddAsync(history, ct);
        return Option.Some<bool, Error>(true);
    }

    public async Task<Option<string, Error>> Undo(Guid mapId, Guid userId, int steps, CancellationToken ct = default)
    {
        var currentUser = _currentUserService.GetUserId();
        if (currentUser is null)
        {
            return Option.None<string, Error>(
                Error.Unauthorized("History.Unauthorized", "User not authenticated"));
        }
        var canEditMap = await _organizationPermissionService.CanEditMap(mapId, currentUser.Value);
        if (!canEditMap)
        {
            return Option.None<string, Error>(
                Error.Forbidden("History.Forbidden", "You don't have permission to modify this map"));
        }
        if (steps <= 0 || steps > MaxHistory)
        {
            return Option.None<string, Error>(Error.ValidationError("History.InvalidSteps", $"Steps must be between 1 and {MaxHistory}"));
        }

        var last = await _store.GetLastAsync(mapId, steps, ct);
        if (last.Count < steps)
        {
            return Option.None<string, Error>(Error.NotFound("History.NotEnough", "Not enough history to undo"));
        }

        var ordered = last.OrderByDescending(h => h.CreatedAt).ToList();
        var target = ordered[steps - 1];
        // Return the snapshot; the caller should apply it and persist map state accordingly
        return Option.Some<string, Error>(target.SnapshotData);
    }
}


