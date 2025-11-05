using CusomMapOSM_Domain.Entities.Animations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.AnimationConfig;

internal class AnimatedLayerConfiguration : IEntityTypeConfiguration<AnimatedLayer>
{
    public void Configure(EntityTypeBuilder<AnimatedLayer> builder)
    {
        builder.ToTable("animated_layers");

        builder.HasKey(al => al.AnimatedLayerId);

        builder.Property(al => al.AnimatedLayerId)
            .HasColumnName("animated_layer_id")
            .IsRequired();

        builder.Property(al => al.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(al => al.LayerId)
            .HasColumnName("layer_id");

        builder.Property(al => al.SegmentId)
            .HasColumnName("segment_id");

        builder.Property(al => al.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(al => al.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(al => al.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(al => al.MediaType)
            .HasColumnName("media_type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(al => al.SourceUrl)
            .HasColumnName("source_url")
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(al => al.ThumbnailUrl)
            .HasColumnName("thumbnail_url")
            .HasMaxLength(2000);

        // Positioning
        builder.Property(al => al.Coordinates)
            .HasColumnName("coordinates")
            .HasColumnType("TEXT");

        builder.Property(al => al.IsScreenOverlay)
            .HasColumnName("is_screen_overlay")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(al => al.ScreenPosition)
            .HasColumnName("screen_position")
            .HasColumnType("TEXT");

        // Transform
        builder.Property(al => al.RotationDeg)
            .HasColumnName("rotation_deg")
            .IsRequired()
            .HasDefaultValue(0.0);

        builder.Property(al => al.Scale)
            .HasColumnName("scale")
            .IsRequired()
            .HasDefaultValue(1.0);

        builder.Property(al => al.Opacity)
            .HasColumnName("opacity")
            .HasColumnType("decimal(3,2)")
            .IsRequired()
            .HasDefaultValue(1.0m);

        builder.Property(al => al.ZIndex)
            .HasColumnName("z_index")
            .IsRequired()
            .HasDefaultValue(1000);

        builder.Property(al => al.CssFilter)
            .HasColumnName("css_filter")
            .HasMaxLength(500);

        // Playback
        builder.Property(al => al.AutoPlay)
            .HasColumnName("auto_play")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(al => al.Loop)
            .HasColumnName("loop")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(al => al.PlaybackSpeed)
            .HasColumnName("playback_speed")
            .IsRequired()
            .HasDefaultValue(100);

        builder.Property(al => al.StartTimeMs)
            .HasColumnName("start_time_ms")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(al => al.EndTimeMs)
            .HasColumnName("end_time_ms");

        // Entry animation
        builder.Property(al => al.EntryDelayMs)
            .HasColumnName("entry_delay_ms")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(al => al.EntryDurationMs)
            .HasColumnName("entry_duration_ms")
            .IsRequired()
            .HasDefaultValue(400);

        builder.Property(al => al.EntryEffect)
            .HasColumnName("entry_effect")
            .HasMaxLength(50)
            .HasDefaultValue("fade");

        // Exit animation
        builder.Property(al => al.ExitDelayMs)
            .HasColumnName("exit_delay_ms")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(al => al.ExitDurationMs)
            .HasColumnName("exit_duration_ms")
            .IsRequired()
            .HasDefaultValue(400);

        builder.Property(al => al.ExitEffect)
            .HasColumnName("exit_effect")
            .HasMaxLength(50)
            .HasDefaultValue("fade");

        // Interaction
        builder.Property(al => al.EnableClick)
            .HasColumnName("enable_click")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(al => al.OnClickAction)
            .HasColumnName("on_click_action")
            .HasColumnType("TEXT");

        // Metadata
        builder.Property(al => al.IsVisible)
            .HasColumnName("is_visible")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(al => al.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(al => al.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        // Relationships
        builder.HasOne(al => al.Layer)
            .WithMany()
            .HasForeignKey(al => al.LayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(al => al.Segment)
            .WithMany()
            .HasForeignKey(al => al.SegmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(al => al.Creator)
            .WithMany()
            .HasForeignKey(al => al.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
