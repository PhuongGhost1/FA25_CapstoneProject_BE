using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Exports;
using CusomMapOSM_Domain.Entities.Exports.Enums;
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
            .HasColumnType("text"); // Use TEXT instead of varchar(255) to support long Firebase URLs

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
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

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

        builder.Property(e => e.ExportType)
            .HasColumnName("export_type")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(ExportStatusEnum.Pending);

        builder.Property(e => e.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(e => e.ApprovedBy)
            .HasColumnName("approved_by")
            .HasColumnType("char(36)")
            .IsRequired(false);

        builder.Property(e => e.ApprovedAt)
            .HasColumnName("approved_at")
            .HasColumnType("datetime")
            .IsRequired(false);

        builder.Property(e => e.RejectionReason)
            .HasColumnName("rejection_reason")
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(e => e.CompletedAt)
            .HasColumnName("completed_at")
            .HasColumnType("datetime")
            .IsRequired(false);
    }
}