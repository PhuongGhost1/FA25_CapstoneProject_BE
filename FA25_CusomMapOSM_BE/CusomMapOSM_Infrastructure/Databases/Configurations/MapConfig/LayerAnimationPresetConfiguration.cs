using CusomMapOSM_Domain.Entities.Segments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MapConfig;

internal class LayerAnimationPresetConfiguration : IEntityTypeConfiguration<LayerAnimationPreset>
{
    public void Configure(EntityTypeBuilder<LayerAnimationPreset> builder)
    {
        builder.ToTable("layer_animation_presets");

        builder.HasKey(p => p.AnimationPresetId);

        builder.Property(p => p.AnimationPresetId)
            .HasColumnName("animation_preset_id")
            .IsRequired();

        builder.Property(p => p.Key)
            .HasColumnName("preset_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.AnimationType)
            .HasColumnName("animation_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.DefaultEasing)
            .HasColumnName("default_easing")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.DefaultDurationMs)
            .HasColumnName("default_duration_ms")
            .HasDefaultValue(600);

        builder.Property(p => p.Description)
            .HasColumnName("description");

        builder.Property(p => p.ConfigSchema)
            .HasColumnName("config_schema")
            .HasColumnType("json");

        builder.Property(p => p.IsSystemPreset)
            .HasColumnName("is_system_preset")
            .HasDefaultValue(true);

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");
    }
}
