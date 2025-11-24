using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class add_session_group : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "session_groups",
                columns: table => new
                {
                    group_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    session_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    group_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    color = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_groups", x => x.group_id);
                    table.ForeignKey(
                        name: "FK_session_groups_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "sessions",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "group_submissions",
                columns: table => new
                {
                    submission_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    group_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    attachment_urls = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    score = table.Column<int>(type: "int", nullable: true),
                    feedback = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    submitted_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    graded_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group_submissions", x => x.submission_id);
                    table.ForeignKey(
                        name: "FK_group_submissions_session_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "session_groups",
                        principalColumn: "group_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "session_group_members",
                columns: table => new
                {
                    group_member_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    group_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    session_participant_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    is_leader = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    joined_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_group_members", x => x.group_member_id);
                    table.ForeignKey(
                        name: "FK_session_group_members_session_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "session_groups",
                        principalColumn: "group_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_session_group_members_session_participants_session_participa~",
                        column: x => x.session_participant_id,
                        principalTable: "session_participants",
                        principalColumn: "session_participant_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_group_submissions_group_id",
                table: "group_submissions",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_session_group_members_group_id",
                table: "session_group_members",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_session_group_members_session_participant_id",
                table: "session_group_members",
                column: "session_participant_id");

            migrationBuilder.CreateIndex(
                name: "IX_session_groups_session_id",
                table: "session_groups",
                column: "session_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "group_submissions");

            migrationBuilder.DropTable(
                name: "session_group_members");

            migrationBuilder.DropTable(
                name: "session_groups");
        }
    }
}
