using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_route : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_question_banks_workspaces_workspace_id",
                table: "question_banks");

            migrationBuilder.AddColumn<string>(
                name: "follow_camera",
                table: "route_animations",
                type: "TEXT",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "follow_camera_zoom",
                table: "route_animations",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "question_banks",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_question_banks_workspaces_workspace_id",
                table: "question_banks",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "workspace_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_question_banks_workspaces_workspace_id",
                table: "question_banks");

            migrationBuilder.DropColumn(
                name: "follow_camera",
                table: "route_animations");

            migrationBuilder.DropColumn(
                name: "follow_camera_zoom",
                table: "route_animations");

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "question_banks",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_question_banks_workspaces_workspace_id",
                table: "question_banks",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "workspace_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
