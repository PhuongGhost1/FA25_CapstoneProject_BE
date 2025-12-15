using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CusomMapOSM_Application.Interfaces.Services.LayerData;
using CusomMapOSM_Commons.Constant;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Infrastructure.Services.LayerData.Relational;
using MongoDB.Driver;

namespace CusomMapOSM_Infrastructure.Services.LayerData.Mongo;

public class MongoLayerDataStore : ILayerDataStore
{
    private readonly IMongoCollection<LayerDataBsonDocument> _collection;
    private readonly RelationalLayerDataStore _sqlFallback;

    public MongoLayerDataStore(IMongoDatabase database, RelationalLayerDataStore sqlFallback)
    {
        _collection = database.GetCollection<LayerDataBsonDocument>(MongoDatabaseConstant.LayerDataCollectionName);
        _sqlFallback = sqlFallback;
    }

    public async Task<string?> GetDataAsync(Layer layer, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(layer.DataStoreKey))
        {
            var filter = Builders<LayerDataBsonDocument>.Filter.Eq(d => d.Id, layer.DataStoreKey);
            var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
            if (document != null)
            {
                return document.GetDataAsString();
            }
        }

        return await _sqlFallback.GetDataAsync(layer, cancellationToken);
    }

    public async Task<object?> GetDataObjectAsync(Layer layer, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(layer.DataStoreKey))
        {
            var filter = Builders<LayerDataBsonDocument>.Filter.Eq(d => d.Id, layer.DataStoreKey);
            var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
            if (document != null)
            {
                return document.Data;
            }
        }

        // Fallback to string data and convert to object if needed
        var stringData = await _sqlFallback.GetDataAsync(layer, cancellationToken);
        return stringData;
    }

    public async Task SetDataAsync(Layer layer, string data, CancellationToken cancellationToken = default)
    {
        await SetDataAsync(layer, (object)data, cancellationToken);
    }

    public async Task SetDataAsync(Layer layer, object data, CancellationToken cancellationToken = default)
    {
        if (layer == null) throw new ArgumentNullException(nameof(layer));
        if (data == null) throw new ArgumentNullException(nameof(data));

        var documentId = layer.DataStoreKey;
        if (string.IsNullOrEmpty(documentId))
        {
            documentId = Guid.NewGuid().ToString("N");
            layer.DataStoreKey = documentId;
        }

        var document = LayerDataBsonDocument.FromData(documentId, layer.LayerId, layer.MapId, layer.UserId, data);

        var filter = Builders<LayerDataBsonDocument>.Filter.Eq(d => d.Id, documentId);
        await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);

        layer.LayerData = null;
        
        // Calculate data size based on the string representation
        var dataString = document.GetDataAsString();
        layer.DataSizeKB = Math.Round(Encoding.UTF8.GetByteCount(dataString) / 1024d, 2);
    }

    public async Task DeleteDataAsync(Layer layer, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(layer.DataStoreKey))
        {
            var filter = Builders<LayerDataBsonDocument>.Filter.Eq(d => d.Id, layer.DataStoreKey);
            await _collection.DeleteOneAsync(filter, cancellationToken);
            layer.DataStoreKey = null;
        }

        await _sqlFallback.DeleteDataAsync(layer, cancellationToken);
    }
}

