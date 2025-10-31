using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class POI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Allow3DEffects",
                table: "plans",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowAnimatedConnections",
                table: "plans",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowAudioContent",
                table: "plans",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowVideoContent",
                table: "plans",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "MaxAudioFileSizeBytes",
                table: "plans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "MaxConnectionsPerMap",
                table: "plans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxInteractionsPerMap",
                table: "plans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "MaxMediaFileSizeBytes",
                table: "plans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "MaxVideoFileSizeBytes",
                table: "plans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                table: "locations",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
                name: "is_visible",
                table: "locations",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "z_index",
                table: "locations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 1,
                columns: new[] { "Allow3DEffects", "AllowAnimatedConnections", "AllowAudioContent", "AllowVideoContent", "MaxAudioFileSizeBytes", "MaxConnectionsPerMap", "MaxInteractionsPerMap", "MaxMediaFileSizeBytes", "MaxVideoFileSizeBytes" },
                values: new object[] { false, true, true, true, 20971520L, 100, 50, 10485760L, 104857600L });

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 2,
                columns: new[] { "Allow3DEffects", "AllowAnimatedConnections", "AllowAudioContent", "AllowVideoContent", "MaxAudioFileSizeBytes", "MaxConnectionsPerMap", "MaxInteractionsPerMap", "MaxMediaFileSizeBytes", "MaxVideoFileSizeBytes" },
                values: new object[] { false, true, true, true, 20971520L, 100, 50, 10485760L, 104857600L });

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 3,
                columns: new[] { "Allow3DEffects", "AllowAnimatedConnections", "AllowAudioContent", "AllowVideoContent", "MaxAudioFileSizeBytes", "MaxConnectionsPerMap", "MaxInteractionsPerMap", "MaxMediaFileSizeBytes", "MaxVideoFileSizeBytes" },
                values: new object[] { false, true, true, true, 20971520L, 100, 50, 10485760L, 104857600L });

            migrationBuilder.CreateIndex(
                name: "IX_locations_created_by",
                table: "locations",
                column: "created_by");

            migrationBuilder.AddForeignKey(
                name: "FK_locations_users_created_by",
                table: "locations",
                column: "created_by",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_locations_users_created_by",
                table: "locations");

            migrationBuilder.DropIndex(
                name: "IX_locations_created_by",
                table: "locations");

            migrationBuilder.DropColumn(
                name: "Allow3DEffects",
                table: "plans");

            migrationBuilder.DropColumn(
                name: "AllowAnimatedConnections",
                table: "plans");

            migrationBuilder.DropColumn(
                name: "AllowAudioContent",
                table: "plans");

            migrationBuilder.DropColumn(
                name: "AllowVideoContent",
                table: "plans");

            migrationBuilder.DropColumn(
                name: "MaxAudioFileSizeBytes",
                table: "plans");

            migrationBuilder.DropColumn(
                name: "MaxConnectionsPerMap",
                table: "plans");

            migrationBuilder.DropColumn(
                name: "MaxInteractionsPerMap",
                table: "plans");

            migrationBuilder.DropColumn(
                name: "MaxMediaFileSizeBytes",
                table: "plans");

            migrationBuilder.DropColumn(
                name: "MaxVideoFileSizeBytes",
                table: "plans");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "locations");

            migrationBuilder.DropColumn(
                name: "is_visible",
                table: "locations");

            migrationBuilder.DropColumn(
                name: "z_index",
                table: "locations");
        }
    }
}
