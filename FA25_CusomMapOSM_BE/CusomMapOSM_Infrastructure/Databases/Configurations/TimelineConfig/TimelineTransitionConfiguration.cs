using CusomMapOSM_Domain.Entities.Timeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.TimelineConfig;

internal class TimelineTransitionConfiguration : IEntityTypeConfiguration<TimelineTransition>
{
    public void Configure(EntityTypeBuilder<TimelineTransition> builder)
    {
        builder.ToTable("timeline_transitions");

        builder.HasKey(tt => tt.TimelineTransitionId);

        builder.Property(tt => tt.TimelineTransitionId)
            .HasColumnName("timeline_transition_id")
            .IsRequired();

        builder.Property(tt => tt.MapId)
            .HasColumnName("map_id")
            .IsRequired();

        builder.Property(tt => tt.FromSegmentId)
            .HasColumnName("from_segment_id")
            .IsRequired();

        builder.Property(tt => tt.ToSegmentId)
            .HasColumnName("to_segment_id")
            .IsRequired();

        builder.Property(tt => tt.TransitionName)
            .HasColumnName("transition_name")
            .HasMaxLength(255);

        builder.Property(tt => tt.DurationMs)
            .HasColumnName("duration_ms")
            .IsRequired()
            .HasDefaultValue(1000);

        builder.Property(tt => tt.TransitionType)
            .HasColumnName("transition_type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Camera animation
        builder.Property(tt => tt.AnimateCamera)
            .HasColumnName("animate_camera")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(tt => tt.CameraAnimationType)
            .HasColumnName("camera_animation_type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(tt => tt.CameraAnimationDurationMs)
            .HasColumnName("camera_animation_duration_ms")
            .IsRequired()
            .HasDefaultValue(1000);

        // Overlay
        builder.Property(tt => tt.ShowOverlay)
            .HasColumnName("show_overlay")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(tt => tt.OverlayContent)
            .HasColumnName("overlay_content")
            .HasColumnType("TEXT");

        // Trigger
        builder.Property(tt => tt.AutoTrigger)
            .HasColumnName("auto_trigger")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(tt => tt.RequireUserAction)
            .HasColumnName("require_user_action")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(tt => tt.TriggerButtonText)
            .HasColumnName("trigger_button_text")
            .HasMaxLength(100)
            .HasDefaultValue("Next");

        builder.Property(tt => tt.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(tt => tt.UpdatedAt)
            .HasColumnName("updated_at");

        // Relationships
        builder.HasOne(tt => tt.Map)
            .WithMany()
            .HasForeignKey(tt => tt.MapId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tt => tt.FromSegment)
            .WithMany()
            .HasForeignKey(tt => tt.FromSegmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tt => tt.ToSegment)
            .WithMany()
            .HasForeignKey(tt => tt.ToSegmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
