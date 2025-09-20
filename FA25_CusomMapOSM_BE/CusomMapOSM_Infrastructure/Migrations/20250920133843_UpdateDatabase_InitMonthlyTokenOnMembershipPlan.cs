using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase_InitMonthlyTokenOnMembershipPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_memberships_membership_statuses_status_id",
                table: "memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_support_tickets_ticket_statuses_status_id",
                table: "support_tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_users_account_statuses_AccountStatusId",
                table: "users");

            migrationBuilder.DropTable(
                name: "account_statuses");

            migrationBuilder.DropTable(
                name: "membership_statuses");

            migrationBuilder.DropTable(
                name: "ticket_statuses");

            migrationBuilder.DropIndex(
                name: "IX_users_AccountStatusId",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_support_tickets_status_id",
                table: "support_tickets");

            migrationBuilder.DropIndex(
                name: "IX_memberships_status_id",
                table: "memberships");

            migrationBuilder.DropColumn(
                name: "AccountStatusId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "status_id",
                table: "support_tickets");

            migrationBuilder.DropColumn(
                name: "status_id",
                table: "memberships");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_login",
                table: "users",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<int>(
                name: "account_status",
                table: "users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_token_reset",
                table: "users",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "monthly_token_usage",
                table: "users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "support_tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "monthly_tokens",
                table: "plans",
                type: "int",
                nullable: false,
                defaultValue: 10000);

            migrationBuilder.AlterColumn<Guid>(
                name: "invited_by",
                table: "organization_invitation",
                type: "char(50)",
                maxLength: 50,
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(string),
                oldType: "char(50)",
                oldMaxLength: 50)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "memberships",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 1,
                column: "monthly_tokens",
                value: 5000);

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 2,
                column: "monthly_tokens",
                value: 50000);

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 3,
                column: "monthly_tokens",
                value: 200000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "account_status",
                table: "users");

            migrationBuilder.DropColumn(
                name: "last_token_reset",
                table: "users");

            migrationBuilder.DropColumn(
                name: "monthly_token_usage",
                table: "users");

            migrationBuilder.DropColumn(
                name: "status",
                table: "support_tickets");

            migrationBuilder.DropColumn(
                name: "monthly_tokens",
                table: "plans");

            migrationBuilder.DropColumn(
                name: "status",
                table: "memberships");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_login",
                table: "users",
                type: "datetime",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AccountStatusId",
                table: "users",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "status_id",
                table: "support_tickets",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "invited_by",
                table: "organization_invitation",
                type: "char(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "status_id",
                table: "memberships",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "account_statuses",
                columns: table => new
                {
                    status_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_statuses", x => x.status_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "membership_statuses",
                columns: table => new
                {
                    status_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membership_statuses", x => x.status_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ticket_statuses",
                columns: table => new
                {
                    status_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket_statuses", x => x.status_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "account_statuses",
                columns: new[] { "status_id", "name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000004"), "Active" },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "Inactive" },
                    { new Guid("00000000-0000-0000-0000-000000000006"), "Suspended" },
                    { new Guid("00000000-0000-0000-0000-000000000007"), "PendingVerification" }
                });

            migrationBuilder.InsertData(
                table: "membership_statuses",
                columns: new[] { "status_id", "name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000030"), "Active" },
                    { new Guid("00000000-0000-0000-0000-000000000031"), "Expired" },
                    { new Guid("00000000-0000-0000-0000-000000000032"), "Suspended" },
                    { new Guid("00000000-0000-0000-0000-000000000033"), "PendingPayment" },
                    { new Guid("00000000-0000-0000-0000-000000000034"), "Cancelled" }
                });

            migrationBuilder.InsertData(
                table: "ticket_statuses",
                columns: new[] { "status_id", "name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000043"), "Open" },
                    { new Guid("00000000-0000-0000-0000-000000000044"), "InProgress" },
                    { new Guid("00000000-0000-0000-0000-000000000045"), "WaitingForCustomer" },
                    { new Guid("00000000-0000-0000-0000-000000000046"), "Resolved" },
                    { new Guid("00000000-0000-0000-0000-000000000047"), "Closed" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_AccountStatusId",
                table: "users",
                column: "AccountStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_support_tickets_status_id",
                table: "support_tickets",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_memberships_status_id",
                table: "memberships",
                column: "status_id");

            migrationBuilder.AddForeignKey(
                name: "FK_memberships_membership_statuses_status_id",
                table: "memberships",
                column: "status_id",
                principalTable: "membership_statuses",
                principalColumn: "status_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_support_tickets_ticket_statuses_status_id",
                table: "support_tickets",
                column: "status_id",
                principalTable: "ticket_statuses",
                principalColumn: "status_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_users_account_statuses_AccountStatusId",
                table: "users",
                column: "AccountStatusId",
                principalTable: "account_statuses",
                principalColumn: "status_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
