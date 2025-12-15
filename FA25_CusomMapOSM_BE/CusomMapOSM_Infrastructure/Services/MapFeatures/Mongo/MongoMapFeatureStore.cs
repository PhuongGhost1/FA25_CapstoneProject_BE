using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CusomMapOSM_Application.Interfaces.Services.MapFeatures;
using CusomMapOSM_Application.Models.Documents;
using CusomMapOSM_Commons.Constant;
using MongoDB.Driver;

namespace CusomMapOSM_Infrastructure.Services.MapFeatures.Mongo;

public class MongoMapFeatureStore : IMapFeatureStore
{
    private readonly IMongoCollection<MapFeatureBsonDocument> _collection;

    public MongoMapFeatureStore(IMongoDatabase database)
    {
        _collection = database.GetCollection<MapFeatureBsonDocument>(
            MongoDatabaseConstant.MapFeatureCollectionName
        );
        
        CreateIndexesAsync().Wait();
    }

    private async Task CreateIndexesAsync()
    {
        try
        {
            var mapIdIndex = Builders<MapFeatureBsonDocument>.IndexKeys.Ascending(x => x.MapId);
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MapFeatureBsonDocument>(mapIdIndex)
            );

            var mapLayerIndex = Builders<MapFeatureBsonDocument>.IndexKeys
                .Ascending(x => x.MapId)
                .Ascending(x => x.LayerId);
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MapFeatureBsonDocument>(mapLayerIndex)
            );

            var geoIndex = Builders<MapFeatureBsonDocument>.IndexKeys.Geo2DSphere(x => x.Geometry);
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MapFeatureBsonDocument>(geoIndex)
            );
        }
        catch
        {
        }
    }

    public async Task<MapFeatureDocument?> GetAsync(Guid featureId, CancellationToken ct = default)
    {
        var filter = Builders<MapFeatureBsonDocument>.Filter.Eq(x => x.Id, featureId.ToString());
        var doc = await _collection.Find(filter).FirstOrDefaultAsync(ct);
        return doc?.ToApplicationModel();
    }

    public async Task<List<MapFeatureDocument>> GetByMapAsync(Guid mapId, CancellationToken ct = default)
    {
        var filter = Builders<MapFeatureBsonDocument>.Filter.Eq(x => x.MapId, mapId);
        var docs = await _collection.Find(filter).ToListAsync(ct);
        return docs.Select(d => d.ToApplicationModel()).ToList();
    }

    public async Task<List<MapFeatureDocument>> GetByLayerAsync(Guid mapId, Guid layerId, CancellationToken ct = default)
    {
        var filter = Builders<MapFeatureBsonDocument>.Filter.And(
            Builders<MapFeatureBsonDocument>.Filter.Eq(x => x.MapId, mapId),
            Builders<MapFeatureBsonDocument>.Filter.Eq(x => x.LayerId, layerId)
        );
        var docs = await _collection.Find(filter).ToListAsync(ct);
        return docs.Select(d => d.ToApplicationModel()).ToList();
    }

    public async Task<List<MapFeatureDocument>> GetByCategoryAsync(Guid mapId, string category, CancellationToken ct = default)
    {
        var filter = Builders<MapFeatureBsonDocument>.Filter.And(
            Builders<MapFeatureBsonDocument>.Filter.Eq(x => x.MapId, mapId),
            Builders<MapFeatureBsonDocument>.Filter.Eq(x => x.FeatureCategory, category)
        );
        var docs = await _collection.Find(filter).ToListAsync(ct);
        return docs.Select(d => d.ToApplicationModel()).ToList();
    }

    public async Task<List<MapFeatureDocument>> GetByBoundsAsync(Guid mapId, double[] bbox, CancellationToken ct = default)
    {
        if (bbox.Length != 4)
            throw new ArgumentException("Bounding box must have 4 coordinates [minLon, minLat, maxLon, maxLat]");

        var filter = Builders<MapFeatureBsonDocument>.Filter.And(
            Builders<MapFeatureBsonDocument>.Filter.Eq(x => x.MapId, mapId),
            Builders<MapFeatureBsonDocument>.Filter.GeoWithinBox(
                x => x.Geometry, 
                bbox[0], bbox[1],
                bbox[2], bbox[3]
            )
        );

        var docs = await _collection.Find(filter).ToListAsync(ct);
        return docs.Select(d => d.ToApplicationModel()).ToList();
    }

    public async Task<string> CreateAsync(MapFeatureDocument document, CancellationToken ct = default)
    {
        var bsonDoc = MapFeatureBsonDocument.FromApplicationModel(document);
        await _collection.InsertOneAsync(bsonDoc, cancellationToken: ct);
        return bsonDoc.Id;
    }

    public async Task UpdateAsync(MapFeatureDocument document, CancellationToken ct = default)
    {
        var bsonDoc = MapFeatureBsonDocument.FromApplicationModel(document);
        var filter = Builders<MapFeatureBsonDocument>.Filter.Eq(x => x.Id, document.Id);
        await _collection.ReplaceOneAsync(filter, bsonDoc, cancellationToken: ct);
    }

    public async Task DeleteAsync(Guid featureId, CancellationToken ct = default)
    {
        var filter = Builders<MapFeatureBsonDocument>.Filter.Eq(x => x.Id, featureId.ToString());
        await _collection.DeleteOneAsync(filter, ct);
    }

    public async Task DeleteByMapAsync(Guid mapId, CancellationToken ct = default)
    {
        var filter = Builders<MapFeatureBsonDocument>.Filter.Eq(x => x.MapId, mapId);
        await _collection.DeleteManyAsync(filter, ct);
    }
}

