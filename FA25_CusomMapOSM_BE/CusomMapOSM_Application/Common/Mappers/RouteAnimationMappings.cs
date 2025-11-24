using CusomMapOSM_Application.Models.DTOs.Features.StoryMaps;
using CusomMapOSM_Domain.Entities.Timeline;

namespace CusomMapOSM_Application.Common.Mappers;

public static class RouteAnimationMappings
{
    public static RouteAnimationDto ToDto(this RouteAnimation routeAnimation)
    {
        return new RouteAnimationDto(
            RouteAnimationId: routeAnimation.RouteAnimationId,
            SegmentId: routeAnimation.SegmentId,
            MapId: routeAnimation.MapId,
            FromLat: routeAnimation.FromLat,
            FromLng: routeAnimation.FromLng,
            FromName: routeAnimation.FromName,
            ToLat: routeAnimation.ToLat,
            ToLng: routeAnimation.ToLng,
            ToName: routeAnimation.ToName,
            ToLocationId: routeAnimation.ToLocationId,
            RoutePath: routeAnimation.RoutePath,
            Waypoints: routeAnimation.Waypoints,
            IconType: routeAnimation.IconType,
            IconUrl: routeAnimation.IconUrl,
            IconWidth: routeAnimation.IconWidth,
            IconHeight: routeAnimation.IconHeight,
            RouteColor: routeAnimation.RouteColor,
            VisitedColor: routeAnimation.VisitedColor,
            RouteWidth: routeAnimation.RouteWidth,
            DurationMs: routeAnimation.DurationMs,
            StartDelayMs: routeAnimation.StartDelayMs,
            Easing: routeAnimation.Easing,
            AutoPlay: routeAnimation.AutoPlay,
            Loop: routeAnimation.Loop,
            IsVisible: routeAnimation.IsVisible,
            ZIndex: routeAnimation.ZIndex,
            DisplayOrder: routeAnimation.DisplayOrder,
            StartTimeMs: routeAnimation.StartTimeMs,
            EndTimeMs: routeAnimation.EndTimeMs,
            CameraStateBefore: routeAnimation.CameraStateBefore,
            CameraStateAfter: routeAnimation.CameraStateAfter,
            ShowLocationInfoOnArrival: routeAnimation.ShowLocationInfoOnArrival,
            LocationInfoDisplayDurationMs: routeAnimation.LocationInfoDisplayDurationMs,
            CreatedAt: routeAnimation.CreatedAt,
            UpdatedAt: routeAnimation.UpdatedAt
        );
    }
}

