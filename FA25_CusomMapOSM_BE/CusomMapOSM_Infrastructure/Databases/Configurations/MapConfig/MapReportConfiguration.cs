using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Maps.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MapConfig;

internal class MapReportConfiguration : IEntityTypeConfiguration<MapReport>
{
    public void Configure(EntityTypeBuilder<MapReport> builder)
    {
        builder.ToTable("map_reports");

        builder.HasKey(mr => mr.MapReportId);

        builder.Property(mr => mr.MapReportId)
               .HasColumnName("map_report_id")
               .IsRequired();

        builder.Property(mr => mr.MapId)
               .HasColumnName("map_id")
               .IsRequired();

        builder.Property(mr => mr.ReporterUserId)
               .HasColumnName("reporter_user_id")
               .IsRequired();
               
        builder.Property(mr => mr.ReporterEmail)
               .HasColumnName("reporter_email")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(mr => mr.ReporterName)
               .HasColumnName("reporter_name")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(mr => mr.Reason)
               .HasColumnName("reason")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(mr => mr.Description)
               .HasColumnName("description")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(mr => mr.Status)
               .HasColumnName("status")
               .HasConversion<int>()
               .HasDefaultValue(MapReportStatusEnum.Pending);

        builder.Property(mr => mr.ReviewedByUserId)
               .HasColumnName("reviewed_by_user_id");
               
        builder.Property(mr => mr.ReviewedAt)
               .HasColumnName("reviewed_at")
               .HasColumnType("datetime");

        builder.Property(mr => mr.ReviewNotes)
               .HasColumnName("review_notes")
               .HasMaxLength(255);

        builder.Property(mr => mr.CreatedAt)
               .HasColumnName("created_at")
               .HasColumnType("datetime")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(mr => mr.UpdatedAt)
               .HasColumnName("updated_at")
               .HasColumnType("datetime");

        builder.HasOne(mr => mr.Map)   
               .WithMany()
               .HasForeignKey(mr => mr.MapId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mr => mr.ReporterUser)
               .WithMany()
               .HasForeignKey(mr => mr.ReporterUserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mr => mr.ReviewedByUser)
               .WithMany()
               .HasForeignKey(mr => mr.ReviewedByUserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}