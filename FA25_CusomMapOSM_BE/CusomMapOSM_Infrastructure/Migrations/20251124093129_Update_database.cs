using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_database : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_question_banks_workspaces_WorkspaceId",
                table: "question_banks");

            migrationBuilder.RenameColumn(
                name: "WorkspaceId",
                table: "question_banks",
                newName: "workspace_id");

            migrationBuilder.RenameIndex(
                name: "IX_question_banks_WorkspaceId",
                table: "question_banks",
                newName: "IX_question_banks_workspace_id");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_question_banks_workspaces_workspace_id",
                table: "question_banks");

            migrationBuilder.RenameColumn(
                name: "workspace_id",
                table: "question_banks",
                newName: "WorkspaceId");

            migrationBuilder.RenameIndex(
                name: "IX_question_banks_workspace_id",
                table: "question_banks",
                newName: "IX_question_banks_WorkspaceId");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkspaceId",
                table: "question_banks",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_question_banks_workspaces_WorkspaceId",
                table: "question_banks",
                column: "WorkspaceId",
                principalTable: "workspaces",
                principalColumn: "workspace_id");
        }
    }
}
