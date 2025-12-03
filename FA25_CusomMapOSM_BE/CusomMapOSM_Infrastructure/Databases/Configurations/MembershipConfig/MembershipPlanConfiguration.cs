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

        builder.Property(p => p.MonthlyTokens)
            .HasColumnName("monthly_tokens")
            .IsRequired()
            .HasDefaultValue(10000);

        builder.Property(p => p.PrioritySupport)
            .HasColumnName("priority_support")
            .IsRequired();

        builder.Property(p => p.Features)
            .HasColumnName("features")
            .HasColumnType("json");


        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        // Membership plans with progressive features and pricing
        builder.HasData(
            new Plan
            {
                PlanId = 1,
                PlanName = MembershipPlanTypeEnum.Free.ToString(),
                Description = "Perfect for getting started. Explore basic mapping features at no cost.",
                PriceMonthly = 0.00m,
                DurationMonths = 1,
                MaxOrganizations = 1,
                MaxLocationsPerOrg = 5, 
                MaxMapsPerMonth = 10, 
                MaxUsersPerOrg = 3, 
                MapQuota = 20, 
                ExportQuota = 10, 
                MaxCustomLayers = 5, 
                MonthlyTokens = 10000, 
                PrioritySupport = false,
                Features = "{\"templates\": true, \"basic_export\": true, \"public_maps\": true, \"basic_collaboration\": true}",
                IsActive = true,
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            },
            new Plan
            {
                PlanId = 2,
                PlanName = MembershipPlanTypeEnum.Basic.ToString(),
                Description = "Ideal for small teams and individual professionals who need more features.",
                PriceMonthly = 9.99m,
                DurationMonths = 1,
                MaxOrganizations = 2,
                MaxLocationsPerOrg = 10,
                MaxMapsPerMonth = 50,
                MaxUsersPerOrg = 10,
                MapQuota = 100,
                ExportQuota = 100,
                MaxCustomLayers = 20,
                MonthlyTokens = 30000,
                PrioritySupport = false,
                Features = "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"private_maps\": true, \"advanced_layers\": true}",
                IsActive = true,
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            },
            new Plan
            {
                PlanId = 3,
                PlanName = MembershipPlanTypeEnum.Pro.ToString(),
                Description = "Advanced features for growing businesses and professional teams.",
                PriceMonthly = 29.99m,
                DurationMonths = 1,
                MaxOrganizations = 10,
                MaxLocationsPerOrg = 50,
                MaxMapsPerMonth = 200,
                MaxUsersPerOrg = 50,
                MapQuota = 500,
                ExportQuota = 500,
                MaxCustomLayers = 100,
                MonthlyTokens = 100000,
                PrioritySupport = true,
                Features = "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"analytics\": true, \"version_history\": true, \"advanced_analytics\": true, \"custom_branding\": true}",
                IsActive = true,
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            },
            new Plan
            {
                PlanId = 4,
                PlanName = MembershipPlanTypeEnum.Enterprise.ToString(),
                Description = "Full-featured solution with unlimited resources for large organizations.",
                PriceMonthly = 149.99m, 
                DurationMonths = 1,
                MaxOrganizations = -1, 
                MaxLocationsPerOrg = -1, 
                MaxMapsPerMonth = -1, 
                MaxUsersPerOrg = -1, 
                MapQuota = -1, 
                ExportQuota = -1, 
                MaxCustomLayers = -1, 
                MonthlyTokens = 500000, 
                PrioritySupport = true,
                Features = "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"analytics\": true, \"version_history\": true, \"api_access\": true, \"white_label\": true, \"sso\": true, \"dedicated_support\": true, \"custom_integrations\": true, \"advanced_security\": true}",
                IsActive = true,
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
