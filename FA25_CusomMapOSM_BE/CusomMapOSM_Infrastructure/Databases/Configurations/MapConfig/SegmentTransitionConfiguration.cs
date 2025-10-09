using CusomMapOSM_Domain.Entities.Segments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MapConfig;

internal class SegmentTransitionConfiguration : IEntityTypeConfiguration<SegmentTransition>
{
    public void Configure(EntityTypeBuilder<SegmentTransition> builder)
    {
        builder.ToTable("segment_transitions");

        builder.HasKey(st => st.SegmentTransitionId);

        builder.Property(st => st.SegmentTransitionId)
            .HasColumnName("segment_transition_id")
            .IsRequired();

        builder.Property(st => st.FromSegmentId)
            .HasColumnName("from_segment_id")
            .IsRequired();

        builder.Property(st => st.ToSegmentId)
            .HasColumnName("to_segment_id")
            .IsRequired();

        builder.Property(st => st.EffectType)
            .HasColumnName("effect_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(st => st.AnimationPresetId)
            .HasColumnName("animation_preset_id");

        builder.Property(st => st.DurationMs)
            .HasColumnName("duration_ms")
            .HasDefaultValue(600);

        builder.Property(st => st.DelayMs)
            .HasColumnName("delay_ms")
            .HasDefaultValue(0);

        builder.Property(st => st.AutoPlay)
            .HasColumnName("auto_play")
            .HasDefaultValue(true);

        builder.Property(st => st.IsSkippable)
            .HasColumnName("is_skippable")
            .HasDefaultValue(true);

        builder.Property(st => st.TransitionConfig)
            .HasColumnName("transition_config")
            .HasColumnType("json");

        builder.Property(st => st.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("json");

        builder.HasOne(st => st.FromSegment)
            .WithMany()
            .HasForeignKey(st => st.FromSegmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(st => st.ToSegment)
            .WithMany()
            .HasForeignKey(st => st.ToSegmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(st => st.AnimationPreset)
            .WithMany()
            .HasForeignKey(st => st.AnimationPresetId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
