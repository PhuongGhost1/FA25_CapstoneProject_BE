using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Reportmap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "map_reports",
                columns: table => new
                {
                    map_report_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    reporter_user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    reporter_email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reporter_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reason = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    reviewed_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    reviewed_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    review_notes = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_map_reports", x => x.map_report_id);
                    table.ForeignKey(
                        name: "FK_map_reports_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_map_reports_users_reporter_user_id",
                        column: x => x.reporter_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_map_reports_users_reviewed_by_user_id",
                        column: x => x.reviewed_by_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_map_reports_map_id",
                table: "map_reports",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_reports_reporter_user_id",
                table: "map_reports",
                column: "reporter_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_reports_reviewed_by_user_id",
                table: "map_reports",
                column: "reviewed_by_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "map_reports");
        }
    }
}
