using CusomMapOSM_Domain.Entities.Timeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MapConfig;

internal class TimelineStepConfiguration : IEntityTypeConfiguration<TimelineStep>
{
    public void Configure(EntityTypeBuilder<TimelineStep> builder)
    {
        builder.ToTable("timeline_steps");

        builder.HasKey(ts => ts.TimelineStepId);

        builder.Property(ts => ts.TimelineStepId)
            .HasColumnName("timeline_step_id")
            .IsRequired();

        builder.Property(ts => ts.MapId)
            .HasColumnName("map_id")
            .IsRequired();

        builder.Property(ts => ts.SegmentId)
            .HasColumnName("segment_id");

        builder.Property(ts => ts.Title)
            .HasColumnName("title")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(ts => ts.Subtitle)
            .HasColumnName("subtitle");

        builder.Property(ts => ts.Description)
            .HasColumnName("description");

        builder.Property(ts => ts.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(ts => ts.AutoAdvance)
            .HasColumnName("auto_advance")
            .HasDefaultValue(true);

        builder.Property(ts => ts.DurationMs)
            .HasColumnName("duration_ms")
            .HasDefaultValue(6000);

        builder.Property(ts => ts.TriggerType)
            .HasColumnName("trigger_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ts => ts.CameraState)
            .HasColumnName("camera_state")
            .HasColumnType("json");

        builder.Property(ts => ts.OverlayContent)
            .HasColumnName("overlay_content")
            .HasColumnType("longtext");

        builder.Property(ts => ts.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.HasOne(ts => ts.Map)
            .WithMany()
            .HasForeignKey(ts => ts.MapId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ts => ts.Segment)
            .WithMany()
            .HasForeignKey(ts => ts.SegmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
