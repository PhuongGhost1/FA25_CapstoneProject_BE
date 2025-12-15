using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CusomMapOSM_Domain.Entities.Timeline;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.TimelineConfig;

public class RouteAnimationConfiguration : IEntityTypeConfiguration<RouteAnimation>
{
    public void Configure(EntityTypeBuilder<RouteAnimation> builder)
    {
        builder.ToTable("route_animations");

        builder.HasKey(ra => ra.RouteAnimationId);
        builder.Property(ra => ra.RouteAnimationId)
            .HasColumnName("route_animation_id")
            .IsRequired();

        builder.Property(ra => ra.SegmentId)
            .HasColumnName("segment_id")
            .IsRequired();

        builder.Property(ra => ra.MapId)
            .HasColumnName("map_id")
            .IsRequired();

        // From location
        builder.Property(ra => ra.FromLat)
            .HasColumnName("from_lat")
            .IsRequired();

        builder.Property(ra => ra.FromLng)
            .HasColumnName("from_lng")
            .IsRequired();

        builder.Property(ra => ra.FromName)
            .HasColumnName("from_name")
            .HasMaxLength(255);

        // To location
        builder.Property(ra => ra.ToLat)
            .HasColumnName("to_lat")
            .IsRequired();

        builder.Property(ra => ra.ToLng)
            .HasColumnName("to_lng")
            .IsRequired();

        builder.Property(ra => ra.ToName)
            .HasColumnName("to_name")
            .HasMaxLength(255);

        builder.Property(ra => ra.ToLocationId)
            .HasColumnName("to_location_id");

        // Route path (GeoJSON LineString)
        builder.Property(ra => ra.RoutePath)
            .HasColumnName("route_path")
            .HasColumnType("TEXT")
            .IsRequired();

        // Waypoints for multi-point routes
        builder.Property(ra => ra.Waypoints)
            .HasColumnName("waypoints")
            .HasColumnType("TEXT");

        // Icon configuration
        builder.Property(ra => ra.IconType)
            .HasColumnName("icon_type")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue("car");

        builder.Property(ra => ra.IconUrl)
            .HasColumnName("icon_url")
            .HasMaxLength(500);

        builder.Property(ra => ra.IconWidth)
            .HasColumnName("icon_width")
            .IsRequired()
            .HasDefaultValue(32);

        builder.Property(ra => ra.IconHeight)
            .HasColumnName("icon_height")
            .IsRequired()
            .HasDefaultValue(32);

        // Route styling
        builder.Property(ra => ra.RouteColor)
            .HasColumnName("route_color")
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("#666666");

        builder.Property(ra => ra.VisitedColor)
            .HasColumnName("visited_color")
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("#3b82f6");

        builder.Property(ra => ra.RouteWidth)
            .HasColumnName("route_width")
            .IsRequired()
            .HasDefaultValue(4);

        // Animation settings
        builder.Property(ra => ra.DurationMs)
            .HasColumnName("duration_ms")
            .IsRequired()
            .HasDefaultValue(5000);

        builder.Property(ra => ra.StartDelayMs)
            .HasColumnName("start_delay_ms");

        builder.Property(ra => ra.Easing)
            .HasColumnName("easing")
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("linear");

        builder.Property(ra => ra.AutoPlay)
            .HasColumnName("auto_play")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ra => ra.Loop)
            .HasColumnName("loop")
            .IsRequired()
            .HasDefaultValue(false);

        // Display settings
        builder.Property(ra => ra.IsVisible)
            .HasColumnName("is_visible")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ra => ra.ZIndex)
            .HasColumnName("z_index")
            .IsRequired()
            .HasDefaultValue(1000);

        builder.Property(ra => ra.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired()
            .HasDefaultValue(0);

        // Timing
        builder.Property(ra => ra.StartTimeMs)
            .HasColumnName("start_time_ms");

        builder.Property(ra => ra.EndTimeMs)
            .HasColumnName("end_time_ms");

        // Camera state transitions
        builder.Property(ra => ra.CameraStateBefore)
            .HasColumnName("camera_state_before")
            .HasColumnType("TEXT");

        builder.Property(ra => ra.CameraStateAfter)
            .HasColumnName("camera_state_after")
            .HasColumnType("TEXT");

        // Location info display settings
        builder.Property(ra => ra.ShowLocationInfoOnArrival)
            .HasColumnName("show_location_info_on_arrival")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ra => ra.LocationInfoDisplayDurationMs)
            .HasColumnName("location_info_display_duration_ms");
        
        builder.Property(ra => ra.FollowCamera)
            .HasColumnName("follow_camera")
            .HasColumnType("TEXT");
        
        builder.Property(ra => ra.FollowCameraZoom)
            .HasColumnName("follow_camera_zoom")
            .HasColumnType("TEXT");

        // Timestamps
        builder.Property(ra => ra.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(ra => ra.UpdatedAt)
            .HasColumnName("updated_at");

        // Relationships
        builder.HasOne(ra => ra.Map)
            .WithMany()
            .HasForeignKey(ra => ra.MapId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ra => ra.Segment)
            .WithMany()
            .HasForeignKey(ra => ra.SegmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Optional relationship to Location at destination
        // Note: Location is in different namespace, so we configure via foreign key only
        builder.HasIndex(ra => ra.ToLocationId)
            .HasDatabaseName("IX_route_animations_to_location_id");
     }
}

