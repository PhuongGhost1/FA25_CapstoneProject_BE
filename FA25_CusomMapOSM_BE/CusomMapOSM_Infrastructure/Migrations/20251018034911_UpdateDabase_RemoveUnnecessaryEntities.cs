using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDabase_RemoveUnnecessaryEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "last_token_reset" },
                values: new object[] { new DateTime(2025, 10, 18, 3, 49, 10, 467, DateTimeKind.Utc).AddTicks(4845), new DateTime(2025, 10, 18, 3, 49, 10, 467, DateTimeKind.Utc).AddTicks(5126) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "last_token_reset" },
                values: new object[] { new DateTime(2025, 10, 16, 18, 49, 39, 794, DateTimeKind.Utc).AddTicks(5678), new DateTime(2025, 10, 16, 18, 49, 39, 794, DateTimeKind.Utc).AddTicks(5960) });
        }
    }
}
