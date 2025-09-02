using System;
using CusomMapOSM_Domain.Entities.Maps.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Request;

public class UpdateMapFeatureRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public FeatureCategoryEnum? FeatureCategory { get; set; }
    public AnnotationTypeEnum? AnnotationType { get; set; }
    public GeometryTypeEnum? GeometryType { get; set; }
    public string? Coordinates { get; set; }
    public string? Properties { get; set; }
    public string? Style { get; set; }
    public bool? IsVisible { get; set; }
    public int? ZIndex { get; set; }
    public Guid? LayerId { get; set; }
}


