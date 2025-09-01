using CusomMapOSM_Domain.Entities.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MapConfig;

internal class MapAnnotationConfiguration : IEntityTypeConfiguration<MapAnnotation>
{
    public void Configure(EntityTypeBuilder<MapAnnotation> builder)
    {
        builder.ToTable("map_annotations");

        builder.HasKey(ma => ma.MapAnnotationId);

        builder.Property(ma => ma.MapAnnotationId)
               .HasColumnName("map_annotation_id")
               .IsRequired()
               .ValueGeneratedOnAdd();

        builder.Property(ma => ma.MapId)
               .HasColumnName("map_id")
               .IsRequired();

        builder.Property(ma => ma.AnnotationName)
               .HasColumnName("annotation_name")
               .HasMaxLength(255);

        builder.Property(ma => ma.AnnotationTypeId)
               .HasColumnName("annotation_type_id");

        builder.Property(ma => ma.GeometryData)
               .HasColumnName("geometry_data")
               .HasColumnType("text");

        builder.Property(ma => ma.Style)
               .HasColumnName("style")
               .HasColumnType("text");

        builder.Property(ma => ma.Content)
               .HasColumnName("content")
               .HasColumnType("text");

        builder.Property(ma => ma.Latitude)
               .HasColumnName("latitude")
               .HasColumnType("decimal(10,8)");

        builder.Property(ma => ma.Longitude)
               .HasColumnName("longitude")
               .HasColumnType("decimal(11,8)");

        builder.Property(ma => ma.IsVisible)
               .HasColumnName("is_visible")
               .HasDefaultValue(true);

        builder.Property(ma => ma.ZIndex)
               .HasColumnName("z_index")
               .HasDefaultValue(1000);

        builder.Property(ma => ma.CreatedAt)
               .HasColumnName("created_at")
               .HasColumnType("datetime")
               .IsRequired();
        
        builder.HasOne(ma => ma.Map)
               .WithMany()
               .HasForeignKey(ma => ma.MapId)
               .OnDelete(DeleteBehavior.Cascade);
        
    }
}
