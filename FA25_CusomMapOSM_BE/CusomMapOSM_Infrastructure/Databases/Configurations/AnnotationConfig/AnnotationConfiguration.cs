using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.AnnotationConfig;

internal class AnnotationConfiguration : IEntityTypeConfiguration<Annotation>
{
       public void Configure(EntityTypeBuilder<Annotation> builder)
       {
              builder.ToTable("annotations");

              builder.HasKey(a => a.AnnotationId);

              builder.Property(a => a.AnnotationId)
                     .HasColumnName("annotation_id")
                     .IsRequired()
                     .ValueGeneratedOnAdd();

              builder.Property(a => a.TypeId)
                     .HasColumnName("type_id")
                     .IsRequired();

              builder.Property(a => a.MapId)
                     .HasColumnName("map_id")
                     .IsRequired();

              builder.Property(a => a.Geometry)
                     .HasColumnName("geometry")
                     .HasColumnType("json") // or "longtext" if not querying json
                     .IsRequired(false);

              builder.Property(a => a.Properties)
                     .HasColumnName("properties")
                     .HasColumnType("json") // or "longtext"
                     .IsRequired(false);

              builder.Property(a => a.CreatedAt)
                     .HasColumnName("created_at")
                     .HasColumnType("datetime")
                     .IsRequired();

              // Relationships
              builder.HasOne(a => a.Type)
                     .WithMany()
                     .HasForeignKey(a => a.TypeId)
                     .OnDelete(DeleteBehavior.Restrict);

              builder.HasOne(a => a.Map)
                     .WithMany()
                     .HasForeignKey(a => a.MapId)
                     .OnDelete(DeleteBehavior.Cascade);
       }
}
