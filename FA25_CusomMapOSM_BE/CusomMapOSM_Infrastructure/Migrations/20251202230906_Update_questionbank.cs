using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_questionbank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_sessions_question_banks_question_bank_id",
                table: "sessions");

            migrationBuilder.DropTable(
                name: "map_question_banks");

            migrationBuilder.DropIndex(
                name: "IX_Session_QuestionBankId",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "question_bank_id",
                table: "sessions");

            migrationBuilder.CreateTable(
                name: "session_question_banks",
                columns: table => new
                {
                    session_question_bank_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    session_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    question_bank_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    attached_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_question_banks", x => x.session_question_bank_id);
                    table.ForeignKey(
                        name: "FK_session_question_banks_question_banks_question_bank_id",
                        column: x => x.question_bank_id,
                        principalTable: "question_banks",
                        principalColumn: "question_bank_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_session_question_banks_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "sessions",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_session_question_banks_question_bank_id",
                table: "session_question_banks",
                column: "question_bank_id");

            migrationBuilder.CreateIndex(
                name: "IX_session_question_banks_session_id",
                table: "session_question_banks",
                column: "session_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "session_question_banks");

            migrationBuilder.AddColumn<Guid>(
                name: "question_bank_id",
                table: "sessions",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "map_question_banks",
                columns: table => new
                {
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    question_bank_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    assigned_at = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_map_question_banks", x => new { x.map_id, x.question_bank_id });
                    table.ForeignKey(
                        name: "FK_map_question_banks_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_map_question_banks_question_banks_question_bank_id",
                        column: x => x.question_bank_id,
                        principalTable: "question_banks",
                        principalColumn: "question_bank_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Session_QuestionBankId",
                table: "sessions",
                column: "question_bank_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_question_banks_question_bank_id",
                table: "map_question_banks",
                column: "question_bank_id");

            migrationBuilder.AddForeignKey(
                name: "FK_sessions_question_banks_question_bank_id",
                table: "sessions",
                column: "question_bank_id",
                principalTable: "question_banks",
                principalColumn: "question_bank_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
