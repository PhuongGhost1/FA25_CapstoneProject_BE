using MongoDB.Driver;

namespace CusomMapOSM_Infrastructure.Services.UserAssets.Mongo;

public class MongoUserAssetStore
{
    private readonly IMongoCollection<UserAssetBsonDocument> _collection;

    public MongoUserAssetStore(IMongoDatabase database)
    {
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

    public async Task<(List<UserAssetBsonDocument> Items, int TotalCount)> GetAssetsAsync(
        Guid userId, 
        Guid? orgId = null, 
        string? type = null, 
        int page = 1, 
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var builder = Builders<UserAssetBsonDocument>.Filter;
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
        
        var totalCount = await _collection.CountDocumentsAsync(finalFilter, cancellationToken: ct);

        // Get paginated data
        var items = await _collection
            .Find(finalFilter)
            .SortByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        return (items, (int)totalCount);
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