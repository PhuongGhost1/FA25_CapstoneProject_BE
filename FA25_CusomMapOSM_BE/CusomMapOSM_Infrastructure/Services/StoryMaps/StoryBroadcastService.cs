using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using CusomMapOSM_Application.Interfaces.Services.StoryMaps;
using CusomMapOSM_Application.Models.StoryMaps;

namespace CusomMapOSM_Infrastructure.Services.StoryMaps;

public class StoryBroadcastService : IStoryBroadcastService
{
    private sealed class StoryBroadcastSession
    {
        public StoryBroadcastSessionInfo Info { get; init; } = default!;
        public string? HostConnectionId { get; set; }
    }

    private readonly ConcurrentDictionary<string, StoryBroadcastSession> _sessions = new();
    private readonly TimeSpan _defaultTtl = TimeSpan.FromHours(2);
    private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    public StoryBroadcastSessionInfo CreateSession(Guid mapId, Guid? storyMapId, TimeSpan? ttl = null)
    {
        CleanupExpired();

        StoryBroadcastSessionInfo sessionInfo;
        string code;
        do
        {
            code = GenerateCode();
        } while (_sessions.ContainsKey(code));

        sessionInfo = new StoryBroadcastSessionInfo
        {
            SessionCode = code,
            MapId = mapId,
            StoryMapId = storyMapId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(ttl ?? _defaultTtl)
        };

        var session = new StoryBroadcastSession
        {
            Info = sessionInfo
        };

        _sessions[code] = session;
        return sessionInfo;
    }

    public bool TryGetSession(string sessionCode, out StoryBroadcastSessionInfo? session)
    {
        session = null;
        if (_sessions.TryGetValue(sessionCode, out var stored))
        {
            if (IsExpired(stored.Info))
            {
                _sessions.TryRemove(sessionCode, out _);
                return false;
            }

            session = stored.Info;
            return true;
        }

        return false;
    }

    public bool EndSession(string sessionCode)
    {
        return _sessions.TryRemove(sessionCode, out _);
    }

    public void BindHost(string sessionCode, string connectionId)
    {
        if (_sessions.TryGetValue(sessionCode, out var session))
        {
            session.HostConnectionId = connectionId;
        }
    }

    public bool IsHost(string sessionCode, string connectionId)
    {
        return _sessions.TryGetValue(sessionCode, out var session) &&
               string.Equals(session.HostConnectionId, connectionId, StringComparison.Ordinal);
    }

    public void ReleaseHost(string connectionId)
    {
        foreach (var entry in _sessions)
        {
            if (string.Equals(entry.Value.HostConnectionId, connectionId, StringComparison.Ordinal))
            {
                entry.Value.HostConnectionId = null;
            }
        }
    }

    public void UpdateState(string sessionCode, StoryBroadcastState state)
    {
        if (_sessions.TryGetValue(sessionCode, out var session))
        {
            state.Timestamp = DateTime.UtcNow;
            session.Info.LastState = state;
        }
    }

    public StoryBroadcastState? GetState(string sessionCode)
    {
        return _sessions.TryGetValue(sessionCode, out var session) ? session.Info.LastState : null;
    }

    private void CleanupExpired()
    {
        foreach (var entry in _sessions)
        {
            if (IsExpired(entry.Value.Info))
            {
                _sessions.TryRemove(entry.Key, out _);
            }
        }
    }

    private static bool IsExpired(StoryBroadcastSessionInfo info)
    {
        return info.ExpiresAt.HasValue && info.ExpiresAt.Value <= DateTime.UtcNow;
    }

    private string GenerateCode()
    {
        Span<byte> buffer = stackalloc byte[4];
        _rng.GetBytes(buffer);
        var value = BitConverter.ToUInt32(buffer);
        var code = value % 1_000_000; // 6 digits
        return code.ToString("D6");
    }
}
