using CusomMapOSM_Domain.Entities.Zones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.ZoneConfig;

internal class ZoneStatisticConfiguration : IEntityTypeConfiguration<ZoneStatistic>
{
    public void Configure(EntityTypeBuilder<ZoneStatistic> builder)
    {
        builder.ToTable("zone_statistics");

        builder.HasKey(zs => zs.ZoneStatisticId);

        builder.Property(zs => zs.ZoneStatisticId)
            .HasColumnName("zone_statistic_id")
            .IsRequired();

        builder.Property(zs => zs.ZoneId)
            .HasColumnName("zone_id")
            .IsRequired();

        builder.Property(zs => zs.MetricType)
            .HasColumnName("metric_type")
            .HasConversion<string>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(zs => zs.NumericValue)
            .HasColumnName("numeric_value");

        builder.Property(zs => zs.TextValue)
            .HasColumnName("text_value");

        builder.Property(zs => zs.Unit)
            .HasColumnName("unit")
            .HasMaxLength(50);

        builder.Property(zs => zs.Year)
            .HasColumnName("year");

        builder.Property(zs => zs.Quarter)
            .HasColumnName("quarter");

        builder.Property(zs => zs.Source)
            .HasColumnName("source")
            .HasMaxLength(255);

        builder.Property(zs => zs.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("json");

        builder.Property(zs => zs.CollectedAt)
            .HasColumnName("collected_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.HasOne(zs => zs.Zone)
            .WithMany()
            .HasForeignKey(zs => zs.ZoneId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
