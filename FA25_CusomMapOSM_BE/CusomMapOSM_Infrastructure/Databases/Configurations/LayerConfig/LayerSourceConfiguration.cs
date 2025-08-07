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

internal class LayerSourceConfiguration : IEntityTypeConfiguration<LayerSource>
{
    public void Configure(EntityTypeBuilder<LayerSource> builder)
    {
        builder.ToTable("layer_sources");

        builder.HasKey(ls => ls.SourceTypeId);

        builder.Property(ls => ls.SourceTypeId)
            .HasColumnName("source_type_id")
            .IsRequired();

        builder.Property(ls => ls.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("name");

        // Sample data for layer sources
        builder.HasData(
            new LayerSource { SourceTypeId = SeedDataConstants.OpenStreetMapSourceTypeId, Name = LayerSourceEnum.OpenStreetMap.ToString() },
            new LayerSource { SourceTypeId = SeedDataConstants.UserUploadSourceTypeId, Name = LayerSourceEnum.UserUploaded.ToString() },
            new LayerSource { SourceTypeId = SeedDataConstants.ExternalApiSourceTypeId, Name = LayerSourceEnum.ExternalAPI.ToString() },
            new LayerSource { SourceTypeId = SeedDataConstants.DatabaseSourceTypeId, Name = LayerSourceEnum.Database.ToString() },
            new LayerSource { SourceTypeId = SeedDataConstants.WebServiceSourceTypeId, Name = LayerSourceEnum.WebMapService.ToString() }
        );
    }
}
