using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CusomMapOSM_Commons.Constant;
using MongoDB.Driver;

namespace CusomMapOSM_Infrastructure.Services.UserAssets.Mongo;

public class MongoUserAssetStore
{
    private readonly IMongoCollection<UserAssetBsonDocument> _collection;

    public MongoUserAssetStore(IMongoDatabase database)
    {
        // We'll reuse the existing Mongo constant or add a new one if needed.
        // For now, let's assume a new collection name "user_assets"
        _collection = database.GetCollection<UserAssetBsonDocument>("user_assets");
        EnsureIndexesAsync().Wait();
    }

    private async Task EnsureIndexesAsync()
    {
        try
        {
            var userIdIndex = Builders<UserAssetBsonDocument>.IndexKeys
                .Ascending(a => a.UserId)
                .Descending(a => a.CreatedAt);
            
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<UserAssetBsonDocument>(userIdIndex));

            var orgIdIndex = Builders<UserAssetBsonDocument>.IndexKeys
                .Ascending(a => a.OrganizationId)
                .Descending(a => a.CreatedAt);
            
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<UserAssetBsonDocument>(orgIdIndex));
        }
        catch
        {
            // best-effort
        }
    }

    public async Task AddAsync(UserAssetBsonDocument asset, CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(asset, cancellationToken: ct);
    }

    public async Task<List<UserAssetBsonDocument>> GetAssetsAsync(Guid userId, Guid? orgId = null, string? type = null, CancellationToken ct = default)
    {
        var builder = Builders<UserAssetBsonDocument>.Filter;
        
        // Filter: (UserId == userId) OR (OrganizationId == orgId AND OrganizationId != null)
        var ownerFilter = builder.Eq(a => a.UserId, userId);
        if (orgId.HasValue)
        {
            ownerFilter |= builder.Eq(a => a.OrganizationId, orgId.Value);
        }

        var finalFilter = ownerFilter;

        if (!string.IsNullOrEmpty(type))
        {
            finalFilter &= builder.Eq(a => a.Type, type);
        }

        return await _collection
            .Find(finalFilter)
            .SortByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<UserAssetBsonDocument?> GetByIdAsync(Guid assetId, CancellationToken ct = default)
    {
        return await _collection
            .Find(a => a.Id == assetId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task DeleteAsync(Guid assetId, CancellationToken ct = default)
    {
        await _collection.DeleteOneAsync(a => a.Id == assetId, ct);
    }
}
