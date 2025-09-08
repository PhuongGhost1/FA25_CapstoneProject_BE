namespace CusomMapOSM_Application.Interfaces;

public interface IMapCollaborationService
{
    Task<bool> TryLockObject(string mapId, string objectId, string userId);
    Task ReleaseLock(string mapId, string objectId);
    Task<(bool success, string error)> ValidateOperation(MapEditOperation operation, string mapId);
    Task<MapEditOperation> TransformOperation(MapEditOperation operation, List<MapEditOperation> concurrentOps);
    Task IncrementVersion(string mapId);
    Task<List<MapCollaborator>> GetActiveCollaborators(string mapId);
    Task UpdateCollaboratorStatus(string mapId, string userId, double? lat = null, double? lng = null, string? action = null);
}

public record MapCollaborator
{
    public string UserId { get; init; } = default!;
    public DateTime JoinedAt { get; init; }
    public DateTime LastActiveAt { get; init; }
    public string? CurrentAction { get; init; }
    public double? CursorLat { get; init; }
    public double? CursorLng { get; init; }
}
