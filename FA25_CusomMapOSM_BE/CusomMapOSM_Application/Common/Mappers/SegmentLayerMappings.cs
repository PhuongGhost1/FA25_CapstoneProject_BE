using System;
using System.Collections.Generic;
using CusomMapOSM_Application.Models.DTOs.Features.Maps.Response;
using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Domain.Entities.Segments;

namespace CusomMapOSM_Application.Common.Mappers;

public static class SegmentLayerMappings
{
    public static SegmentLayerDto ToDto(
        this SegmentLayer segmentLayer,
        LayerDTO? layer = null,
        IReadOnlyCollection<MapFeatureResponse>? mapFeatures = null)
    {
        var resolvedLayer = layer ?? segmentLayer.Layer?.ToLayerDto();
        return new SegmentLayerDto(
            segmentLayer.SegmentLayerId,
            segmentLayer.SegmentId,
            segmentLayer.LayerId,
            segmentLayer.DisplayOrder,
            segmentLayer.IsVisible,
            segmentLayer.Opacity,
            segmentLayer.ZIndex,
            segmentLayer.EntryDelayMs,
            segmentLayer.EntryDurationMs,
            segmentLayer.ExitDelayMs,
            segmentLayer.ExitDurationMs,
            segmentLayer.EntryEffect,
            segmentLayer.ExitEffect,
            segmentLayer.StyleOverride,
            segmentLayer.CreatedAt,
            segmentLayer.UpdatedAt,
            resolvedLayer,
            mapFeatures ?? Array.Empty<MapFeatureResponse>());
    }
}
