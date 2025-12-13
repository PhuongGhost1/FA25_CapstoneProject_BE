using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMaxOrganizationsFromPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max_organizations",
                table: "plans");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "max_organizations",
                table: "plans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 1,
                column: "max_organizations",
                value: 1);

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 2,
                column: "max_organizations",
                value: 2);

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 3,
                column: "max_organizations",
                value: 10);

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 4,
                column: "max_organizations",
                value: -1);
        }
    }
}
