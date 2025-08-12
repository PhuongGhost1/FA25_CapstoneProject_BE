using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CusomMapOSM_Infrastructure.Services;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.FailedEmailConfig;

public class FailedEmailConfiguration : IEntityTypeConfiguration<FailedEmail>
{
    public void Configure(EntityTypeBuilder<FailedEmail> builder)
    {
        builder.ToTable("FailedEmails");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.ToEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Body)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(e => e.EmailData)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(e => e.FailureReason)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        builder.Property(e => e.LastRetryAt);
        builder.Property(e => e.ProcessedAt);

        // Indexes
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.ToEmail);
        builder.HasIndex(e => new { e.Status, e.RetryCount });
    }
}
