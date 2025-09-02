using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.Layers.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.LayerConfig;

internal class LayerTypeConfiguration : IEntityTypeConfiguration<LayerType>
{
    public void Configure(EntityTypeBuilder<LayerType> builder)
    {
        builder.ToTable("layer_types");

        builder.HasKey(lt => lt.LayerTypeId);

        builder.Property(lt => lt.LayerTypeId)
            .HasColumnName("layer_type_id")
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(lt => lt.TypeName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("type_name");

        builder.Property(lt => lt.Description)
            .HasMaxLength(500)
            .HasColumnName("description");

        builder.Property(lt => lt.IconUrl)
            .HasMaxLength(255)
            .HasColumnName("icon_url");

        builder.Property(lt => lt.IsActive)
            .HasColumnName("is_active");

        builder.Property(lt => lt.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        // Sample data based on URD layer requirements
        builder.HasData(
            new LayerType
            {
                LayerTypeId = 1,
                TypeName = LayerTypeEnum.GEOJSON.ToString(),
                Description = "Street and road networks from OpenStreetMap",
                IconUrl = "/icons/roads.svg",
                IsActive = true,
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            },
            new LayerType
            {
                LayerTypeId = 2,
                TypeName = LayerTypeEnum.KML.ToString(),
                Description = "Building footprints and structures",
                IconUrl = "/icons/buildings.svg",
                IsActive = true,
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            },
            new LayerType
            {
                LayerTypeId = 3,
                TypeName = LayerTypeEnum.Shapefile.ToString(),
                Description = "Points of Interest including amenities and landmarks",
                IconUrl = "/icons/poi.svg",
                IsActive = true,
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            },
            new LayerType
            {
                LayerTypeId = 4,
                TypeName = LayerTypeEnum.GEOJSON.ToString(),
                Description = "User uploaded GeoJSON data layers",
                IconUrl = "/icons/geojson.svg",
                IsActive = true,
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            },
            new LayerType
            {
                LayerTypeId = 5,
                TypeName = LayerTypeEnum.KML.ToString(),
                Description = "User uploaded KML data layers",
                IconUrl = "/icons/kml.svg",
                IsActive = true,
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            },
            new LayerType
            {
                LayerTypeId = 6,
                TypeName = LayerTypeEnum.CSV.ToString(),
                Description = "User uploaded CSV data with coordinates",
                IconUrl = "/icons/csv.svg",
                IsActive = true,
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
