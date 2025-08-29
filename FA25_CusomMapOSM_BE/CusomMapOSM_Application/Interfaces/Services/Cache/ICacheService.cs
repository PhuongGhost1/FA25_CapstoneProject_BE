using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Application.Interfaces.Services.Cache;

public interface ICacheService
{
    // Generic cache operations
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<bool> RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);

    // Template-specific cache operations
    Task<List<T>?> GetTemplateDataAsync<T>(string cacheKey);
    Task SetTemplateDataAsync<T>(string cacheKey, List<T> data, TimeSpan? expiry = null);

    // Batch operations
    Task InvalidateTemplateCacheAsync(string pattern = "*");
    Task WarmupTemplateCacheAsync();
}
