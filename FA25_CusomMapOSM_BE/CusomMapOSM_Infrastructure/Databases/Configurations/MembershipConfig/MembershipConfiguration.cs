using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Memberships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MembershipConfig;

internal class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.ToTable("memberships");

        builder.HasKey(m => m.MembershipId);

        builder.Property(m => m.MembershipId)
            .HasColumnName("membership_id")
            .IsRequired();

        builder.Property(m => m.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(m => m.OrgId)
            .HasColumnName("org_id")
            .IsRequired();

        builder.Property(m => m.PlanId)
            .HasColumnName("plan_id")
            .IsRequired();

        builder.Property(m => m.StartDate)
            .HasColumnName("start_date")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(m => m.EndDate)
            .HasColumnName("end_date")
            .HasColumnType("datetime");

        builder.Property(m => m.StatusId)
            .HasColumnName("status_id")
            .IsRequired();

        builder.Property(m => m.AutoRenew)
            .HasColumnName("auto_renew")
            .IsRequired();

        builder.Property(m => m.CurrentUsage)
            .HasColumnName("current_usage")
            .HasColumnType("json")
            // .HasConversion(
            //     v => JsonSerializer.Serialize(v, null),
            //     v => JsonSerializer.Deserialize<MembershipUsage>(v, null) ?? new MembershipUsage()
            // )
            ;

        builder.Property(m => m.LastResetDate)
            .HasColumnName("last_reset_date")
            .HasColumnType("datetime");

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Organization)
            .WithMany()
            .HasForeignKey(m => m.OrgId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Plan)
            .WithMany()
            .HasForeignKey(m => m.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Status)
            .WithMany()
            .HasForeignKey(m => m.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
