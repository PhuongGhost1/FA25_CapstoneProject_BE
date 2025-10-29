using System;
using System.Linq;
using System.Text.Json;
using CusomMapOSM_Application.Models.Documents;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CusomMapOSM_Infrastructure.Services.MapFeatures.Mongo;

[BsonIgnoreExtraElements]
internal class MapFeatureBsonDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = string.Empty;

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid MapId { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    [BsonIgnoreIfNull]
    public Guid? LayerId { get; set; }

    [BsonIgnoreIfNull]
    public string? Name { get; set; }

    public string FeatureCategory { get; set; } = string.Empty;

    [BsonIgnoreIfNull]
    public string? AnnotationType { get; set; }

    public string GeometryType { get; set; } = string.Empty;

    [BsonElement("geometry")]
    [BsonIgnoreIfNull]
    public BsonValue? Geometry { get; set; }

    [BsonElement("properties")]
    [BsonIgnoreIfNull]
    public BsonDocument? Properties { get; set; }

    [BsonElement("style")]
    [BsonIgnoreIfNull]
    public BsonDocument? Style { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonIgnoreIfNull]
    public DateTime? UpdatedAt { get; set; }

    public bool IsVisible { get; set; } = true;
    public int ZIndex { get; set; } = 0;

    public MapFeatureDocument ToApplicationModel()
    {
        return new MapFeatureDocument
        {
            Id = Id,
            MapId = MapId,
            LayerId = LayerId,
            Name = Name,
            FeatureCategory = FeatureCategory,
            AnnotationType = AnnotationType,
            GeometryType = GeometryType,
            Geometry = Geometry?.ToJson(),
            Properties = Properties?.ToDictionary(
                e => e.Name,
                e => BsonTypeMapper.MapToDotNetValue(e.Value)
            ),
            Style = Style?.ToDictionary(
                e => e.Name,
                e => BsonTypeMapper.MapToDotNetValue(e.Value)
            ),
            CreatedBy = CreatedBy,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,
            IsVisible = IsVisible,
            ZIndex = ZIndex
        };
    }

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

    private static BsonValue ConvertToGeoJsonBsonValue(object geometry, string geometryType)
    {
        // First convert to BsonValue
        var bsonValue = ConvertToBsonValue(geometry);
        
        // If it's already a BsonDocument with type field, it's likely already GeoJSON
        if (bsonValue is BsonDocument doc && doc.Contains("type"))
        {
            return bsonValue;
        }
        
        // If it's a BsonArray, wrap it in proper GeoJSON format
        if (bsonValue is BsonArray coordinates)
        {
            var geoJson = new BsonDocument
            {
                { "type", geometryType },
                { "coordinates", coordinates }
            };
            return geoJson;
        }
        
        // Fallback to original value
        return bsonValue;
    }

    public static MapFeatureBsonDocument FromApplicationModel(MapFeatureDocument doc)
    {
        return new MapFeatureBsonDocument
        {
            Id = string.IsNullOrEmpty(doc.Id) ? Guid.NewGuid().ToString() : doc.Id,
            MapId = doc.MapId,
            LayerId = doc.LayerId,
            Name = doc.Name,
            FeatureCategory = doc.FeatureCategory,
            AnnotationType = doc.AnnotationType,
            GeometryType = doc.GeometryType,
            Geometry = doc.Geometry != null
                ? ConvertToGeoJsonBsonValue(doc.Geometry, doc.GeometryType)
                : null,
            Properties = doc.Properties != null
                ? new BsonDocument(doc.Properties.Select(kvp =>
                    new BsonElement(kvp.Key, ConvertToBsonValue(kvp.Value))))
                : null,
            Style = doc.Style != null
                ? new BsonDocument(doc.Style.Select(kvp =>
                    new BsonElement(kvp.Key, ConvertToBsonValue(kvp.Value))))
                : null,
            CreatedBy = doc.CreatedBy,
            CreatedAt = doc.CreatedAt,
            UpdatedAt = doc.UpdatedAt,
            IsVisible = doc.IsVisible,
            ZIndex = doc.ZIndex
        };
    }
}

