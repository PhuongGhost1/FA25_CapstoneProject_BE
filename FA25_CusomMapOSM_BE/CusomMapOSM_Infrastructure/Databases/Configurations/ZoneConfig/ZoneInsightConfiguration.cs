using CusomMapOSM_Domain.Entities.Zones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.ZoneConfig;

internal class ZoneInsightConfiguration : IEntityTypeConfiguration<ZoneInsight>
{
    public void Configure(EntityTypeBuilder<ZoneInsight> builder)
    {
        builder.ToTable("zone_insights");

        builder.HasKey(zi => zi.ZoneInsightId);

        builder.Property(zi => zi.ZoneInsightId)
            .HasColumnName("zone_insight_id")
            .IsRequired();

        builder.Property(zi => zi.ZoneId)
            .HasColumnName("zone_id")
            .IsRequired();

        builder.Property(zi => zi.InsightType)
            .HasColumnName("insight_type")
            .HasConversion<string>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(zi => zi.Title)
            .HasColumnName("title")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(zi => zi.Summary)
            .HasColumnName("summary");

        builder.Property(zi => zi.Description)
            .HasColumnName("description");

        builder.Property(zi => zi.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(1024);

        builder.Property(zi => zi.ExternalUrl)
            .HasColumnName("external_url")
            .HasMaxLength(1024);

        builder.Property(zi => zi.Location)
            .HasColumnName("location")
            .HasColumnType("json");

        builder.Property(zi => zi.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("json");

        builder.Property(zi => zi.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(zi => zi.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        builder.HasOne(zi => zi.Zone)
            .WithMany()
            .HasForeignKey(zi => zi.ZoneId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
