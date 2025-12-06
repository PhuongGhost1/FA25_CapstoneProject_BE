using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_animation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "auto_play",
                table: "animated_layers");

            migrationBuilder.DropColumn(
                name: "css_filter",
                table: "animated_layers");

            migrationBuilder.DropColumn(
                name: "enable_click",
                table: "animated_layers");

            migrationBuilder.DropColumn(
                name: "end_time_ms",
                table: "animated_layers");

            migrationBuilder.DropColumn(
                name: "entry_delay_ms",
                table: "animated_layers");

            migrationBuilder.DropColumn(
                name: "entry_duration_ms",
                table: "animated_layers");

            migrationBuilder.DropColumn(
                name: "entry_effect",
                table: "animated_layers");

            migrationBuilder.DropColumn(
                name: "exit_delay_ms",
                table: "animated_layers");

            migrationBuilder.DropColumn(
                name: "exit_duration_ms",
                table: "animated_layers");

            migrationBuilder.DropColumn(
                name: "exit_effect",
                table: "animated_layers");

            migrationBuilder.DropColumn(
                name: "loop",
                table: "animated_layers");

            migrationBuilder.DropColumn(
                name: "on_click_action",
                table: "animated_layers");

            migrationBuilder.DropColumn(
                name: "playback_speed",
                table: "animated_layers");

            migrationBuilder.DropColumn(
                name: "start_time_ms",
                table: "animated_layers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "auto_play",
                table: "animated_layers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "css_filter",
                table: "animated_layers",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "enable_click",
                table: "animated_layers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "end_time_ms",
                table: "animated_layers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "entry_delay_ms",
                table: "animated_layers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "entry_duration_ms",
                table: "animated_layers",
                type: "int",
                nullable: false,
                defaultValue: 400);

            migrationBuilder.AddColumn<string>(
                name: "entry_effect",
                table: "animated_layers",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "fade")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "exit_delay_ms",
                table: "animated_layers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "exit_duration_ms",
                table: "animated_layers",
                type: "int",
                nullable: false,
                defaultValue: 400);

            migrationBuilder.AddColumn<string>(
                name: "exit_effect",
                table: "animated_layers",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "fade")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "loop",
                table: "animated_layers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "on_click_action",
                table: "animated_layers",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "playback_speed",
                table: "animated_layers",
                type: "int",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddColumn<int>(
                name: "start_time_ms",
                table: "animated_layers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
