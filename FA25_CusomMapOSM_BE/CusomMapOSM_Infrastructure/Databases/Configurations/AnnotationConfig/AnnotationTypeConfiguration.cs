using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.AnnotationConfig;

internal class AnnotationTypeConfiguration : IEntityTypeConfiguration<AnnotationType>
{
    public void Configure(EntityTypeBuilder<AnnotationType> builder)
    {
        builder.ToTable("annotation_types");

        builder.HasKey(t => t.TypeId);

        builder.Property(t => t.TypeId)
               .HasColumnName("type_id")
               .IsRequired();

        builder.Property(t => t.TypeName)
               .IsRequired()
               .HasMaxLength(100)
               .HasColumnName("type_name");

        // Sample data based on URD annotation requirements (markers/lines/polygons)
        builder.HasData(
            new AnnotationType { TypeId = SeedDataConstants.MarkerTypeId, TypeName = "Marker" },
            new AnnotationType { TypeId = SeedDataConstants.LineTypeId, TypeName = "Line" },
            new AnnotationType { TypeId = SeedDataConstants.PolygonTypeId, TypeName = "Polygon" },
            new AnnotationType { TypeId = SeedDataConstants.CircleTypeId, TypeName = "Circle" },
            new AnnotationType { TypeId = SeedDataConstants.RectangleTypeId, TypeName = "Rectangle" },
            new AnnotationType { TypeId = SeedDataConstants.TextLabelTypeId, TypeName = "Text Label" }
        );
    }
}
