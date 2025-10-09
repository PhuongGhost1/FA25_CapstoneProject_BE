using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase_FixTransactionMembershipRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_transactions_memberships_membership_id",
                table: "transactions");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "organizations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationOrgId",
                table: "organization_members",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

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

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationOrgId",
                table: "memberships",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "PlanId1",
                table: "memberships",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "memberships",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_organization_members_OrganizationOrgId",
                table: "organization_members",
                column: "OrganizationOrgId");

            migrationBuilder.CreateIndex(
                name: "IX_memberships_OrganizationOrgId",
                table: "memberships",
                column: "OrganizationOrgId");

            migrationBuilder.CreateIndex(
                name: "IX_memberships_PlanId1",
                table: "memberships",
                column: "PlanId1");

            migrationBuilder.CreateIndex(
                name: "IX_memberships_UserId1",
                table: "memberships",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_memberships_organizations_OrganizationOrgId",
                table: "memberships",
                column: "OrganizationOrgId",
                principalTable: "organizations",
                principalColumn: "org_id");

            migrationBuilder.AddForeignKey(
                name: "FK_memberships_plans_PlanId1",
                table: "memberships",
                column: "PlanId1",
                principalTable: "plans",
                principalColumn: "plan_id");

            migrationBuilder.AddForeignKey(
                name: "FK_memberships_users_UserId1",
                table: "memberships",
                column: "UserId1",
                principalTable: "users",
                principalColumn: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_organization_members_organizations_OrganizationOrgId",
                table: "organization_members",
                column: "OrganizationOrgId",
                principalTable: "organizations",
                principalColumn: "org_id");

            migrationBuilder.AddForeignKey(
                name: "FK_transactions_memberships_membership_id",
                table: "transactions",
                column: "membership_id",
                principalTable: "memberships",
                principalColumn: "membership_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_memberships_organizations_OrganizationOrgId",
                table: "memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_memberships_plans_PlanId1",
                table: "memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_memberships_users_UserId1",
                table: "memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_organization_members_organizations_OrganizationOrgId",
                table: "organization_members");

            migrationBuilder.DropForeignKey(
                name: "FK_transactions_memberships_membership_id",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "IX_organization_members_OrganizationOrgId",
                table: "organization_members");

            migrationBuilder.DropIndex(
                name: "IX_memberships_OrganizationOrgId",
                table: "memberships");

            migrationBuilder.DropIndex(
                name: "IX_memberships_PlanId1",
                table: "memberships");

            migrationBuilder.DropIndex(
                name: "IX_memberships_UserId1",
                table: "memberships");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "OrganizationOrgId",
                table: "organization_members");

            migrationBuilder.DropColumn(
                name: "OrganizationOrgId",
                table: "memberships");

            migrationBuilder.DropColumn(
                name: "PlanId1",
                table: "memberships");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "memberships");

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

            migrationBuilder.AddForeignKey(
                name: "FK_transactions_memberships_membership_id",
                table: "transactions",
                column: "membership_id",
                principalTable: "memberships",
                principalColumn: "membership_id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
