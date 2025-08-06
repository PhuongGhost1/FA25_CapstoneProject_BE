using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Exports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.ExportConfig;

internal class ExportTypeConfiguration : IEntityTypeConfiguration<ExportType>
{
    public void Configure(EntityTypeBuilder<ExportType> builder)
    {
        builder.ToTable("export_types");

        builder.HasKey(et => et.TypeId);

        builder.Property(et => et.TypeId)
            .HasColumnName("type_id")
            .IsRequired();

        builder.Property(et => et.Name)
            .IsRequired()
            .HasColumnName("name")
            .HasMaxLength(100);

        // Sample data based on URD export format requirements
        builder.HasData(
            new ExportType { TypeId = SeedDataConstants.PdfExportTypeId, Name = "PDF" },
            new ExportType { TypeId = SeedDataConstants.PngExportTypeId, Name = "PNG" },
            new ExportType { TypeId = SeedDataConstants.SvgExportTypeId, Name = "SVG" },
            new ExportType { TypeId = SeedDataConstants.GeoJsonExportTypeId, Name = "GeoJSON" },
            new ExportType { TypeId = SeedDataConstants.MbtilesExportTypeId, Name = "MBTiles" }
        );
    }
}
