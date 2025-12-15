using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Domain.Entities.Animations;

namespace CusomMapOSM_Application.Common.Mappers;

public static class AnimatedLayerPresetMappings
{
    public static AnimatedLayerPresetDto ToDto(this AnimatedLayerPreset preset)
        => new AnimatedLayerPresetDto(
            preset.AnimatedLayerPresetId,
            preset.CreatedBy,
            preset.Name,
            preset.Description,
            preset.Category,
            preset.Tags,
            preset.MediaType,
            preset.SourceUrl,
            preset.ThumbnailUrl,
            preset.DefaultCoordinates,
            preset.DefaultIsScreenOverlay,
            preset.DefaultScreenPosition,
            preset.DefaultScale,
            preset.DefaultOpacity,
            preset.DefaultAutoPlay,
            preset.DefaultLoop,
            preset.IsSystemPreset,
            preset.IsPublic,
            preset.UsageCount,
            preset.IsActive,
            preset.CreatedAt,
            preset.UpdatedAt);
}
