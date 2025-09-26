using CusomMapOSM_Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CusomMapOSM_Infrastructure.Services;

public class CollaborativeMapService
{
    private readonly IDistributedCache _cache;
    private const string VERSION_KEY_PREFIX = "map_version:";
    private const string LOCK_KEY_PREFIX = "map_lock:";
    private const int LOCK_TIMEOUT_SECONDS = 30;

    public CollaborativeMapService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<bool> TryLockObject(string mapId, string objectId, string userId)
    {
        var lockKey = $"{LOCK_KEY_PREFIX}{mapId}:{objectId}";
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(LOCK_TIMEOUT_SECONDS)
        };

        return await _cache.GetStringAsync(lockKey) == null && 
               await _cache.TrySetStringAsync(lockKey, userId, options);
    }

    public async Task ReleaseLock(string mapId, string objectId)
    {
        var lockKey = $"{LOCK_KEY_PREFIX}{mapId}:{objectId}";
        await _cache.RemoveAsync(lockKey);
    }

    public async Task<(bool success, string error)> ValidateOperation(MapEditOperation operation, string mapId)
    {
        // Check if object is locked by another user
        if (operation.Data is JsonElement dataElement && 
            dataElement.TryGetProperty("objectId", out JsonElement objectIdElement))
        {
            var objectId = objectIdElement.GetString();
            var lockKey = $"{LOCK_KEY_PREFIX}{mapId}:{objectId}";
            var lockedBy = await _cache.GetStringAsync(lockKey);
            
            if (lockedBy != null && lockedBy != operation.UserId)
            {
                return (false, $"Object is locked by user {lockedBy}");
            }
        }

        // Check version conflicts
        var versionKey = $"{VERSION_KEY_PREFIX}{mapId}";
        var currentVersion = await _cache.GetStringAsync(versionKey);
        var operationVersion = operation.Data is JsonElement element && 
                             element.TryGetProperty("version", out JsonElement versionElement)
            ? versionElement.GetInt32()
            : 0;

        if (currentVersion != null && int.Parse(currentVersion) > operationVersion)
        {
            return (false, "Version conflict - please refresh your map");
        }

        return (true, null);
    }

    public async Task<MapEditOperation> TransformOperation(MapEditOperation operation, List<MapEditOperation> concurrentOps)
    {
        // Simple operational transformation
        // For now, we'll just adjust coordinates if there are overlapping changes
        if (operation.Type == "addMarker" || operation.Type == "moveObject")
        {
            foreach (var concurrentOp in concurrentOps)
            {
                if (concurrentOp.Type == operation.Type)
                {
                    // Adjust position slightly if there's overlap
                    if (operation.Data is JsonElement dataElement)
                    {
                        var lat = dataElement.GetProperty("lat").GetDouble();
                        var lng = dataElement.GetProperty("lng").GetDouble();
                        
                        // Offset by a small amount
                        var properties = dataElement.TryGetProperty("properties", out var props) 
                            ? JsonSerializer.Deserialize<object>(props.GetRawText())
                            : new object();

                        var newData = JsonSerializer.Serialize(new
                        {
                            lat = lat + 0.0001,
                            lng = lng + 0.0001,
                            properties = properties
                        });
                        
                        operation = operation with { Data = newData };
                    }
                }
            }
        }

        return operation;
    }

    public async Task IncrementVersion(string mapId)
    {
        var versionKey = $"{VERSION_KEY_PREFIX}{mapId}";
        var currentVersion = await _cache.GetStringAsync(versionKey);
        var newVersion = currentVersion == null ? 1 : int.Parse(currentVersion) + 1;
        await _cache.SetStringAsync(versionKey, newVersion.ToString());
    }
}

public static class DistributedCacheExtensions
{
    public static async Task<bool> TrySetStringAsync(
        this IDistributedCache cache,
        string key,
        string value,
        DistributedCacheEntryOptions options)
    {
        try
        {
            await cache.SetStringAsync(key, value, options);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
