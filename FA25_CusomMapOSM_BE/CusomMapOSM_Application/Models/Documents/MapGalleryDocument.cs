using System;
using System.Collections.Generic;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace CusomMapOSM_Application.Models.Documents;

[BsonIgnoreExtraElements]
public class MapGalleryDocument
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Id { get; set; } = string.Empty;
    
    [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.Standard)]
    public Guid MapId { get; set; }
    
    [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.Standard)]
    public Guid UserId { get; set; }
    public string MapName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PreviewImage { get; set; }
    public MapTemplateCategoryEnum? Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? AuthorName { get; set; }
    public string? AuthorEmail { get; set; }
    
    // Approval workflow
    public MapGalleryStatusEnum Status { get; set; } = MapGalleryStatusEnum.Pending;
    
    [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.Standard)]
    [BsonIgnoreIfNull]
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    
    // Display metadata
    public bool IsFeatured { get; set; } = false;
    public int ViewCount { get; set; } = 0;
    public int LikeCount { get; set; } = 0;
    
    [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.Standard)]
    [BsonIgnoreIfNull]
    public List<Guid>? LikedByUsers { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MapGalleryStatusEnum
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

