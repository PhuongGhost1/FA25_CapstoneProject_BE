using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStartDateEndDateFromMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "end_date",
                table: "memberships");

            migrationBuilder.DropColumn(
                name: "start_date",
                table: "memberships");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "end_date",
                table: "memberships",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "start_date",
                table: "memberships",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
