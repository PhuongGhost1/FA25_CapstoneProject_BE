using CusomMapOSM_Domain.Entities.StoryElement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.StoryElementConfig;

internal class StoryElementLayerConfiguration : IEntityTypeConfiguration<StoryElementLayer>
{
    public void Configure(EntityTypeBuilder<StoryElementLayer> builder)
    {
        builder.ToTable("story_element_layers");

        builder.HasKey(sel => sel.StoryElementLayerId);

        builder.Property(sel => sel.StoryElementLayerId)
            .HasColumnName("story_element_layer_id")
            .IsRequired();

        builder.Property(sel => sel.ElementId)
            .HasColumnName("element_id")
            .IsRequired();

        builder.Property(sel => sel.ElementType)
            .HasColumnName("element_type")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(sel => sel.LayerId)
            .HasColumnName("layer_id")
            .IsRequired();

        builder.Property(sel => sel.ZoneId)
            .HasColumnName("zone_id");

        builder.Property(sel => sel.ExpandToZone)
            .HasColumnName("expand_to_zone")
            .IsRequired();

        builder.Property(sel => sel.HighlightZoneBoundary)
            .HasColumnName("highlight_zone_boundary")
            .IsRequired();

        builder.Property(sel => sel.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired();

        builder.Property(sel => sel.DelayMs)
            .HasColumnName("delay_ms")
            .IsRequired();

        builder.Property(sel => sel.FadeInMs)
            .HasColumnName("fade_in_ms")
            .IsRequired();

        builder.Property(sel => sel.FadeOutMs)
            .HasColumnName("fade_out_ms")
            .IsRequired();

        builder.Property(sel => sel.StartOpacity)
            .HasColumnName("start_opacity")
            .IsRequired()
            .HasColumnType("decimal(3,2)");

        builder.Property(sel => sel.EndOpacity)
            .HasColumnName("end_opacity")
            .IsRequired()
            .HasColumnType("decimal(3,2)");

        builder.Property(sel => sel.Easing)
            .HasColumnName("easing")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(sel => sel.AnimationPresetId)
            .HasColumnName("animation_preset_id");

        builder.Property(sel => sel.AutoPlayAnimation)
            .HasColumnName("auto_play_animation")
            .IsRequired();

        builder.Property(sel => sel.RepeatCount)
            .HasColumnName("repeat_count")
            .IsRequired();

        builder.Property(sel => sel.AnimationOverrides)
            .HasColumnName("animation_overrides")
            .HasColumnType("TEXT");

        builder.Property(sel => sel.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("TEXT");

        builder.Property(sel => sel.IsVisible)
            .HasColumnName("is_visible")
            .IsRequired();

        builder.Property(sel => sel.Opacity)
            .HasColumnName("opacity")
            .IsRequired()
            .HasColumnType("decimal(3,2)");

        builder.Property(sel => sel.DisplayMode)
            .HasColumnName("display_mode")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(sel => sel.StyleOverride)
            .HasColumnName("style_override")
            .HasColumnType("TEXT");

        builder.Property(sel => sel.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(sel => sel.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        // Relationships
        builder.HasOne(sel => sel.Layer)
            .WithMany()
            .HasForeignKey(sel => sel.LayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sel => sel.Zone)
            .WithMany()
            .HasForeignKey(sel => sel.ZoneId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(sel => sel.AnimationPreset)
            .WithMany()
            .HasForeignKey(sel => sel.AnimationPresetId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}