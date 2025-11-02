using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class POI_update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_locations_layer_animation_presets_animation_preset_id",
                table: "locations");

            migrationBuilder.DropForeignKey(
                name: "FK_locations_layers_associated_layer_id",
                table: "locations");

            migrationBuilder.DropForeignKey(
                name: "FK_locations_locations_linked_location_id",
                table: "locations");

            migrationBuilder.DropForeignKey(
                name: "FK_locations_segments_segment_id",
                table: "locations");

            migrationBuilder.DropForeignKey(
                name: "FK_locations_users_created_by",
                table: "locations");

            migrationBuilder.DropForeignKey(
                name: "FK_locations_zones_zone_id",
                table: "locations");

            migrationBuilder.DropIndex(
                name: "IX_locations_created_by",
                table: "locations");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorUserId",
                table: "locations",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_locations_CreatorUserId",
                table: "locations",
                column: "CreatorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_locations_layer_animation_presets_animation_preset_id",
                table: "locations",
                column: "animation_preset_id",
                principalTable: "layer_animation_presets",
                principalColumn: "animation_preset_id");

            migrationBuilder.AddForeignKey(
                name: "FK_locations_layers_associated_layer_id",
                table: "locations",
                column: "associated_layer_id",
                principalTable: "layers",
                principalColumn: "layer_id");

            migrationBuilder.AddForeignKey(
                name: "FK_locations_locations_linked_location_id",
                table: "locations",
                column: "linked_location_id",
                principalTable: "locations",
                principalColumn: "location_id");

            migrationBuilder.AddForeignKey(
                name: "FK_locations_segments_segment_id",
                table: "locations",
                column: "segment_id",
                principalTable: "segments",
                principalColumn: "segment_id");

            migrationBuilder.AddForeignKey(
                name: "FK_locations_users_CreatorUserId",
                table: "locations",
                column: "CreatorUserId",
                principalTable: "users",
                principalColumn: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_locations_zones_zone_id",
                table: "locations",
                column: "zone_id",
                principalTable: "zones",
                principalColumn: "zone_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_locations_layer_animation_presets_animation_preset_id",
                table: "locations");

            migrationBuilder.DropForeignKey(
                name: "FK_locations_layers_associated_layer_id",
                table: "locations");

            migrationBuilder.DropForeignKey(
                name: "FK_locations_locations_linked_location_id",
                table: "locations");

            migrationBuilder.DropForeignKey(
                name: "FK_locations_segments_segment_id",
                table: "locations");

            migrationBuilder.DropForeignKey(
                name: "FK_locations_users_CreatorUserId",
                table: "locations");

            migrationBuilder.DropForeignKey(
                name: "FK_locations_zones_zone_id",
                table: "locations");

            migrationBuilder.DropIndex(
                name: "IX_locations_CreatorUserId",
                table: "locations");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "locations");

            migrationBuilder.CreateIndex(
                name: "IX_locations_created_by",
                table: "locations",
                column: "created_by");

            migrationBuilder.AddForeignKey(
                name: "FK_locations_layer_animation_presets_animation_preset_id",
                table: "locations",
                column: "animation_preset_id",
                principalTable: "layer_animation_presets",
                principalColumn: "animation_preset_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_locations_layers_associated_layer_id",
                table: "locations",
                column: "associated_layer_id",
                principalTable: "layers",
                principalColumn: "layer_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_locations_locations_linked_location_id",
                table: "locations",
                column: "linked_location_id",
                principalTable: "locations",
                principalColumn: "location_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_locations_segments_segment_id",
                table: "locations",
                column: "segment_id",
                principalTable: "segments",
                principalColumn: "segment_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_locations_users_created_by",
                table: "locations",
                column: "created_by",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_locations_zones_zone_id",
                table: "locations",
                column: "zone_id",
                principalTable: "zones",
                principalColumn: "zone_id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
