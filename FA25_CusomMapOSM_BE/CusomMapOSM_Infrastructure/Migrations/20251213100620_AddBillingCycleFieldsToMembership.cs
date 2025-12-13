using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingCycleFieldsToMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "billing_cycle_end_date",
                table: "memberships",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "billing_cycle_start_date",
                table: "memberships",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // Update existing records with default billing cycle dates
            migrationBuilder.Sql(@"
                UPDATE memberships 
                SET 
                    billing_cycle_start_date = start_date,
                    billing_cycle_end_date = COALESCE(end_date, DATE_ADD(start_date, INTERVAL 30 DAY))
                WHERE billing_cycle_start_date = '0001-01-01 00:00:00' OR billing_cycle_end_date = '0001-01-01 00:00:00';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "billing_cycle_end_date",
                table: "memberships");

            migrationBuilder.DropColumn(
                name: "billing_cycle_start_date",
                table: "memberships");
        }
    }
}
