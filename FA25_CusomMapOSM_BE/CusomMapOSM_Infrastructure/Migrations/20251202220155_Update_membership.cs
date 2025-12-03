using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_membership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_membership_usages_membership_id",
                table: "membership_usages",
                column: "membership_id");

            migrationBuilder.CreateIndex(
                name: "IX_membership_usages_org_id",
                table: "membership_usages",
                column: "org_id");

            migrationBuilder.AddForeignKey(
                name: "FK_membership_usages_memberships_membership_id",
                table: "membership_usages",
                column: "membership_id",
                principalTable: "memberships",
                principalColumn: "membership_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_membership_usages_organizations_org_id",
                table: "membership_usages",
                column: "org_id",
                principalTable: "organizations",
                principalColumn: "org_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_membership_usages_memberships_membership_id",
                table: "membership_usages");

            migrationBuilder.DropForeignKey(
                name: "FK_membership_usages_organizations_org_id",
                table: "membership_usages");

            migrationBuilder.DropIndex(
                name: "IX_membership_usages_membership_id",
                table: "membership_usages");

            migrationBuilder.DropIndex(
                name: "IX_membership_usages_org_id",
                table: "membership_usages");
        }
    }
}
