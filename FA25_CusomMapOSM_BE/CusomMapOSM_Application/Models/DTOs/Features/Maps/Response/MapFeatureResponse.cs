using System;
using CusomMapOSM_Domain.Entities.Maps.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;

public class MapFeatureResponse
{
    public Guid FeatureId { get; set; }
    public Guid MapId { get; set; }
    public Guid? LayerId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public FeatureCategoryEnum FeatureCategory { get; set; }
    public AnnotationTypeEnum? AnnotationType { get; set; }
    public GeometryTypeEnum GeometryType { get; set; }
    public string Coordinates { get; set; } = string.Empty;
    public string? Properties { get; set; }
    public string? Style { get; set; }
    // Individual feature styling properties
    public string? FeatureStyle { get; set; }
    public bool UseIndividualStyle { get; set; }
    public bool IsVisible { get; set; }
    public int ZIndex { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}


