using System;
using CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.Maps.Enums;
namespace CusomMapOSM_Domain.Entities.Maps;

public class MapFeature
{
    public Guid FeatureId { get; set; }
    public Guid MapId { get; set; }
    public Guid? LayerId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public FeatureCategoryEnum FeatureCategory { get; set; } = FeatureCategoryEnum.Data;
    public AnnotationTypeEnum? AnnotationType { get; set; }
    public GeometryTypeEnum GeometryType { get; set; } = GeometryTypeEnum.Point;
    public string? MongoDocumentId { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsVisible { get; set; } = true;
    public int ZIndex { get; set; } = 0;
    
    public Map? Map { get; set; }
    public Layer? Layer { get; set; }
    public User? Creator { get; set; }
}