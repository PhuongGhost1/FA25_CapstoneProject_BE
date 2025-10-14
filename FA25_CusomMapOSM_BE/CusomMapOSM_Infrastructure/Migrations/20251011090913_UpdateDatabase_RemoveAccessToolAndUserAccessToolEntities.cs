using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase_RemoveAccessToolAndUserAccessToolEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_access_tools");

            migrationBuilder.DropTable(
                name: "access_tools");

            migrationBuilder.DropColumn(
                name: "access_tool_ids",
                table: "plans");

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

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "user_id", "account_status", "created_at", "email", "full_name", "last_login", "last_token_reset", "password_hash", "phone", "RoleId" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), 1, new DateTime(2025, 10, 11, 9, 9, 12, 529, DateTimeKind.Utc).AddTicks(7230), "admin@cusommaposm.com", "System Administrator", null, new DateTime(2025, 10, 11, 9, 9, 12, 529, DateTimeKind.Utc).AddTicks(7611), "3eb3fe66b31e3b4d10fa70b5cad49c7112294af6ae4e476a1c405155d45aa121", "+1234567890", new Guid("00000000-0000-0000-0000-000000000003") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.AddColumn<string>(
                name: "access_tool_ids",
                table: "plans",
                type: "json",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.CreateTable(
                name: "access_tools",
                columns: table => new
                {
                    access_tool_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    access_tool_description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    access_tool_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icon_url = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    required_membership = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_access_tools", x => x.access_tool_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_access_tools",
                columns: table => new
                {
                    user_access_tool_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccessToolId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ExpiredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    granted_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_access_tools", x => x.user_access_tool_id);
                    table.ForeignKey(
                        name: "FK_user_access_tools_access_tools_AccessToolId",
                        column: x => x.AccessToolId,
                        principalTable: "access_tools",
                        principalColumn: "access_tool_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_access_tools_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "access_tools",
                columns: new[] { "access_tool_id", "access_tool_description", "access_tool_name", "icon_url", "required_membership" },
                values: new object[,]
                {
                    { 1, "Add pin markers to maps", "Pin", "/icons/pin.svg", false },
                    { 2, "Draw lines on maps", "Line", "/icons/line.svg", false },
                    { 3, "Create and display routes", "Route", "/icons/route.svg", false },
                    { 4, "Draw polygon shapes on maps", "Polygon", "/icons/polygon.svg", false },
                    { 5, "Draw circular areas on maps", "Circle", "/icons/circle.svg", false },
                    { 6, "Add custom markers to maps", "Marker", "/icons/marker.svg", false },
                    { 7, "Highlight areas on maps", "Highlighter", "/icons/highlighter.svg", false },
                    { 8, "Add text annotations to maps", "Text", "/icons/text.svg", false },
                    { 9, "Add notes to map locations", "Note", "/icons/note.svg", false },
                    { 10, "Add clickable links to map elements", "Link", "/icons/link.svg", false },
                    { 11, "Embed videos in map popups", "Video", "/icons/video.svg", false },
                    { 12, "Calculate and display map bounds", "Bounds", "/icons/bounds.svg", true },
                    { 13, "Create buffer zones around features", "Buffer", "/icons/buffer.svg", true },
                    { 14, "Calculate centroids of features", "Centroid", "/icons/centroid.svg", true },
                    { 15, "Dissolve overlapping features", "Dissolve", "/icons/dissolve.svg", true },
                    { 16, "Clip features to specified boundaries", "Clip", "/icons/clip.svg", true },
                    { 17, "Count points within areas", "Count Points", "/icons/count-points.svg", true },
                    { 18, "Find intersections between features", "Intersect", "/icons/intersect.svg", true },
                    { 19, "Join data from different sources", "Join", "/icons/join.svg", true },
                    { 20, "Subtract one feature from another", "Subtract", "/icons/subtract.svg", true },
                    { 21, "Generate statistical analysis", "Statistic", "/icons/statistic.svg", true },
                    { 22, "Create bar charts from map data", "Bar Chart", "/icons/bar-chart.svg", true },
                    { 23, "Generate histograms from data", "Histogram", "/icons/histogram.svg", true },
                    { 24, "Filter map data by criteria", "Filter", "/icons/filter.svg", true },
                    { 25, "Analyze data over time", "Time Series", "/icons/time-series.svg", true },
                    { 26, "Search and find features", "Find", "/icons/find.svg", true },
                    { 27, "Measure distances and areas", "Measure", "/icons/measure.svg", true },
                    { 28, "Filter by spatial relationships", "Spatial Filter", "/icons/spatial-filter.svg", true },
                    { 29, "Create custom map extensions", "Custom Extension", "/icons/custom-extension.svg", true },
                    { 30, "Design custom popup templates", "Custom Popup", "/icons/custom-popup.svg", true },
                    { 31, "Get AI-powered map suggestions", "AI Suggestion", "/icons/ai-suggestion.svg", true }
                });

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 1,
                column: "access_tool_ids",
                value: "[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11]");

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 2,
                column: "access_tool_ids",
                value: "[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28]");

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 3,
                column: "access_tool_ids",
                value: "[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31]");

            migrationBuilder.CreateIndex(
                name: "IX_user_access_tools_AccessToolId",
                table: "user_access_tools",
                column: "AccessToolId");

            migrationBuilder.CreateIndex(
                name: "IX_user_access_tools_UserId",
                table: "user_access_tools",
                column: "UserId");
        }
    }
}
