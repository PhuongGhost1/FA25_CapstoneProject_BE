using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDB_ModifyPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 1,
                columns: new[] { "export_quota", "max_locations_per_org", "max_maps_per_month", "max_organizations", "max_users_per_org", "monthly_tokens" },
                values: new object[] { 10, 5, 10, 5, 5, 10000 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 1,
                columns: new[] { "export_quota", "max_locations_per_org", "max_maps_per_month", "max_organizations", "max_users_per_org", "monthly_tokens" },
                values: new object[] { 5, 1, 5, 1, 1, 5000 });

            migrationBuilder.InsertData(
                table: "plans",
                columns: new[] { "plan_id", "Allow3DEffects", "AllowAnimatedConnections", "AllowAudioContent", "AllowVideoContent", "created_at", "description", "duration_months", "export_quota", "features", "is_active", "map_quota", "MaxAudioFileSizeBytes", "MaxConnectionsPerMap", "max_custom_layers", "MaxInteractionsPerMap", "max_locations_per_org", "max_maps_per_month", "MaxMediaFileSizeBytes", "max_organizations", "max_users_per_org", "MaxVideoFileSizeBytes", "monthly_tokens", "plan_name", "price_monthly", "priority_support", "updated_at" },
                values: new object[] { 3, false, true, true, true, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Full-featured solution for large organizations", 1, -1, "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"analytics\": true, \"version_history\": true, \"api_access\": true, \"white_label\": true, \"sso\": true}", true, -1, 20971520L, 100, -1, 50, -1, -1, 10485760L, -1, -1, 104857600L, 200000, "Enterprise", 99.99m, true, null });
        }
    }
}
