using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CusomMapOSM_Infrastructure.Services.UserAssets.Mongo;

public class UserAssetBsonDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }
    
    [BsonRepresentation(BsonType.String)]
    public Guid? OrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "image" or "audio"
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }

    [BsonIgnore]
    public bool IsImage => Type == "image";
}
