using CusomMapOSM_Application.Interfaces.Services.Cache;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using Microsoft.Extensions.DependencyInjection;

namespace CusomMapOSM_Infrastructure.Services;

public class TemplateCacheManager
{
    private readonly IServiceProvider _serviceProvider;

    public TemplateCacheManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task WarmupCacheAsync()
    {
        Console.WriteLine("🔄 Starting template cache warmup...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
            var mapRepository = scope.ServiceProvider.GetRequiredService<IMapRepository>();

            // 1. Cache all templates
            var allTemplates = await mapRepository.GetMapTemplates();
            await cacheService.SetTemplateDataAsync("templates:all", allTemplates, TimeSpan.FromMinutes(30));
            Console.WriteLine($"Cached {allTemplates.Count} templates");

            // 2. Cache templates by category
            var categories = Enum.GetValues(typeof(CusomMapOSM_Domain.Entities.Maps.Enums.MapTemplateCategoryEnum));
            foreach (var category in categories)
            {
                var categoryName = category.ToString();
                if (!string.IsNullOrEmpty(categoryName))
                {
                    var categoryTemplates = await mapRepository.GetMapsByCategory(categoryName);
                    await cacheService.SetTemplateDataAsync($"templates:category:{categoryName}", categoryTemplates, TimeSpan.FromMinutes(30));
                    Console.WriteLine($"Cached {categoryTemplates.Count} templates for category: {categoryName}");
                }
            }

            // 3. Cache featured templates
            var featuredTemplates = allTemplates.Where(t => t.IsFeatured).ToList();
            await cacheService.SetTemplateDataAsync("templates:featured", featuredTemplates, TimeSpan.FromHours(1));
            Console.WriteLine($"Cached {featuredTemplates.Count} featured templates");

            // 4. Cache popular templates (usage > 100)
            var popularTemplates = allTemplates.Where(t => t.UsageCount > 100).ToList();
            await cacheService.SetTemplateDataAsync("templates:popular", popularTemplates, TimeSpan.FromHours(2));
            Console.WriteLine($"Cached {popularTemplates.Count} popular templates");

            Console.WriteLine("Template cache warmup completed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cache warmup failed: {ex.Message}");
        }
    }

    public async Task RefreshPopularTemplatesAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
            var mapRepository = scope.ServiceProvider.GetRequiredService<IMapRepository>();

            // Refresh popular templates every 24 hours
            var allTemplates = await mapRepository.GetMapTemplates();
            var popularTemplates = allTemplates.Where(t => t.UsageCount > 100).ToList();

            await cacheService.SetTemplateDataAsync("templates:popular", popularTemplates, TimeSpan.FromHours(24));
            Console.WriteLine($"Refreshed {popularTemplates.Count} popular templates");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to refresh popular templates: {ex.Message}");
        }
    }

    public async Task ClearTemplateCacheAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

            await cacheService.InvalidateTemplateCacheAsync();
            Console.WriteLine("Template cache cleared");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to clear template cache: {ex.Message}");
        }
    }
}
