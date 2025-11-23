using CusomMapOSM_Domain.Entities.Groups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.GroupConfig;

internal class SessionGroupConfiguration : IEntityTypeConfiguration<SessionGroup>
{
    public void Configure(EntityTypeBuilder<SessionGroup> builder)
    {
        builder.ToTable("session_groups");

        builder.HasKey(g => g.GroupId);

        builder.Property(g => g.GroupId)
            .HasColumnName("group_id")
            .IsRequired();

        builder.Property(g => g.SessionId)
            .HasColumnName("session_id")
            .IsRequired();

        builder.Property(g => g.GroupName)
            .HasColumnName("group_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(g => g.Color)
            .HasColumnName("color")
            .HasMaxLength(20);

        builder.Property(g => g.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(g => g.Session)
            .WithMany()
            .HasForeignKey(g => g.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}
