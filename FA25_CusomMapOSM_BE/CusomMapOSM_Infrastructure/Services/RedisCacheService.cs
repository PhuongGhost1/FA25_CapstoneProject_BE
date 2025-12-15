using CusomMapOSM_Application.Interfaces.Services.Cache;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CusomMapOSM_Infrastructure.Services;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _redis;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public RedisCacheService(IDistributedCache cache, IConnectionMultiplexer redis)
    {
        _cache = cache;
        _redis = redis;
    }

    public async Task<T?> Get<T>(string key)
    {
        var cachedData = await _cache.GetAsync(key);
        if (cachedData == null || cachedData.Length == 0)
            return default;

        var json = Encoding.UTF8.GetString(cachedData);
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    public async Task Set<T>(string key, T value)
    {
        await Set(key, value, TimeSpan.FromDays(1));
    }

    public async Task Set<T>(string key, T value, TimeSpan expiration)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        var data = Encoding.UTF8.GetBytes(json);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        await _cache.SetAsync(key, data, options);
    }

    public async Task<bool> Update<T>(string key, T value)
    {
        var exists = await Exists(key);
        if (!exists)
            return false;

        await Set(key, value);
        return true;
    }

    public async Task Remove(string key)
    {
        await _cache.RemoveAsync(key);
    }

    public async Task<bool> Exists(string key)
    {
        var data = await _cache.GetAsync(key);
        return data != null && data.Length > 0;
    }

    public async Task Clear()
    {
        await ClearWithPattern("*");
    }

    public async Task ClearWithPattern(string pattern)
    {
        var endpoints = _redis.GetEndPoints();
        if (endpoints == null || endpoints.Length == 0)
            return;

        var server = _redis.GetServer(endpoints.First());
        var database = _redis.GetDatabase();
        
        const string instanceName = "IMOS:";
        var searchPattern = pattern.StartsWith(instanceName, StringComparison.Ordinal)
            ? pattern
            : $"{instanceName}{pattern}";

        var keys = new List<RedisKey>();
        await foreach (var key in server.KeysAsync(pattern: searchPattern))
        {
            keys.Add(key);
        }

        if (keys.Count > 0)
        {
            await database.KeyDeleteAsync(keys.ToArray());
        }
    }

    public async Task ForceLogout(Guid userId)
    {
        var key = $"user:{userId}:token";
        await Remove(key);
    }
}