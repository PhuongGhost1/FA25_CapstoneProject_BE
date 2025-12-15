using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Domain.Entities.Timeline;

namespace CusomMapOSM_Application.Common.Mappers;

public static class TimelineTransitionMappings
{
    public static TimelineTransitionDto ToDto(this TimelineTransition transition)
        => new TimelineTransitionDto(
            transition.TimelineTransitionId,
            transition.MapId,
            transition.FromSegmentId,
            transition.ToSegmentId,
            transition.TransitionName,
            transition.DurationMs,
            transition.TransitionType,
            transition.AnimateCamera,
            transition.CameraAnimationType,
            transition.CameraAnimationDurationMs,
            transition.ShowOverlay,
            transition.OverlayContent,
            transition.AutoTrigger,
            transition.RequireUserAction,
            transition.TriggerButtonText,
            transition.CreatedAt,
            transition.UpdatedAt);
}
