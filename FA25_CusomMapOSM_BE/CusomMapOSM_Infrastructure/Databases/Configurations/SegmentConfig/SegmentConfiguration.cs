using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Segments.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.SegmentConfig;

internal class SegmentConfiguration : IEntityTypeConfiguration<Segment>
{
    public void Configure(EntityTypeBuilder<Segment> builder)
    {
        builder.ToTable("segments");

        builder.HasKey(s => s.SegmentId);

        builder.Property(s => s.SegmentId)
            .HasColumnName("segment_id")
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.Summary)
            .HasColumnName("summary")
            .HasMaxLength(1000);

        builder.Property(s => s.StoryContent)
            .HasColumnName("story_content")
            .HasColumnType("TEXT");

        builder.Property(s => s.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired();

        builder.Property(s => s.AutoFitBounds)
            .HasColumnName("auto_fit_bounds")
            .IsRequired();

        builder.Property(s => s.EntryAnimationPresetId)
            .HasColumnName("entry_animation_preset_id");

        builder.Property(s => s.ExitAnimationPresetId)
            .HasColumnName("exit_animation_preset_id");

        builder.Property(s => s.DefaultLayerAnimationPresetId)
            .HasColumnName("default_layer_animation_preset_id");

        builder.Property(s => s.MapId)
            .HasColumnName("map_id")
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(s => s.PlaybackMode)
            .HasColumnName("playback_mode")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);


        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

    }
}
