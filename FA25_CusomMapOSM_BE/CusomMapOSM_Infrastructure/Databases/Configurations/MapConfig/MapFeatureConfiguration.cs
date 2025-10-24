using CusomMapOSM_Domain.Entities.Maps;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MapConfig;

internal class MapFeatureConfiguration : IEntityTypeConfiguration<MapFeature>
{
    public void Configure(EntityTypeBuilder<MapFeature> builder)
    {
        builder.ToTable("map_features");

        builder.HasKey(mf => mf.FeatureId);

        builder.Property(mf => mf.FeatureId)
            .HasColumnName("feature_id")
            .IsRequired();

        builder.Property(mf => mf.MapId)
            .HasColumnName("map_id")
            .IsRequired();

        builder.Property(mf => mf.Name)
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.Property(mf => mf.Description)
            .HasColumnType("text")
            .HasColumnName("description");

        // Enum conversions stored as strings for readability
        builder.Property(mf => mf.GeometryType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("geometry_type")
            .IsRequired();

        builder.Property(mf => mf.FeatureCategory)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("feature_category")
            .IsRequired();

        builder.Property(mf => mf.AnnotationType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("annotation_type");

        builder.Property(mf => mf.MongoDocumentId)
            .HasMaxLength(50)
            .HasColumnName("mongo_document_id");

        builder.Property(mf => mf.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(mf => mf.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(mf => mf.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        builder.Property(mf => mf.IsVisible)
            .HasColumnName("is_visible")
            .HasDefaultValue(true);

        builder.Property(mf => mf.ZIndex)
            .HasColumnName("z_index")
            .HasDefaultValue(0);

        // Foreign key relationships
        builder.HasOne(mf => mf.Map)
            .WithMany()
            .HasForeignKey(mf => mf.MapId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mf => mf.Creator)
            .WithMany()
            .HasForeignKey(mf => mf.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(mf => mf.LayerId)
            .HasColumnName("layer_id");

        builder.HasOne(mf => mf.Layer)
            .WithMany()
            .HasForeignKey(mf => mf.LayerId)
            .OnDelete(DeleteBehavior.SetNull);

    }
}
