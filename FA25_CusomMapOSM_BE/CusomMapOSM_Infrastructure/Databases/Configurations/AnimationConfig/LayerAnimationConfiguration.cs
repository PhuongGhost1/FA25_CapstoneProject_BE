using CusomMapOSM_Domain.Entities.Animations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.AnimationConfig;

internal class LayerAnimationConfiguration : IEntityTypeConfiguration<LayerAnimation>
{
    public void Configure(EntityTypeBuilder<LayerAnimation> builder)
    {
        builder.ToTable("layer_animations");

        builder.HasKey(la => la.LayerAnimationId);

        builder.Property(la => la.LayerAnimationId)
            .HasColumnName("layer_animation_id")
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(la => la.LayerId)
            .HasColumnName("layer_id")
            .IsRequired();

        // Basic properties
        builder.Property(la => la.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(la => la.SourceUrl)
            .HasColumnName("source_url")
            .IsRequired()
            .HasMaxLength(500);

        // Map coordinates
        builder.Property(la => la.Coordinates)
            .HasColumnName("coordinates")
            .HasMaxLength(1000);

        // Transform
        builder.Property(la => la.RotationDeg)
            .HasColumnName("rotation_deg")
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0.0);

        builder.Property(la => la.Scale)
            .HasColumnName("scale")
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(1.0);

        builder.Property(la => la.ZIndex)
            .HasColumnName("z_index")
            .HasDefaultValue(1000);

        builder.Property(la => la.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        // Timestamps
        builder.Property(la => la.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(la => la.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime")
            .IsRequired(false);

        // Relationships
        builder.HasOne(la => la.Layer)
            .WithMany()
            .HasForeignKey(la => la.LayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
