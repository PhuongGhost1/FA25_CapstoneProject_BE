using CusomMapOSM_Application.Models.StoryMaps;

namespace CusomMapOSM_Application.Interfaces.Services.StoryMaps;

public interface IStoryBroadcastService
{
    StoryBroadcastSessionInfo CreateSession(Guid mapId, Guid? storyMapId, TimeSpan? ttl = null);
    bool TryGetSession(string sessionCode, out StoryBroadcastSessionInfo? session);
    bool EndSession(string sessionCode);
    void BindHost(string sessionCode, string connectionId);
    bool IsHost(string sessionCode, string connectionId);
    void ReleaseHost(string connectionId);
    void UpdateState(string sessionCode, StoryBroadcastState state);
    StoryBroadcastState? GetState(string sessionCode);
}
