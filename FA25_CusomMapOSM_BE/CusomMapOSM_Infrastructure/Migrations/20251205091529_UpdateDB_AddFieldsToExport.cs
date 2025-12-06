using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDB_AddFieldsToExport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "approved_at",
                table: "exports",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "approved_by",
                table: "exports",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "completed_at",
                table: "exports",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "error_message",
                table: "exports",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "rejection_reason",
                table: "exports",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "exports",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "approved_at",
                table: "exports");

            migrationBuilder.DropColumn(
                name: "approved_by",
                table: "exports");

            migrationBuilder.DropColumn(
                name: "completed_at",
                table: "exports");

            migrationBuilder.DropColumn(
                name: "error_message",
                table: "exports");

            migrationBuilder.DropColumn(
                name: "rejection_reason",
                table: "exports");

            migrationBuilder.DropColumn(
                name: "status",
                table: "exports");
        }
    }
}
