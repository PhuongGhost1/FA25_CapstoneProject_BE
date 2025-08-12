using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MIGRATION_NAME : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_map_histories_map_id",
                table: "map_histories",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_histories_user_id",
                table: "map_histories",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_data_source_bookmarks_user_id",
                table: "data_source_bookmarks",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_data_source_bookmarks_users_user_id",
                table: "data_source_bookmarks",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_map_histories_maps_map_id",
                table: "map_histories",
                column: "map_id",
                principalTable: "maps",
                principalColumn: "map_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_map_histories_users_user_id",
                table: "map_histories",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_data_source_bookmarks_users_user_id",
                table: "data_source_bookmarks");

            migrationBuilder.DropForeignKey(
                name: "FK_map_histories_maps_map_id",
                table: "map_histories");

            migrationBuilder.DropForeignKey(
                name: "FK_map_histories_users_user_id",
                table: "map_histories");

            migrationBuilder.DropIndex(
                name: "IX_map_histories_map_id",
                table: "map_histories");

            migrationBuilder.DropIndex(
                name: "IX_map_histories_user_id",
                table: "map_histories");

            migrationBuilder.DropIndex(
                name: "IX_data_source_bookmarks_user_id",
                table: "data_source_bookmarks");
        }
    }
}
