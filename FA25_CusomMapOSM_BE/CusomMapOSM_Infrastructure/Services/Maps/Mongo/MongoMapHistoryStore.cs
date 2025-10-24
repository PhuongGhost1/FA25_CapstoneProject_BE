using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CusomMapOSM_Application.Interfaces.Services.Maps;
using CusomMapOSM_Commons.Constant;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using MongoDB.Driver;

namespace CusomMapOSM_Infrastructure.Services.Maps.Mongo;

public class MongoMapHistoryStore : IMapHistoryStore
{
    private readonly IMongoCollection<MapHistoryBsonDocument> _collection;
    private readonly IMapHistoryRepository? _sqlRepository;

    public MongoMapHistoryStore(IMongoDatabase database, IMapHistoryRepository? sqlRepository = null)
    {
        _collection = database.GetCollection<MapHistoryBsonDocument>(
            MongoDatabaseConstant.MapHistoryCollectionName);
        _sqlRepository = sqlRepository;
        EnsureIndexesAsync().Wait();
    }

    private async Task EnsureIndexesAsync()
    {
        try
        {
            var mapIndex = Builders<MapHistoryBsonDocument>.IndexKeys
                .Ascending(h => h.MapId)
                .Descending(h => h.CreatedAt);
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MapHistoryBsonDocument>(mapIndex));
        }
        catch
        {
            // best-effort indexing
        }
    }

    public async Task AddAsync(MapHistory history, CancellationToken ct = default)
    {
        if (history == null) throw new ArgumentNullException(nameof(history));

        var latestDoc = await _collection
            .Find(h => h.MapId == history.MapId)
            .SortByDescending(h => h.Version)
            .FirstOrDefaultAsync(ct);

        var nextVersion = (latestDoc?.Version ?? 0) + 1;
        history.VersionId = nextVersion;

        var document = MapHistoryBsonDocument.FromDomain(history, nextVersion);
        await _collection.InsertOneAsync(document, cancellationToken: ct);

        if (_sqlRepository != null)
        {
            // Clone entity so SQL still receives snapshot data for now.
            var sqlHistory = new MapHistory
            {
                VersionId = history.VersionId,
                MapId = history.MapId,
                UserId = history.UserId,
                SnapshotData = history.SnapshotData,
                CreatedAt = history.CreatedAt
            };
            await _sqlRepository.AddAsync(sqlHistory, ct);
        }
    }

    public async Task<IReadOnlyCollection<MapHistory>> GetLastAsync(Guid mapId, int maxCount, CancellationToken ct = default)
    {
        var docs = await _collection
            .Find(h => h.MapId == mapId)
            .SortByDescending(h => h.CreatedAt)
            .Limit(maxCount)
            .ToListAsync(ct);

        if (docs.Count >= maxCount || _sqlRepository == null)
        {
            return docs.Select(d => d.ToDomain())
                .OrderBy(h => h.CreatedAt)
                .ToList();
        }

        var results = docs.Select(d => d.ToDomain()).ToList();
        var remaining = maxCount - results.Count;
        if (remaining > 0)
        {
            var legacy = await _sqlRepository.GetLastAsync(mapId, remaining, ct);
            foreach (var item in legacy)
            {
                if (results.All(r => r.VersionId != item.VersionId))
                {
                    results.Add(item);
                }
            }
        }

        return results
            .OrderBy(h => h.CreatedAt)
            .ToList();
    }

    public async Task TrimToAsync(Guid mapId, int keepCount, CancellationToken ct = default)
    {
        if (keepCount <= 0)
        {
            await _collection.DeleteManyAsync(
                Builders<MapHistoryBsonDocument>.Filter.Eq(h => h.MapId, mapId),
                ct);
        }
        else
        {
            var toKeep = await _collection
                .Find(h => h.MapId == mapId)
                .SortByDescending(h => h.CreatedAt)
                .Limit(keepCount)
                .Project(h => h.Version)
                .ToListAsync(ct);

            var filter = Builders<MapHistoryBsonDocument>.Filter.And(
                Builders<MapHistoryBsonDocument>.Filter.Eq(h => h.MapId, mapId),
                Builders<MapHistoryBsonDocument>.Filter.Not(
                    Builders<MapHistoryBsonDocument>.Filter.In(h => h.Version, toKeep)));

            await _collection.DeleteManyAsync(filter, ct);
        }

        if (_sqlRepository != null)
        {
            await _sqlRepository.TrimToAsync(mapId, keepCount, ct);
        }
    }

    public async Task<int> DeleteOlderThanAsync(DateTime cutoffUtc, CancellationToken ct = default)
    {
        var filter = Builders<MapHistoryBsonDocument>.Filter.Lt(h => h.CreatedAt, cutoffUtc);
        var result = await _collection.DeleteManyAsync(filter, ct);

        if (_sqlRepository != null)
        {
            await _sqlRepository.DeleteOlderThanAsync(cutoffUtc, ct);
        }

        return (int)result.DeletedCount;
    }
}
