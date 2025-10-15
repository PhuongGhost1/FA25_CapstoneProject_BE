using CusomMapOSM_Application.Common.Errors;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Maps;

public interface IMapHistoryService
{
    Task<Option<bool, Error>> RecordSnapshot(Guid mapId, Guid userId, string snapshotJson, CancellationToken ct = default);
    Task<Option<string, Error>> Undo(Guid mapId, Guid userId, int steps, CancellationToken ct = default);
}


