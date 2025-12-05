using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Domain.Entities.Animations;

namespace CusomMapOSM_Application.Common.Mappers;

public static class AnimatedLayerMappings
{
    public static AnimatedLayerDto ToDto(this AnimatedLayer layer)
        => new AnimatedLayerDto(
            layer.AnimatedLayerId,
            layer.CreatedBy,
            layer.LayerId,
            layer.SegmentId,
            layer.Name,
            layer.Description,
            layer.DisplayOrder,
            layer.MediaType,
            layer.SourceUrl,
            layer.ThumbnailUrl,
            layer.Coordinates,
            layer.IsScreenOverlay,
            layer.ScreenPosition,
            layer.RotationDeg,
            layer.Scale,
            layer.Opacity,
            layer.ZIndex,
            layer.IsVisible,
            layer.CreatedAt,
            layer.UpdatedAt);
}
