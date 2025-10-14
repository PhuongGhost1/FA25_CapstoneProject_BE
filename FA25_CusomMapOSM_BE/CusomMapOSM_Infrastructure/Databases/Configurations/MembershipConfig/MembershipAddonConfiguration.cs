using System;
using CusomMapOSM_Domain.Entities.Memberships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MembershipConfig;

internal class MembershipAddonConfiguration : IEntityTypeConfiguration<MembershipAddon>
{
    public void Configure(EntityTypeBuilder<MembershipAddon> builder)
    {
        builder.ToTable("membership_addons");

        builder.HasKey(a => a.AddonId);

        builder.Property(a => a.AddonId)
               .HasColumnName("addon_id")
               .IsRequired();

        builder.Property(a => a.MembershipId)
               .HasColumnName("membership_id")
               .IsRequired();

        builder.Property(a => a.OrgId)
               .HasColumnName("org_id")
               .IsRequired();

        builder.Property(a => a.AddonKey)
               .HasColumnName("addon_key")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(a => a.Quantity)
               .HasColumnName("quantity");

        builder.Property(a => a.FeaturePayload)
               .HasColumnName("feature_payload")
               .HasColumnType("json");

        builder.Property(a => a.PurchasedAt)
               .HasColumnName("purchased_at")
               .HasColumnType("datetime")
               .IsRequired();

        builder.Property(a => a.EffectiveFrom)
               .HasColumnName("effective_from")
               .HasColumnType("datetime");

        builder.Property(a => a.EffectiveUntil)
               .HasColumnName("effective_until")
               .HasColumnType("datetime");

        builder.Property(a => a.CreatedAt)
               .HasColumnName("created_at")
               .HasColumnType("datetime")
               .IsRequired();

        builder.Property(a => a.UpdatedAt)
               .HasColumnName("updated_at")
               .HasColumnType("datetime");

    }
}


