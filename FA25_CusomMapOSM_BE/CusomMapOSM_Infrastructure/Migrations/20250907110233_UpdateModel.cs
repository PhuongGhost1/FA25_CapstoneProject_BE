using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "map_layers");

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

            migrationBuilder.AddColumn<string>(
                name: "custom_style",
                table: "layers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "data_bounds",
                table: "layers",
                type: "text",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "data_size_kb",
                table: "layers",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "feature_count",
                table: "layers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "filter_config",
                table: "layers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "is_visible",
                table: "layers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "layer_order",
                table: "layers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "map_id",
                table: "layers",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "z_index",
                table: "layers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_layers_map_id",
                table: "layers",
                column: "map_id");

            migrationBuilder.AddForeignKey(
                name: "FK_layers_maps_map_id",
                table: "layers",
                column: "map_id",
                principalTable: "maps",
                principalColumn: "map_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_maps_layers_map_id",
                table: "maps",
                column: "map_id",
                principalTable: "layers",
                principalColumn: "layer_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_layers_maps_map_id",
                table: "layers");

            migrationBuilder.DropForeignKey(
                name: "FK_maps_layers_map_id",
                table: "maps");

            migrationBuilder.DropIndex(
                name: "IX_layers_map_id",
                table: "layers");

            migrationBuilder.DropColumn(
                name: "custom_style",
                table: "layers");

            migrationBuilder.DropColumn(
                name: "data_bounds",
                table: "layers");

            migrationBuilder.DropColumn(
                name: "data_size_kb",
                table: "layers");

            migrationBuilder.DropColumn(
                name: "feature_count",
                table: "layers");

            migrationBuilder.DropColumn(
                name: "filter_config",
                table: "layers");

            migrationBuilder.DropColumn(
                name: "is_visible",
                table: "layers");

            migrationBuilder.DropColumn(
                name: "layer_order",
                table: "layers");

            migrationBuilder.DropColumn(
                name: "map_id",
                table: "layers");

            migrationBuilder.DropColumn(
                name: "z_index",
                table: "layers");

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
                name: "map_layers",
                columns: table => new
                {
                    map_layer_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    layer_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    custom_style = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data_bounds = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data_size_kb = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    feature_count = table.Column<int>(type: "int", nullable: true),
                    filter_config = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_visible = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    layer_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    z_index = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_map_layers", x => x.map_layer_id);
                    table.ForeignKey(
                        name: "FK_map_layers_layers_layer_id",
                        column: x => x.layer_id,
                        principalTable: "layers",
                        principalColumn: "layer_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_map_layers_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_map_layers_layer_id",
                table: "map_layers",
                column: "layer_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_layers_map_id",
                table: "map_layers",
                column: "map_id");
        }
    }
}
