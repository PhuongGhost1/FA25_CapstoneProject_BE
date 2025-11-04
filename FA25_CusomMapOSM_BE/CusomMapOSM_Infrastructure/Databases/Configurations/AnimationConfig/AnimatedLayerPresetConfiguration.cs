using CusomMapOSM_Domain.Entities.Animations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.AnimationConfig;

internal class AnimatedLayerPresetConfiguration : IEntityTypeConfiguration<AnimatedLayerPreset>
{
    public void Configure(EntityTypeBuilder<AnimatedLayerPreset> builder)
    {
        builder.ToTable("animated_layer_presets");

        builder.HasKey(alp => alp.AnimatedLayerPresetId);

        builder.Property(alp => alp.AnimatedLayerPresetId)
            .HasColumnName("animated_layer_preset_id")
            .IsRequired();

        builder.Property(alp => alp.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(alp => alp.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(alp => alp.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(alp => alp.Category)
            .HasColumnName("category")
            .HasMaxLength(100);

        builder.Property(alp => alp.Tags)
            .HasColumnName("tags")
            .HasMaxLength(500);

        builder.Property(alp => alp.MediaType)
            .HasColumnName("media_type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(alp => alp.SourceUrl)
            .HasColumnName("source_url")
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(alp => alp.ThumbnailUrl)
            .HasColumnName("thumbnail_url")
            .IsRequired()
            .HasMaxLength(2000);

        // Default settings
        builder.Property(alp => alp.DefaultCoordinates)
            .HasColumnName("default_coordinates")
            .HasColumnType("TEXT");

        builder.Property(alp => alp.DefaultIsScreenOverlay)
            .HasColumnName("default_is_screen_overlay")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(alp => alp.DefaultScreenPosition)
            .HasColumnName("default_screen_position")
            .HasColumnType("TEXT");

        builder.Property(alp => alp.DefaultScale)
            .HasColumnName("default_scale")
            .IsRequired()
            .HasDefaultValue(1.0);

        builder.Property(alp => alp.DefaultOpacity)
            .HasColumnName("default_opacity")
            .HasColumnType("decimal(3,2)")
            .IsRequired()
            .HasDefaultValue(1.0m);

        builder.Property(alp => alp.DefaultAutoPlay)
            .HasColumnName("default_auto_play")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(alp => alp.DefaultLoop)
            .HasColumnName("default_loop")
            .IsRequired()
            .HasDefaultValue(true);

        // Metadata
        builder.Property(alp => alp.IsSystemPreset)
            .HasColumnName("is_system_preset")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(alp => alp.IsPublic)
            .HasColumnName("is_public")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(alp => alp.UsageCount)
            .HasColumnName("usage_count")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(alp => alp.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(alp => alp.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(alp => alp.UpdatedAt)
            .HasColumnName("updated_at");

        // Relationships
        builder.HasOne(alp => alp.Creator)
            .WithMany()
            .HasForeignKey(alp => alp.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
