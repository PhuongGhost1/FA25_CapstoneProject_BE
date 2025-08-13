using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase_AddNewTableAndDataForUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "membership_addons",
                columns: table => new
                {
                    addon_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    membership_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    org_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    addon_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    quantity = table.Column<int>(type: "int", nullable: true),
                    feature_payload = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    purchased_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    effective_from = table.Column<DateTime>(type: "datetime", nullable: true),
                    effective_until = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membership_addons", x => x.addon_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "membership_usages",
                columns: table => new
                {
                    usage_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    membership_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    org_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    maps_created_this_cycle = table.Column<int>(type: "int", nullable: false),
                    exports_this_cycle = table.Column<int>(type: "int", nullable: false),
                    active_users_in_org = table.Column<int>(type: "int", nullable: false),
                    feature_flags = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cycle_start_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    cycle_end_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membership_usages", x => x.usage_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "payment_gateways",
                columns: new[] { "gateway_id", "name" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000052"), "PayOS" });

            migrationBuilder.CreateIndex(
                name: "IX_membership_addons_membership_id_org_id_addon_key",
                table: "membership_addons",
                columns: new[] { "membership_id", "org_id", "addon_key" });

            migrationBuilder.CreateIndex(
                name: "IX_membership_usages_membership_id_org_id",
                table: "membership_usages",
                columns: new[] { "membership_id", "org_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "membership_addons");

            migrationBuilder.DropTable(
                name: "membership_usages");

            migrationBuilder.DeleteData(
                table: "payment_gateways",
                keyColumn: "gateway_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000052"));
        }
    }
}
