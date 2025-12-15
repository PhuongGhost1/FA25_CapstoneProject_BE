using CusomMapOSM_Application.Models.DTOs.Features.Animations;
using CusomMapOSM_Domain.Entities.Animations;

namespace CusomMapOSM_Application.Common.Mappers;

public static class LayerAnimationMapping
{
    public static LayerAnimationDto ToLayerAnimationDto(this AnimatedLayer a)
    {
        return new LayerAnimationDto(
            a.AnimatedLayerId,
            a.LayerId ?? Guid.Empty,
            a.Name,
            a.SourceUrl,
            a.Coordinates,
            a.RotationDeg,
            a.Scale,
            a.ZIndex,
            a.CreatedAt,
            a.UpdatedAt,
            a.IsVisible
        );
    }
    
}