using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase_AddColumnForAccessToolOfUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "access_tool_ids",
                table: "plans",
                type: "json",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 1,
                column: "access_tool_ids",
                value: "[1, 2, 3]");

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 2,
                column: "access_tool_ids",
                value: "[1, 2, 3, 4, 5]");

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 3,
                column: "access_tool_ids",
                value: "[1, 2, 3, 4, 5, 6, 7]");

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 4,
                column: "access_tool_ids",
                value: "[1, 2, 3, 4, 5, 6, 7, 8, 9, 10]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "access_tool_ids",
                table: "plans");
        }
    }
}
