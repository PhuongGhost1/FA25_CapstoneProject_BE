using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;

namespace CusomMapOSM_Application.Interfaces.Features.StoryMaps;

public interface ISegmentExecutionStateStore
{
    SegmentExecutionCheckpoint? Get(Guid mapId);
    void Set(Guid mapId, SegmentExecutionCheckpoint checkpoint);
    void Reset(Guid mapId);
}


