using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Exports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.ExportConfig;

internal class ExportConfiguration : IEntityTypeConfiguration<Export>
{
    public void Configure(EntityTypeBuilder<Export> builder)
    {
        builder.ToTable("exports");

        builder.HasKey(e => e.ExportId);
        builder.Property(e => e.ExportId)
            .HasColumnName("export_id")
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(e => e.FilePath)
            .HasColumnName("file_path")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.FileSize)
            .HasColumnName("file_size")
            .IsRequired();

        builder.Property(e => e.QuotaType)
            .HasColumnName("quota_type")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Membership)
            .WithMany()
            .HasForeignKey(e => e.MembershipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Map)
            .WithMany()
            .HasForeignKey(e => e.MapId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ExportType)
            .WithMany()
            .HasForeignKey(e => e.ExportTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}