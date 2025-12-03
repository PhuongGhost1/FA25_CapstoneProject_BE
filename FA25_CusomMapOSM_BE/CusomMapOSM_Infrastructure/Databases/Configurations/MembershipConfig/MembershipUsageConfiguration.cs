using System;
using CusomMapOSM_Domain.Entities.Memberships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MembershipConfig;

internal class MembershipUsageConfiguration : IEntityTypeConfiguration<MembershipUsage>
{
    public void Configure(EntityTypeBuilder<MembershipUsage> builder)
    {
        builder.ToTable("membership_usages");

        builder.HasKey(u => u.UsageId);

        builder.Property(u => u.UsageId)
            .HasColumnName("usage_id")
            .IsRequired();

        builder.Property(u => u.MembershipId)
            .HasColumnName("membership_id")
            .IsRequired();

        builder.Property(u => u.OrgId)
            .HasColumnName("org_id")
            .IsRequired();

        builder.Property(u => u.MapsCreatedThisCycle)
            .HasColumnName("maps_created_this_cycle")
            .IsRequired();

        builder.Property(u => u.ExportsThisCycle)
            .HasColumnName("exports_this_cycle")
            .IsRequired();

        builder.Property(u => u.ActiveUsersInOrg)
            .HasColumnName("active_users_in_org")
            .IsRequired();

        builder.Property(u => u.FeatureFlags)
            .HasColumnName("feature_flags")
            .HasColumnType("json");

        builder.Property(u => u.CycleStartDate)
            .HasColumnName("cycle_start_date")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(u => u.CycleEndDate)
            .HasColumnName("cycle_end_date")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");
        
        builder.HasOne(u => u.Organizations)
            .WithMany()
            .HasForeignKey(m => m.OrgId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(u => u.Membership)
            .WithMany()
            .HasForeignKey(m => m.MembershipId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}


