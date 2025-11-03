using CusomMapOSM_Domain.Entities.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MapConfig;

internal class MapImageConfiguration : IEntityTypeConfiguration<MapImage>
{
    public void Configure(EntityTypeBuilder<MapImage> builder)
    {
        builder.ToTable("map_images");

        builder.HasKey(mi => mi.MapImageId);

        builder.Property(mi => mi.MapImageId)
               .HasColumnName("map_image_id")
               .IsRequired();

        builder.Property(mi => mi.MapId)
               .HasColumnName("map_id")
               .IsRequired();

        builder.Property(mi => mi.ImageName)
               .HasColumnName("image_name")
               .HasMaxLength(255);

        builder.Property(mi => mi.ImageUrl)
               .HasColumnName("image_url")
               .HasMaxLength(500);

        builder.Property(mi => mi.ImageData)
               .HasColumnName("image_data")
               .HasColumnType("longtext");

        builder.Property(mi => mi.Latitude)
               .HasColumnName("latitude")
               .HasColumnType("decimal(10,8)");

        builder.Property(mi => mi.Longitude)
               .HasColumnName("longitude")
               .HasColumnType("decimal(11,8)");

        builder.Property(mi => mi.Width)
               .HasColumnName("width")
               .HasColumnType("decimal(10,2)");

        builder.Property(mi => mi.Height)
               .HasColumnName("height")
               .HasColumnType("decimal(10,2)");

        builder.Property(mi => mi.Rotation)
               .HasColumnName("rotation")
               .HasColumnType("decimal(5,2)");

        builder.Property(mi => mi.ZIndex)
               .HasColumnName("z_index")
               .HasDefaultValue(500);

        builder.Property(mi => mi.IsVisible)
               .HasColumnName("is_visible")
               .HasDefaultValue(true);

        builder.Property(mi => mi.Description)
               .HasColumnName("description")
               .HasMaxLength(500);

        builder.Property(mi => mi.CreatedAt)
               .HasColumnName("created_at")
               .HasColumnType("datetime")
               .IsRequired();
        
        builder.HasOne(mi => mi.Map)
               .WithMany()
               .HasForeignKey(mi => mi.MapId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
