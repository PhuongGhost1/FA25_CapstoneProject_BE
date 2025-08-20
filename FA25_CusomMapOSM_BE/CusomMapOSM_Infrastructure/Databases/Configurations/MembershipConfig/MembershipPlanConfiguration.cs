using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Memberships;
using CusomMapOSM_Domain.Entities.Memberships.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MembershipConfig;

internal class MembershipPlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("plans");

        builder.HasKey(p => p.PlanId);

        builder.Property(p => p.PlanId)
            .HasColumnName("plan_id")
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(p => p.PlanName)
            .HasColumnName("plan_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(p => p.PriceMonthly)
            .HasColumnName("price_monthly")
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.DurationMonths)
            .HasColumnName("duration_months")
            .IsRequired();

        builder.Property(p => p.MaxOrganizations)
            .HasColumnName("max_organizations")
            .IsRequired();

        builder.Property(p => p.MaxLocationsPerOrg)
            .HasColumnName("max_locations_per_org")
            .IsRequired();

        builder.Property(p => p.MaxMapsPerMonth)
            .HasColumnName("max_maps_per_month")
            .IsRequired();

        builder.Property(p => p.MaxUsersPerOrg)
            .HasColumnName("max_users_per_org")
            .IsRequired();

        builder.Property(p => p.MapQuota)
            .HasColumnName("map_quota")
            .IsRequired();

        builder.Property(p => p.ExportQuota)
            .HasColumnName("export_quota")
            .IsRequired();

        builder.Property(p => p.MaxCustomLayers)
            .HasColumnName("max_custom_layers")
            .IsRequired();

        builder.Property(p => p.PrioritySupport)
            .HasColumnName("priority_support")
            .IsRequired();

        builder.Property(p => p.Features)
            .HasColumnName("features")
            .HasColumnType("json");

        builder.Property(p => p.AccessToolIds)
            .HasColumnName("access_tool_ids")
            .HasColumnType("json");

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        // Sample membership plans based on URD subscription requirements
        builder.HasData(
            new Plan
            {
                PlanId = 1,
                PlanName = MembershipPlanTypeEnum.Free.ToString(),
                Description = "Basic features for individual users",
                PriceMonthly = 0.00m,
                DurationMonths = 1,
                MaxOrganizations = 1,
                MaxLocationsPerOrg = 1,
                MaxMapsPerMonth = 5,
                MaxUsersPerOrg = 1,
                MapQuota = 10,
                ExportQuota = 5,
                MaxCustomLayers = 3,
                PrioritySupport = false,
                Features = "{\"templates\": true, \"basic_export\": true, \"public_maps\": true}",
                AccessToolIds = "[1, 2, 3]",
                IsActive = true,
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            },
            new Plan
            {
                PlanId = 2,
                PlanName = MembershipPlanTypeEnum.Basic.ToString(),
                Description = "Essential features for small teams",
                PriceMonthly = 9.99m,
                DurationMonths = 1,
                MaxOrganizations = 2,
                MaxLocationsPerOrg = 5,
                MaxMapsPerMonth = 25,
                MaxUsersPerOrg = 5,
                MapQuota = 50,
                ExportQuota = 50,
                MaxCustomLayers = 10,
                PrioritySupport = false,
                Features = "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true}",
                AccessToolIds = "[1, 2, 3, 4, 5]",
                IsActive = true,
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            },
            new Plan
            {
                PlanId = 3,
                PlanName = MembershipPlanTypeEnum.Pro.ToString(),
                Description = "Advanced features for growing businesses",
                PriceMonthly = 29.99m,
                DurationMonths = 1,
                MaxOrganizations = 5,
                MaxLocationsPerOrg = 20,
                MaxMapsPerMonth = 100,
                MaxUsersPerOrg = 20,
                MapQuota = 200,
                ExportQuota = 200,
                MaxCustomLayers = 50,
                PrioritySupport = true,
                Features = "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"analytics\": true, \"version_history\": true}",
                AccessToolIds = "[1, 2, 3, 4, 5, 6, 7]",
                IsActive = true,
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            },
            new Plan
            {
                PlanId = 4,
                PlanName = MembershipPlanTypeEnum.Enterprise.ToString(),
                Description = "Full-featured solution for large organizations",
                PriceMonthly = 99.99m,
                DurationMonths = 1,
                MaxOrganizations = -1, // Unlimited
                MaxLocationsPerOrg = -1, // Unlimited
                MaxMapsPerMonth = -1, // Unlimited
                MaxUsersPerOrg = -1, // Unlimited
                MapQuota = -1, // Unlimited
                ExportQuota = -1, // Unlimited
                MaxCustomLayers = -1, // Unlimited
                PrioritySupport = true,
                Features = "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"analytics\": true, \"version_history\": true, \"api_access\": true, \"white_label\": true, \"sso\": true}",
                AccessToolIds = "[1, 2, 3, 4, 5, 6, 7, 8, 9, 10]",
                IsActive = true,
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
