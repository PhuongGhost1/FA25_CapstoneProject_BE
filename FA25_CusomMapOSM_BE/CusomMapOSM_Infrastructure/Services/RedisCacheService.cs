using CusomMapOSM_Application.Interfaces.Services.Cache;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CusomMapOSM_Infrastructure.Services;

public class RedisCacheService : CusomMapOSM_Application.Interfaces.Services.Cache.ICacheService, CusomMapOSM_Application.Interfaces.Services.Cache.IRedisCacheService
{
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var cachedData = await _cache.GetAsync(key);
        if (cachedData == null)
            return default;

        var json = System.Text.Encoding.UTF8.GetString(cachedData);
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        var data = System.Text.Encoding.UTF8.GetBytes(json);

        var options = new DistributedCacheEntryOptions();
        if (expiry.HasValue)
            options.AbsoluteExpirationRelativeToNow = expiry;

        await _cache.SetAsync(key, data, options);
    }

    public async Task<bool> RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
        return true;
    }

    public async Task<bool> ExistsAsync(string key)
    {
        var data = await _cache.GetAsync(key);
        return data != null;
    }

    // Template-specific cache operations
    public async Task<List<T>?> GetTemplateDataAsync<T>(string cacheKey)
    {
        return await GetAsync<List<T>>(cacheKey);
    }

    public async Task SetTemplateDataAsync<T>(string cacheKey, List<T> data, TimeSpan? expiry = null)
    {
        await SetAsync(cacheKey, data, expiry);
    }

    // Batch operations for template cache
    public async Task InvalidateTemplateCacheAsync(string pattern = "*")
    {
        // Note: Redis pattern matching would require additional setup
        // For now, we'll use specific keys
        var keys = new[]
        {
            "templates:all",
            "templates:category:Business",
            "templates:category:Planning",
            "templates:category:Logistics",
            "templates:category:Research",
            "templates:category:Operations",
            "templates:category:Education"
        };

        foreach (var key in keys)
        {
            if (key.Contains(pattern) || pattern == "*")
            {
                await RemoveAsync(key);
            }
        }
    }

    public async Task WarmupTemplateCacheAsync()
    {
        // This would be called on application startup or scheduled job
        // to preload frequently used templates into cache

        // Example: Preload all featured templates
        // var featuredTemplates = await _templateRepository.GetFeaturedTemplates();
        // await SetTemplateDataAsync("templates:featured", featuredTemplates, TimeSpan.FromHours(1));
    }

    // IRedisCacheService implementation
    public async Task<T?> Get<T>(string key)
    {
        return await GetAsync<T>(key);
    }

    public async Task Set<T>(string key, T value)
    {
        await SetAsync(key, value);
    }

    public async Task Set<T>(string key, T value, TimeSpan expiration)
    {
        await SetAsync(key, value, expiration);
    }

    public async Task<bool> Update<T>(string key, T value)
    {
        if (await ExistsAsync(key))
        {
            await SetAsync(key, value);
            return true;
        }
        return false;
    }

    public async Task Remove(string key)
    {
        await RemoveAsync(key);
    }

    public async Task<bool> Exists(string key)
    {
        return await ExistsAsync(key);
    }

    public async Task Clear()
    {
        // Note: Redis doesn't have a direct "clear all" command
        // This is a placeholder implementation
        // In a real scenario, you might use Redis SCAN or maintain a list of keys to clear
        throw new NotImplementedException("Clear operation requires key pattern matching or key registry");
    }

    public async Task ClearWithPattern(string pattern)
    {
        await InvalidateTemplateCacheAsync(pattern);
    }

    public async Task ForceLogout(Guid userId)
    {
        var key = $"user:{userId}:token";
        await RemoveAsync(key);
    }
}