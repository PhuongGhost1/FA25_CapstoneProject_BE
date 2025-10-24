using System;
using System.Text.Json;
using CusomMapOSM_Domain.Entities.Maps;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CusomMapOSM_Infrastructure.Services.Maps.Mongo;

[BsonIgnoreExtraElements]
internal class MapHistoryBsonDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("mapId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid MapId { get; set; }

    [BsonElement("userId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid UserId { get; set; }

    [BsonElement("snapshot")]
    [BsonIgnoreIfNull]
    public BsonValue? Snapshot { get; set; }

    [BsonElement("version")]
    public int Version { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    public static MapHistoryBsonDocument FromDomain(MapHistory history, int version)
    {
        if (history == null) throw new ArgumentNullException(nameof(history));

        return new MapHistoryBsonDocument
        {
            MapId = history.MapId,
            UserId = history.UserId,
            Snapshot = ConvertSnapshot(history.SnapshotData),
            Version = version,
            CreatedAt = history.CreatedAt
        };
    }

    public MapHistory ToDomain()
    {
        return new MapHistory
        {
            VersionId = Version,
            MapId = MapId,
            UserId = UserId,
            SnapshotData = Snapshot != null ? ConvertSnapshotToString(Snapshot) : string.Empty,
            CreatedAt = CreatedAt
        };
    }

    private static BsonValue ConvertSnapshot(string snapshot)
    {
        if (string.IsNullOrWhiteSpace(snapshot))
        {
            return BsonNull.Value;
        }

        try
        {
            using var document = JsonDocument.Parse(snapshot);
            return ConvertJsonElement(document.RootElement);
        }
        catch (JsonException)
        {
            try
            {
                return BsonDocument.Parse(snapshot);
            }
            catch (FormatException)
            {
                return BsonValue.Create(snapshot);
            }
        }
    }

    private static BsonValue ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => new BsonDocument(
                element.EnumerateObject()
                    .Select(p => new BsonElement(p.Name, ConvertJsonElement(p.Value)))),
            JsonValueKind.Array => new BsonArray(
                element.EnumerateArray()
                    .Select(ConvertJsonElement)),
            JsonValueKind.String => BsonValue.Create(element.GetString()),
            JsonValueKind.Number => element.TryGetInt64(out var l)
                ? BsonValue.Create(l)
                : BsonValue.Create(element.GetDouble()),
            JsonValueKind.True => BsonBoolean.True,
            JsonValueKind.False => BsonBoolean.False,
            JsonValueKind.Null => BsonNull.Value,
            _ => BsonValue.Create(element.ToString())
        };
    }

    private static string ConvertSnapshotToString(BsonValue snapshot)
    {
        return snapshot switch
        {
            BsonDocument doc => doc.ToJson(),
            BsonArray arr => arr.ToJson(),
            BsonNull => string.Empty,
            _ => snapshot.ToString()
        };
    }
}
