using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CusomMapOSM_Application.Interfaces.Services.StoryMaps;
using CusomMapOSM_Commons.Constant;
using CusomMapOSM_Domain.Entities.Locations;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Microsoft.Extensions.DependencyInjection;
using CusomMapOSM_Infrastructure.Databases;

namespace CusomMapOSM_Infrastructure.Services.StoryMaps.Mongo;

public class MongoSegmentLocationStore : ISegmentLocationStore
{
    private readonly IMongoCollection<SegmentLocationBsonDocument> _collection;
    private readonly IServiceScopeFactory? _scopeFactory;

    public MongoSegmentLocationStore(
        IMongoDatabase database,
        IServiceScopeFactory? scopeFactory = null)
    {
        _collection = database.GetCollection<SegmentLocationBsonDocument>(
            MongoDatabaseConstant.LocationCollectionName);
        _scopeFactory = scopeFactory;
        EnsureIndexesAsync().Wait();
    }

    private async Task EnsureIndexesAsync()
    {
        try
        {
            var mapIndex = Builders<SegmentLocationBsonDocument>.IndexKeys
                .Ascending(l => l.MapId)
                .Ascending(l => l.DisplayOrder);
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<SegmentLocationBsonDocument>(mapIndex));

            var segmentIndex = Builders<SegmentLocationBsonDocument>.IndexKeys
                .Ascending(l => l.SegmentId)
                .Ascending(l => l.DisplayOrder);
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<SegmentLocationBsonDocument>(segmentIndex));
        }
        catch
        {
            // best-effort indexing only
        }
    }

    public async Task<Location?> GetAsync(Guid locationId, CancellationToken ct = default)
    {
        var doc = await _collection
            .Find(l => l.Id == locationId.ToString())
            .FirstOrDefaultAsync(ct);

        if (doc != null)
        {
            return doc.ToDomain();
        }

        if (_scopeFactory != null)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();
            return await context.MapLocations
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.LocationId == locationId, ct);
        }

        return null;
    }

    public async Task<IReadOnlyCollection<Location>> GetByMapAsync(Guid mapId, CancellationToken ct = default)
    {
        var docs = await _collection
            .Find(l => l.MapId == mapId)
            .SortBy(l => l.DisplayOrder)
            .ToListAsync(ct);

        if (docs.Count > 0)
        {
            return docs
                .Select(d => d.ToDomain())
                .OrderBy(l => l.DisplayOrder)
                .ToList();
        }

        if (_scopeFactory != null)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();
            var legacy = await context.MapLocations
                .AsNoTracking()
                .Where(l => l.MapId == mapId)
                .OrderBy(l => l.DisplayOrder)
                .ToListAsync(ct);
            return legacy;
        }

        return Array.Empty<Location>();
    }

    public async Task<IReadOnlyCollection<Location>> GetBySegmentAsync(Guid segmentId, CancellationToken ct = default)
    {
        var docs = await _collection
            .Find(l => l.SegmentId == segmentId)
            .SortBy(l => l.DisplayOrder)
            .ToListAsync(ct);

        if (docs.Count > 0)
        {
            return docs
                .Select(d => d.ToDomain())
                .OrderBy(l => l.DisplayOrder)
                .ToList();
        }

        if (_scopeFactory != null)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();
            var legacy = await context.MapLocations
                .AsNoTracking()
                .Where(l => l.SegmentId == segmentId)
                .OrderBy(l => l.DisplayOrder)
                .ToListAsync(ct);
            return legacy;
        }

        return Array.Empty<Location>();
    }

    public async Task<Location> CreateAsync(Location location, CancellationToken ct = default)
    {
        if (location == null) throw new ArgumentNullException(nameof(location));

        if (location.LocationId == Guid.Empty)
        {
            location.LocationId = Guid.NewGuid();
        }

        if (location.CreatedAt == default)
        {
            location.CreatedAt = DateTime.UtcNow;
        }

        location.UpdatedAt = location.UpdatedAt ?? location.CreatedAt;

        var document = SegmentLocationBsonDocument.FromDomain(location);
        document.Id = location.LocationId.ToString();

        await _collection.ReplaceOneAsync(
            Builders<SegmentLocationBsonDocument>.Filter.Eq(l => l.Id, document.Id),
            document,
            new ReplaceOptions { IsUpsert = true },
            ct);

        if (_scopeFactory != null)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();
            var clone = Clone(location);
            await context.MapLocations.AddAsync(clone, ct);
            await context.SaveChangesAsync(ct);
        }

        return location;
    }

    public async Task<Location?> UpdateAsync(Location location, CancellationToken ct = default)
    {
        if (location == null) throw new ArgumentNullException(nameof(location));
        if (location.LocationId == Guid.Empty)
        {
            return null;
        }

        location.UpdatedAt ??= DateTime.UtcNow;

        var document = SegmentLocationBsonDocument.FromDomain(location);
        document.Id = location.LocationId.ToString();

        var filter = Builders<SegmentLocationBsonDocument>.Filter.Eq(l => l.Id, document.Id);
        var result = await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = false }, ct);

        if (result.MatchedCount == 0)
        {
            // fallback: document missing in Mongo, recreate from current state
            await CreateAsync(location, ct);
            return location;
        }

        if (_scopeFactory != null)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();
            var tracked = await context.MapLocations.FirstOrDefaultAsync(l => l.LocationId == location.LocationId, ct);
            if (tracked != null)
            {
                CopyValues(location, tracked);
                await context.SaveChangesAsync(ct);
            }
        }

        return location;
    }

    public async Task<bool> DeleteAsync(Guid locationId, CancellationToken ct = default)
    {
        var filter = Builders<SegmentLocationBsonDocument>.Filter.Eq(l => l.Id, locationId.ToString());
        var result = await _collection.DeleteOneAsync(filter, ct);

        if (_scopeFactory != null)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CustomMapOSMDbContext>();
            var entity = await context.MapLocations.FirstOrDefaultAsync(l => l.LocationId == locationId, ct);
            if (entity != null)
            {
                context.MapLocations.Remove(entity);
                await context.SaveChangesAsync(ct);
            }
        }

        return result.DeletedCount > 0;
    }

    private static Location Clone(Location location)
    {
        return new Location
        {
            LocationId = location.LocationId,
            MapId = location.MapId,
            SegmentId = location.SegmentId,
            SegmentZoneId = location.SegmentZoneId,
            Title = location.Title,
            Subtitle = location.Subtitle,
            LocationType = location.LocationType,
            MarkerGeometry = location.MarkerGeometry,
            StoryContent = location.StoryContent,
            MediaResources = location.MediaResources,
            DisplayOrder = location.DisplayOrder,
            HighlightOnEnter = location.HighlightOnEnter,
            ShowTooltip = location.ShowTooltip,
            TooltipContent = location.TooltipContent,
            EffectType = location.EffectType,
            OpenSlideOnClick = location.OpenSlideOnClick,
            SlideContent = location.SlideContent,
            LinkedLocationId = location.LinkedLocationId,
            PlayAudioOnClick = location.PlayAudioOnClick,
            AudioUrl = location.AudioUrl,
            ExternalUrl = location.ExternalUrl,
            AssociatedLayerId = location.AssociatedLayerId,
            AnimationPresetId = location.AnimationPresetId,
            AnimationOverrides = location.AnimationOverrides,
            CreatedAt = location.CreatedAt,
            UpdatedAt = location.UpdatedAt
        };
    }

    private static void CopyValues(Location source, Location target)
    {
        target.MapId = source.MapId;
        target.SegmentId = source.SegmentId;
        target.SegmentZoneId = source.SegmentZoneId;
        target.Title = source.Title;
        target.Subtitle = source.Subtitle;
        target.LocationType = source.LocationType;
        target.MarkerGeometry = source.MarkerGeometry;
        target.StoryContent = source.StoryContent;
        target.MediaResources = source.MediaResources;
        target.DisplayOrder = source.DisplayOrder;
        target.HighlightOnEnter = source.HighlightOnEnter;
        target.ShowTooltip = source.ShowTooltip;
        target.TooltipContent = source.TooltipContent;
        target.EffectType = source.EffectType;
        target.OpenSlideOnClick = source.OpenSlideOnClick;
        target.SlideContent = source.SlideContent;
        target.LinkedLocationId = source.LinkedLocationId;
        target.PlayAudioOnClick = source.PlayAudioOnClick;
        target.AudioUrl = source.AudioUrl;
        target.ExternalUrl = source.ExternalUrl;
        target.AssociatedLayerId = source.AssociatedLayerId;
        target.AnimationPresetId = source.AnimationPresetId;
        target.AnimationOverrides = source.AnimationOverrides;
        target.CreatedAt = source.CreatedAt;
        target.UpdatedAt = source.UpdatedAt;
    }
}

