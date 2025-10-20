using System;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CusomMapOSM_Infrastructure.Services.LayerData.Mongo;

[BsonIgnoreExtraElements]
internal class LayerDataBsonDocument
{
    [BsonId]
    [BsonIgnoreIfDefault]
    public string Id { get; set; } = string.Empty;

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid LayerId { get; set; }
    
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid MapId { get; set; }
    
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid UserId { get; set; }
    
    [BsonElement("data")]
    [BsonIgnoreIfNull]
    public BsonValue? Data { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    private static BsonValue ConvertToBsonValue(object value)
    {
        return value switch
        {
            JsonElement jsonElement => jsonElement.ValueKind switch
            {
                JsonValueKind.String => BsonValue.Create(jsonElement.GetString()),
                JsonValueKind.Number => BsonValue.Create(jsonElement.GetDouble()),
                JsonValueKind.True => BsonValue.Create(true),
                JsonValueKind.False => BsonValue.Create(false),
                JsonValueKind.Null => BsonNull.Value,
                JsonValueKind.Array => new BsonArray(jsonElement.EnumerateArray().Select(item => ConvertToBsonValue((object)item))),
                JsonValueKind.Object => new BsonDocument(jsonElement.EnumerateObject().Select(prop => 
                    new BsonElement(prop.Name, ConvertToBsonValue((object)prop.Value)))),
                _ => BsonValue.Create(jsonElement.ToString())
            },
            string str when IsJsonString(str) => ParseJsonToBsonValue(str),
            _ => BsonValue.Create(value)
        };
    }

    private static bool IsJsonString(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return false;
            
        str = str.Trim();
        return (str.StartsWith("[") && str.EndsWith("]")) || 
               (str.StartsWith("{") && str.EndsWith("}"));
    }

    private static BsonValue ParseJsonToBsonValue(string jsonString)
    {
        try
        {
            // Try to parse as JSON and convert to BsonValue
            using var document = JsonDocument.Parse(jsonString);
            return ConvertJsonElementToBsonValue(document.RootElement);
        }
        catch (JsonException)
        {
            // If parsing fails, return as string
            return BsonValue.Create(jsonString);
        }
    }

    private static BsonValue ConvertJsonElementToBsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => BsonValue.Create(element.GetString()),
            JsonValueKind.Number => element.TryGetDouble(out var d) ? BsonValue.Create(d) : BsonValue.Create(element.GetInt32()),
            JsonValueKind.True => BsonValue.Create(true),
            JsonValueKind.False => BsonValue.Create(false),
            JsonValueKind.Null => BsonNull.Value,
            JsonValueKind.Array => new BsonArray(element.EnumerateArray().Select(ConvertJsonElementToBsonValue)),
            JsonValueKind.Object => new BsonDocument(element.EnumerateObject().Select(prop => 
                new BsonElement(prop.Name, ConvertJsonElementToBsonValue(prop.Value)))),
            _ => BsonValue.Create(element.ToString())
        };
    }

    public static LayerDataBsonDocument FromData(string id, Guid layerId, Guid mapId, Guid userId, object data)
    {
        return new LayerDataBsonDocument
        {
            Id = id,
            LayerId = layerId,
            MapId = mapId,
            UserId = userId,
            Data = data != null ? ConvertToBsonValue(data) : null,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public string GetDataAsString()
    {
        return Data?.ToJson() ?? string.Empty;
    }
}

