using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentUrlAndCancellationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "transactions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PaymentGatewayOrderCode",
                table: "transactions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PaymentUrl",
                table: "transactions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentUrlCreatedAt",
                table: "transactions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentUrlExpiresAt",
                table: "transactions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "transactions",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "PaymentGatewayOrderCode",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "PaymentUrl",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "PaymentUrlCreatedAt",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "PaymentUrlExpiresAt",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "transactions");
        }
    }
}
