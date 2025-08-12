using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFailedEmailsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FailedEmails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ToEmail = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subject = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Body = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailData = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FailureReason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    LastRetryAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailedEmails", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "organization_invitation",
                columns: table => new
                {
                    invite_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    org_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    member_email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    invited_by = table.Column<Guid>(type: "char(50)", maxLength: 50, nullable: false, collation: "ascii_general_ci"),
                    role_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    invited_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    is_accepted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    accepted_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_invitation", x => x.invite_id);
                    table.ForeignKey(
                        name: "FK_organization_invitation_organization_member_types_role_id",
                        column: x => x.role_id,
                        principalTable: "organization_member_types",
                        principalColumn: "type_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_organization_invitation_organizations_org_id",
                        column: x => x.org_id,
                        principalTable: "organizations",
                        principalColumn: "org_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organization_invitation_users_invited_by",
                        column: x => x.invited_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FailedEmails_CreatedAt",
                table: "FailedEmails",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FailedEmails_Status",
                table: "FailedEmails",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FailedEmails_Status_RetryCount",
                table: "FailedEmails",
                columns: new[] { "Status", "RetryCount" });

            migrationBuilder.CreateIndex(
                name: "IX_FailedEmails_ToEmail",
                table: "FailedEmails",
                column: "ToEmail");

            migrationBuilder.CreateIndex(
                name: "IX_organization_invitation_invited_by",
                table: "organization_invitation",
                column: "invited_by");

            migrationBuilder.CreateIndex(
                name: "IX_organization_invitation_org_id",
                table: "organization_invitation",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_invitation_role_id",
                table: "organization_invitation",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FailedEmails");

            migrationBuilder.DropTable(
                name: "organization_invitation");
        }
    }
}
