using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update_membership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 1,
                columns: new[] { "description", "export_quota", "features", "map_quota", "max_custom_layers", "max_locations_per_org", "max_maps_per_month", "max_users_per_org", "monthly_tokens" },
                values: new object[] { "Perfect for getting started. Explore basic mapping features at no cost.", 10, "{\"templates\": true, \"basic_export\": true, \"public_maps\": true, \"basic_collaboration\": true}", 20, 5, 5, 10, 3, 10000 });

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 2,
                columns: new[] { "description", "export_quota", "features", "map_quota", "max_custom_layers", "max_locations_per_org", "max_maps_per_month", "max_organizations", "max_users_per_org", "monthly_tokens", "plan_name", "price_monthly", "priority_support" },
                values: new object[] { "Ideal for small teams and individual professionals who need more features.", 100, "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"private_maps\": true, \"advanced_layers\": true}", 100, 20, 10, 50, 2, 10, 30000, "Basic", 9.99m, false });

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 3,
                columns: new[] { "description", "export_quota", "features", "map_quota", "max_custom_layers", "max_locations_per_org", "max_maps_per_month", "max_organizations", "max_users_per_org", "monthly_tokens", "plan_name", "price_monthly" },
                values: new object[] { "Advanced features for growing businesses and professional teams.", 500, "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"analytics\": true, \"version_history\": true, \"advanced_analytics\": true, \"custom_branding\": true}", 500, 100, 50, 200, 10, 50, 100000, "Pro", 29.99m });

            migrationBuilder.InsertData(
                table: "plans",
                columns: new[] { "plan_id", "Allow3DEffects", "AllowAnimatedConnections", "AllowAudioContent", "AllowVideoContent", "created_at", "description", "duration_months", "export_quota", "features", "is_active", "map_quota", "MaxAudioFileSizeBytes", "MaxConnectionsPerMap", "max_custom_layers", "MaxInteractionsPerMap", "max_locations_per_org", "max_maps_per_month", "MaxMediaFileSizeBytes", "max_organizations", "max_users_per_org", "MaxVideoFileSizeBytes", "monthly_tokens", "plan_name", "price_monthly", "priority_support", "updated_at" },
                values: new object[] { 4, false, true, true, true, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Full-featured solution with unlimited resources for large organizations.", 1, -1, "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"analytics\": true, \"version_history\": true, \"api_access\": true, \"white_label\": true, \"sso\": true, \"dedicated_support\": true, \"custom_integrations\": true, \"advanced_security\": true}", true, -1, 20971520L, 100, -1, 50, -1, -1, 10485760L, -1, -1, 104857600L, 500000, "Enterprise", 149.99m, true, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 1,
                columns: new[] { "description", "export_quota", "features", "map_quota", "max_custom_layers", "max_locations_per_org", "max_maps_per_month", "max_users_per_org", "monthly_tokens" },
                values: new object[] { "Basic features for individual users", 5, "{\"templates\": true, \"basic_export\": true, \"public_maps\": true}", 10, 3, 1, 5, 1, 5000 });

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 2,
                columns: new[] { "description", "export_quota", "features", "map_quota", "max_custom_layers", "max_locations_per_org", "max_maps_per_month", "max_organizations", "max_users_per_org", "monthly_tokens", "plan_name", "price_monthly", "priority_support" },
                values: new object[] { "Advanced features for growing businesses", 200, "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"analytics\": true, \"version_history\": true}", 200, 50, 20, 100, 5, 20, 50000, "Pro", 29.99m, true });

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 3,
                columns: new[] { "description", "export_quota", "features", "map_quota", "max_custom_layers", "max_locations_per_org", "max_maps_per_month", "max_organizations", "max_users_per_org", "monthly_tokens", "plan_name", "price_monthly" },
                values: new object[] { "Full-featured solution for large organizations", -1, "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"analytics\": true, \"version_history\": true, \"api_access\": true, \"white_label\": true, \"sso\": true}", -1, -1, -1, -1, -1, -1, 200000, "Enterprise", 99.99m });
        }
    }
}
