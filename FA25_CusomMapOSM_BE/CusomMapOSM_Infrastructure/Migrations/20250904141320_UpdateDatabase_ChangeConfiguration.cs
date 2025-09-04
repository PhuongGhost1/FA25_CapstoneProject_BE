using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase_ChangeConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 1,
                columns: new[] { "access_tool_description", "access_tool_name", "icon_url" },
                values: new object[] { "Add pin markers to maps", "Pin", "/icons/pin.svg" });

            migrationBuilder.UpdateData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 2,
                columns: new[] { "access_tool_description", "access_tool_name", "icon_url" },
                values: new object[] { "Draw lines on maps", "Line", "/icons/line.svg" });

            migrationBuilder.UpdateData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 3,
                columns: new[] { "access_tool_description", "access_tool_name", "icon_url" },
                values: new object[] { "Create and display routes", "Route", "/icons/route.svg" });

            migrationBuilder.UpdateData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 4,
                columns: new[] { "access_tool_description", "access_tool_name", "icon_url", "required_membership" },
                values: new object[] { "Draw polygon shapes on maps", "Polygon", "/icons/polygon.svg", false });

            migrationBuilder.UpdateData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 5,
                columns: new[] { "access_tool_description", "access_tool_name", "icon_url", "required_membership" },
                values: new object[] { "Draw circular areas on maps", "Circle", "/icons/circle.svg", false });

            migrationBuilder.UpdateData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 6,
                columns: new[] { "access_tool_description", "access_tool_name", "icon_url", "required_membership" },
                values: new object[] { "Add custom markers to maps", "Marker", "/icons/marker.svg", false });

            migrationBuilder.InsertData(
                table: "access_tools",
                columns: new[] { "access_tool_id", "access_tool_description", "access_tool_name", "icon_url", "required_membership" },
                values: new object[,]
                {
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
                columns: new[] { "access_tool_ids", "description", "export_quota", "features", "map_quota", "max_custom_layers", "max_locations_per_org", "max_maps_per_month", "max_organizations", "max_users_per_org", "plan_name", "price_monthly", "priority_support" },
                values: new object[] { "[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28]", "Advanced features for growing businesses", 200, "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"analytics\": true, \"version_history\": true}", 200, 50, 20, 100, 5, 20, "Pro", 29.99m, true });

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 3,
                columns: new[] { "access_tool_ids", "description", "export_quota", "features", "map_quota", "max_custom_layers", "max_locations_per_org", "max_maps_per_month", "max_organizations", "max_users_per_org", "plan_name", "price_monthly" },
                values: new object[] { "[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31]", "Full-featured solution for large organizations", -1, "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"analytics\": true, \"version_history\": true, \"api_access\": true, \"white_label\": true, \"sso\": true}", -1, -1, -1, -1, -1, -1, "Enterprise", 99.99m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 31);

            migrationBuilder.UpdateData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 1,
                columns: new[] { "access_tool_description", "access_tool_name", "icon_url" },
                values: new object[] { "Create and customize maps with OSM data", "Map Creation", "/icons/map-create.svg" });

            migrationBuilder.UpdateData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 2,
                columns: new[] { "access_tool_description", "access_tool_name", "icon_url" },
                values: new object[] { "Upload GeoJSON, KML, and CSV files (max 50MB)", "Data Import", "/icons/data-import.svg" });

            migrationBuilder.UpdateData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 3,
                columns: new[] { "access_tool_description", "access_tool_name", "icon_url" },
                values: new object[] { "Export maps in PDF, PNG, SVG, GeoJSON, MBTiles formats", "Export System", "/icons/export.svg" });

            migrationBuilder.UpdateData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 4,
                columns: new[] { "access_tool_description", "access_tool_name", "icon_url", "required_membership" },
                values: new object[] { "Advanced map analytics and reporting", "Advanced Analytics", "/icons/analytics.svg", true });

            migrationBuilder.UpdateData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 5,
                columns: new[] { "access_tool_description", "access_tool_name", "icon_url", "required_membership" },
                values: new object[] { "Share maps and collaborate with team members", "Team Collaboration", "/icons/collaboration.svg", true });

            migrationBuilder.UpdateData(
                table: "access_tools",
                keyColumn: "access_tool_id",
                keyValue: 6,
                columns: new[] { "access_tool_description", "access_tool_name", "icon_url", "required_membership" },
                values: new object[] { "Access to REST API for integration", "API Access", "/icons/api.svg", true });

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 1,
                column: "access_tool_ids",
                value: "[1, 2, 3]");

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 2,
                columns: new[] { "access_tool_ids", "description", "export_quota", "features", "map_quota", "max_custom_layers", "max_locations_per_org", "max_maps_per_month", "max_organizations", "max_users_per_org", "plan_name", "price_monthly", "priority_support" },
                values: new object[] { "[1, 2, 3, 4, 5]", "Essential features for small teams", 50, "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true}", 50, 10, 5, 25, 2, 5, "Basic", 9.99m, false });

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "plan_id",
                keyValue: 3,
                columns: new[] { "access_tool_ids", "description", "export_quota", "features", "map_quota", "max_custom_layers", "max_locations_per_org", "max_maps_per_month", "max_organizations", "max_users_per_org", "plan_name", "price_monthly" },
                values: new object[] { "[1, 2, 3, 4, 5, 6, 7]", "Advanced features for growing businesses", 200, "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"analytics\": true, \"version_history\": true}", 200, 50, 20, 100, 5, 20, "Pro", 29.99m });

            migrationBuilder.InsertData(
                table: "plans",
                columns: new[] { "plan_id", "access_tool_ids", "created_at", "description", "duration_months", "export_quota", "features", "is_active", "map_quota", "max_custom_layers", "max_locations_per_org", "max_maps_per_month", "max_organizations", "max_users_per_org", "plan_name", "price_monthly", "priority_support", "updated_at" },
                values: new object[] { 4, "[1, 2, 3, 4, 5, 6, 7, 8, 9, 10]", new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Full-featured solution for large organizations", 1, -1, "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"analytics\": true, \"version_history\": true, \"api_access\": true, \"white_label\": true, \"sso\": true}", true, -1, -1, -1, -1, -1, -1, "Enterprise", 99.99m, true, null });
        }
    }
}
