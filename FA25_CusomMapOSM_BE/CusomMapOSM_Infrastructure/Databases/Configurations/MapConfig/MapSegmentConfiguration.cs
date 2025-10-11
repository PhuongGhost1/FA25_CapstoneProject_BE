using CusomMapOSM_Domain.Entities.Segments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MapConfig;

internal class MapSegmentConfiguration : IEntityTypeConfiguration<MapSegment>
{
    public void Configure(EntityTypeBuilder<MapSegment> builder)
    {
        builder.ToTable("map_segments");

        builder.HasKey(ms => ms.SegmentId);

        builder.Property(ms => ms.SegmentId)
            .HasColumnName("segment_id")
            .IsRequired();

        builder.Property(ms => ms.MapId)
            .HasColumnName("map_id")
            .IsRequired();

        builder.Property(ms => ms.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(ms => ms.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(ms => ms.Summary)
            .HasColumnName("summary");

        builder.Property(ms => ms.StoryContent)
            .HasColumnName("story_content")
            .HasColumnType("longtext");

        builder.Property(ms => ms.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(ms => ms.AutoFitBounds)
            .HasColumnName("auto_fit_bounds")
            .HasDefaultValue(true);

        builder.Property(ms => ms.EntryAnimationPresetId)
            .HasColumnName("entry_animation_preset_id");

        builder.Property(ms => ms.ExitAnimationPresetId)
            .HasColumnName("exit_animation_preset_id");

        builder.Property(ms => ms.DefaultLayerAnimationPresetId)
            .HasColumnName("default_layer_animation_preset_id");

        builder.Property(ms => ms.PlaybackMode)
            .HasColumnName("playback_mode")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ms => ms.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(ms => ms.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        builder.HasOne(ms => ms.Map)
            .WithMany()
            .HasForeignKey(ms => ms.MapId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ms => ms.Creator)
            .WithMany()
            .HasForeignKey(ms => ms.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ms => ms.EntryAnimationPreset)
            .WithMany()
            .HasForeignKey(ms => ms.EntryAnimationPresetId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(ms => ms.ExitAnimationPreset)
            .WithMany()
            .HasForeignKey(ms => ms.ExitAnimationPresetId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(ms => ms.DefaultLayerAnimationPreset)
            .WithMany()
            .HasForeignKey(ms => ms.DefaultLayerAnimationPresetId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
