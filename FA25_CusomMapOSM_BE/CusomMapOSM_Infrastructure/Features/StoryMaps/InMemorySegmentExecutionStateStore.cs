using System.Collections.Concurrent;
using CusomMapOSM_Application.Interfaces.Features.StoryMaps;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;

namespace CusomMapOSM_Infrastructure.Features.StoryMaps;

public class InMemorySegmentExecutionStateStore : ISegmentExecutionStateStore
{
    private static readonly ConcurrentDictionary<Guid, SegmentExecutionCheckpoint> _store = new();

    public SegmentExecutionCheckpoint? Get(Guid mapId)
    {
        _store.TryGetValue(mapId, out var cp);
        return cp;
    }

    public void Set(Guid mapId, SegmentExecutionCheckpoint checkpoint)
    {
        _store[mapId] = checkpoint with { UpdatedAt = DateTime.UtcNow };
    }

    public void Reset(Guid mapId)
    {
        _store.TryRemove(mapId, out _);
    }
}


