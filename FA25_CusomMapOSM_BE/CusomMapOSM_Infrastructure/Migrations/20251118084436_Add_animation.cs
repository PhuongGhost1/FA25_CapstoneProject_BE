using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_animation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "route_animations",
                columns: table => new
                {
                    route_animation_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    segment_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    from_lat = table.Column<double>(type: "double", nullable: false),
                    from_lng = table.Column<double>(type: "double", nullable: false),
                    from_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    to_lat = table.Column<double>(type: "double", nullable: false),
                    to_lng = table.Column<double>(type: "double", nullable: false),
                    to_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    route_path = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icon_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValue: "car")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icon_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icon_width = table.Column<int>(type: "int", nullable: false, defaultValue: 32),
                    icon_height = table.Column<int>(type: "int", nullable: false, defaultValue: 32),
                    route_color = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "#666666")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    visited_color = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "#3b82f6")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    route_width = table.Column<int>(type: "int", nullable: false, defaultValue: 4),
                    duration_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 5000),
                    start_delay_ms = table.Column<int>(type: "int", nullable: true),
                    easing = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "linear")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    auto_play = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    loop = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    is_visible = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    z_index = table.Column<int>(type: "int", nullable: false, defaultValue: 1000),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    start_time_ms = table.Column<int>(type: "int", nullable: true),
                    end_time_ms = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route_animations", x => x.route_animation_id);
                    table.ForeignKey(
                        name: "FK_route_animations_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_route_animations_segments_segment_id",
                        column: x => x.segment_id,
                        principalTable: "segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_route_animations_map_id",
                table: "route_animations",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_route_animations_segment_id",
                table: "route_animations",
                column: "segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_route_animations_segment_id_display_order",
                table: "route_animations",
                columns: new[] { "segment_id", "display_order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "route_animations");
        }
    }
}
