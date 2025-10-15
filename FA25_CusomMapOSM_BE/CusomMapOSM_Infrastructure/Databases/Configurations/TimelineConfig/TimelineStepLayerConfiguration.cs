using CusomMapOSM_Domain.Entities.Timeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.TimelineConfig;

internal class TimelineStepLayerConfiguration : IEntityTypeConfiguration<TimelineStepLayer>
{
    public void Configure(EntityTypeBuilder<TimelineStepLayer> builder)
    {
        builder.ToTable("timeline_step_layers");

        builder.HasKey(tsl => tsl.TimelineStepLayerId);

        builder.Property(tsl => tsl.TimelineStepLayerId)
            .HasColumnName("timeline_step_layer_id")
            .IsRequired();

        builder.Property(tsl => tsl.TimelineStepId)
            .HasColumnName("timeline_step_id")
            .IsRequired();

        builder.Property(tsl => tsl.LayerId)
            .HasColumnName("layer_id")
            .IsRequired();

        builder.Property(tsl => tsl.IsVisible)
            .HasColumnName("is_visible")
            .HasDefaultValue(true);

        builder.Property(tsl => tsl.Opacity)
            .HasColumnName("opacity")
            .HasDefaultValue(1.0);

        builder.Property(tsl => tsl.FadeInMs)
            .HasColumnName("fade_in_ms")
            .HasDefaultValue(300);

        builder.Property(tsl => tsl.FadeOutMs)
            .HasColumnName("fade_out_ms")
            .HasDefaultValue(300);

        builder.Property(tsl => tsl.DelayMs)
            .HasColumnName("delay_ms")
            .HasDefaultValue(0);

        builder.Property(tsl => tsl.DisplayMode)
            .HasColumnName("display_mode")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(tsl => tsl.StyleOverride)
            .HasColumnName("style_override")
            .HasColumnType("json");

        builder.Property(tsl => tsl.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("json");

        builder.HasOne(tsl => tsl.TimelineStep)
            .WithMany()
            .HasForeignKey(tsl => tsl.TimelineStepId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tsl => tsl.Layer)
            .WithMany()
            .HasForeignKey(tsl => tsl.LayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
