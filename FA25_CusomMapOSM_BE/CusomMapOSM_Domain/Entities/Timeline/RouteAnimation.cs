using System;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Segments;

namespace CusomMapOSM_Domain.Entities.Timeline;

/// <summary>
/// RouteAnimation - Hiển thị animation route trong một segment
/// Khác với TimelineTransition (chuyển giữa segments)
/// </summary>
public class RouteAnimation
{
    public Guid RouteAnimationId { get; set; }
    public Guid SegmentId { get; set; }
    public Guid MapId { get; set; }
    
    // Route information
    public double FromLat { get; set; }
    public double FromLng { get; set; }
    public string? FromName { get; set; }
    
    public double ToLat { get; set; }
    public double ToLng { get; set; }
    public string? ToName { get; set; }
    
    // Link to Location at destination point (for showing info after route completes)
    public Guid? ToLocationId { get; set; }
    
    // Route path as GeoJSON LineString
    public string RoutePath { get; set; } = string.Empty; // GeoJSON LineString
    
    // Waypoints for multi-point routes (JSON array of {locationId, lat, lng, name, segmentId})
    // Format: [{"locationId":"guid","lat":0.0,"lng":0.0,"name":"Location Name","segmentId":"guid"},...]
    // If null or empty, uses From/To locations (backward compatible)
    public string? Waypoints { get; set; }
    
    // Icon configuration
    public string IconType { get; set; } = "car"; // car, walking, bike, plane, custom
    public string? IconUrl { get; set; }
    public int IconWidth { get; set; } = 32;
    public int IconHeight { get; set; } = 32;
    
    // Route styling
    public string RouteColor { get; set; } = "#666666"; // Color for unvisited route
    public string VisitedColor { get; set; } = "#3b82f6"; // Color for visited route
    public int RouteWidth { get; set; } = 4;
    
    // Animation settings
    public int DurationMs { get; set; } = 5000;
    public int? StartDelayMs { get; set; }
    public string Easing { get; set; } = "linear"; // linear, ease-in, ease-out, ease-in-out
    public bool AutoPlay { get; set; } = true;
    public bool Loop { get; set; } = false;
    
    // Display settings
    public bool IsVisible { get; set; } = true;
    public int ZIndex { get; set; } = 1000;
    public int DisplayOrder { get; set; } = 0;
    
    // Timing relative to segment (optional)
    public int? StartTimeMs { get; set; } // When to start in segment timeline
    public int? EndTimeMs { get; set; } // When to end in segment timeline
    
    // Camera state transitions (JSON stringified CameraState)
    // CameraStateBefore: Camera state before route starts (e.g., zoom out to show overview)
    public string? CameraStateBefore { get; set; }
    // CameraStateAfter: Camera state after route completes (e.g., zoom in to destination)
    public string? CameraStateAfter { get; set; }
    
    // Location info display settings
    public bool ShowLocationInfoOnArrival { get; set; } = true; // Auto-show location popup when route completes
    public int? LocationInfoDisplayDurationMs { get; set; } // Duration to show location info popup (null = until user closes)
    
    // Camera follow settings
    public bool FollowCamera { get; set; } = true; // Whether camera should follow the moving icon
    public int? FollowCameraZoom { get; set; } // Zoom level when following (null = keep current zoom)
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public Map? Map { get; set; }
    public Segment? Segment { get; set; }
}

