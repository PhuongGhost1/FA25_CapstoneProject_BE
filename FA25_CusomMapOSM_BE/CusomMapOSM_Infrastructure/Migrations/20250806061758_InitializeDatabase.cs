using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitializeDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "access_tools",
                columns: table => new
                {
                    access_tool_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    access_tool_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    access_tool_description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
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
                name: "account_statuses",
                columns: table => new
                {
                    status_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_statuses", x => x.status_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "advertisements",
                columns: table => new
                {
                    advertisement_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    advertisement_title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    advertisement_content = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    start_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_advertisements", x => x.advertisement_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "annotation_types",
                columns: table => new
                {
                    type_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    type_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_annotation_types", x => x.type_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "collaboration_permissions",
                columns: table => new
                {
                    permission_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    permission_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    level_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collaboration_permissions", x => x.permission_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "collaboration_target_types",
                columns: table => new
                {
                    target_type_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    type_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collaboration_target_types", x => x.target_type_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "data_source_bookmarks",
                columns: table => new
                {
                    data_source_bookmark_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    osm_query = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_source_bookmarks", x => x.data_source_bookmark_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "export_types",
                columns: table => new
                {
                    type_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_export_types", x => x.type_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "faqs",
                columns: table => new
                {
                    faq_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    question = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    answer = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faqs", x => x.faq_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "layer_sources",
                columns: table => new
                {
                    source_type_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_layer_sources", x => x.source_type_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "layer_types",
                columns: table => new
                {
                    layer_type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    type_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icon_url = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_layer_types", x => x.layer_type_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "map_histories",
                columns: table => new
                {
                    version_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    snapshot_data = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_map_histories", x => x.version_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "membership_statuses",
                columns: table => new
                {
                    status_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membership_statuses", x => x.status_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "organization_location_statuses",
                columns: table => new
                {
                    status_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_location_statuses", x => x.status_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "organization_member_types",
                columns: table => new
                {
                    type_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_member_types", x => x.type_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "payment_gateways",
                columns: table => new
                {
                    gateway_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_gateways", x => x.gateway_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "plans",
                columns: table => new
                {
                    plan_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    plan_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    price_monthly = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    duration_months = table.Column<int>(type: "int", nullable: false),
                    max_organizations = table.Column<int>(type: "int", nullable: false),
                    max_locations_per_org = table.Column<int>(type: "int", nullable: false),
                    max_maps_per_month = table.Column<int>(type: "int", nullable: false),
                    max_users_per_org = table.Column<int>(type: "int", nullable: false),
                    map_quota = table.Column<int>(type: "int", nullable: false),
                    export_quota = table.Column<int>(type: "int", nullable: false),
                    max_custom_layers = table.Column<int>(type: "int", nullable: false),
                    priority_support = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    features = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plans", x => x.plan_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ticket_statuses",
                columns: table => new
                {
                    status_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket_statuses", x => x.status_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.role_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    full_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountStatusId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastLogin = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_users_account_statuses_AccountStatusId",
                        column: x => x.AccountStatusId,
                        principalTable: "account_statuses",
                        principalColumn: "status_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_user_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "user_roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "collaborations",
                columns: table => new
                {
                    collaboration_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    target_type_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    target_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    permission_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    invited_by = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collaborations", x => x.collaboration_id);
                    table.ForeignKey(
                        name: "FK_collaborations_collaboration_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "collaboration_permissions",
                        principalColumn: "permission_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_collaborations_collaboration_target_types_target_type_id",
                        column: x => x.target_type_id,
                        principalTable: "collaboration_target_types",
                        principalColumn: "target_type_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_collaborations_users_invited_by",
                        column: x => x.invited_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_collaborations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "layers",
                columns: table => new
                {
                    layer_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    layer_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    layer_type_id = table.Column<int>(type: "int", nullable: false),
                    source_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    file_path = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    layer_data = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    layer_style = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_public = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_layers", x => x.layer_id);
                    table.ForeignKey(
                        name: "FK_layers_layer_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "layer_sources",
                        principalColumn: "source_type_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_layers_layer_types_layer_type_id",
                        column: x => x.layer_type_id,
                        principalTable: "layer_types",
                        principalColumn: "layer_type_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_layers_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "map_templates",
                columns: table => new
                {
                    template_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    template_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    preview_image = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    default_bounds = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    template_config = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    base_layer = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, defaultValue: "osm")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    initial_layers = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    view_state = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_public = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    is_featured = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    usage_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    created_by = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_map_templates", x => x.template_id);
                    table.ForeignKey(
                        name: "FK_map_templates_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    notification_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    message = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    sent_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.notification_id);
                    table.ForeignKey(
                        name: "FK_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "organizations",
                columns: table => new
                {
                    org_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    org_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    abbreviation = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    logo_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contact_email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contact_phone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    owner_user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.org_id);
                    table.ForeignKey(
                        name: "FK_organizations_users_owner_user_id",
                        column: x => x.owner_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "support_tickets",
                columns: table => new
                {
                    ticket_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    subject = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    message = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    priority = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValue: "low")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    resolved_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_support_tickets", x => x.ticket_id);
                    table.ForeignKey(
                        name: "FK_support_tickets_ticket_statuses_status_id",
                        column: x => x.status_id,
                        principalTable: "ticket_statuses",
                        principalColumn: "status_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_support_tickets_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_access_tools",
                columns: table => new
                {
                    user_access_tool_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccessToolId = table.Column<int>(type: "int", nullable: false),
                    granted_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "user_preferences",
                columns: table => new
                {
                    user_preference_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    language = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false, defaultValue: "en")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    default_map_style = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValue: "default")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    measurement_unit = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false, defaultValue: "metric")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_preferences", x => x.user_preference_id);
                    table.ForeignKey(
                        name: "FK_user_preferences_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_favorite_templates",
                columns: table => new
                {
                    user_favorite_template_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    favorite_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_favorite_templates", x => x.user_favorite_template_id);
                    table.ForeignKey(
                        name: "FK_user_favorite_templates_map_templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "map_templates",
                        principalColumn: "template_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_favorite_templates_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "maps",
                columns: table => new
                {
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    org_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    map_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    geographic_bounds = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    map_config = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    base_layer = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, defaultValue: "osm")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    view_state = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    preview_image = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_public = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    template_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maps", x => x.map_id);
                    table.ForeignKey(
                        name: "FK_maps_map_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "map_templates",
                        principalColumn: "template_id");
                    table.ForeignKey(
                        name: "FK_maps_organizations_org_id",
                        column: x => x.org_id,
                        principalTable: "organizations",
                        principalColumn: "org_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_maps_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "memberships",
                columns: table => new
                {
                    membership_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    org_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    plan_id = table.Column<int>(type: "int", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    status_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    auto_renew = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    current_usage = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    last_reset_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memberships", x => x.membership_id);
                    table.ForeignKey(
                        name: "FK_memberships_membership_statuses_status_id",
                        column: x => x.status_id,
                        principalTable: "membership_statuses",
                        principalColumn: "status_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_memberships_organizations_org_id",
                        column: x => x.org_id,
                        principalTable: "organizations",
                        principalColumn: "org_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_memberships_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "plan_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_memberships_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "organization_locations",
                columns: table => new
                {
                    location_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    org_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    location_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    latitude = table.Column<decimal>(type: "decimal(10,6)", nullable: true),
                    longitude = table.Column<decimal>(type: "decimal(10,6)", nullable: true),
                    phone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    website = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    operating_hours = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    services = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    categories = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    amenities = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    photos = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    social_media = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    verified = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    last_verified_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_locations", x => x.location_id);
                    table.ForeignKey(
                        name: "FK_organization_locations_organization_location_statuses_status~",
                        column: x => x.status_id,
                        principalTable: "organization_location_statuses",
                        principalColumn: "status_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_organization_locations_organizations_org_id",
                        column: x => x.org_id,
                        principalTable: "organizations",
                        principalColumn: "org_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "organization_members",
                columns: table => new
                {
                    member_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    org_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    role_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    invited_by = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    joined_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_members", x => x.member_id);
                    table.ForeignKey(
                        name: "FK_organization_members_organization_member_types_role_id",
                        column: x => x.role_id,
                        principalTable: "organization_member_types",
                        principalColumn: "type_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_organization_members_organizations_org_id",
                        column: x => x.org_id,
                        principalTable: "organizations",
                        principalColumn: "org_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organization_members_users_invited_by",
                        column: x => x.invited_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_organization_members_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "annotations",
                columns: table => new
                {
                    annotation_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    type_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    geometry = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    properties = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_annotations", x => x.annotation_id);
                    table.ForeignKey(
                        name: "FK_annotations_annotation_types_type_id",
                        column: x => x.type_id,
                        principalTable: "annotation_types",
                        principalColumn: "type_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_annotations_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bookmarks",
                columns: table => new
                {
                    bookmark_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    view_state = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookmarks", x => x.bookmark_id);
                    table.ForeignKey(
                        name: "FK_bookmarks_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bookmarks_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "comments",
                columns: table => new
                {
                    comment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    layer_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    content = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    position = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comments", x => x.comment_id);
                    table.ForeignKey(
                        name: "FK_comments_layers_layer_id",
                        column: x => x.layer_id,
                        principalTable: "layers",
                        principalColumn: "layer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_comments_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_comments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "map_layers",
                columns: table => new
                {
                    map_layer_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    layer_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    is_visible = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    z_index = table.Column<int>(type: "int", nullable: false),
                    layer_order = table.Column<int>(type: "int", nullable: false),
                    custom_style = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    filter_config = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "exports",
                columns: table => new
                {
                    export_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MembershipId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MapId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    file_path = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_size = table.Column<int>(type: "int", nullable: false),
                    ExportTypeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    quota_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exports", x => x.export_id);
                    table.ForeignKey(
                        name: "FK_exports_export_types_ExportTypeId",
                        column: x => x.ExportTypeId,
                        principalTable: "export_types",
                        principalColumn: "type_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exports_maps_MapId",
                        column: x => x.MapId,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exports_memberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "memberships",
                        principalColumn: "membership_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exports_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    transaction_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    payment_gateway_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    transaction_reference = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValue: "pending")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    transaction_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    membership_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    export_id = table.Column<int>(type: "int", nullable: true),
                    purpose = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.transaction_id);
                    table.ForeignKey(
                        name: "FK_transactions_exports_export_id",
                        column: x => x.export_id,
                        principalTable: "exports",
                        principalColumn: "export_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_transactions_memberships_membership_id",
                        column: x => x.membership_id,
                        principalTable: "memberships",
                        principalColumn: "membership_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_transactions_payment_gateways_payment_gateway_id",
                        column: x => x.payment_gateway_id,
                        principalTable: "payment_gateways",
                        principalColumn: "gateway_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "access_tools",
                columns: new[] { "access_tool_id", "access_tool_description", "access_tool_name", "icon_url", "required_membership" },
                values: new object[,]
                {
                    { 1, "Create and customize maps with OSM data", "Map Creation", "/icons/map-create.svg", false },
                    { 2, "Upload GeoJSON, KML, and CSV files (max 50MB)", "Data Import", "/icons/data-import.svg", false },
                    { 3, "Export maps in PDF, PNG, SVG, GeoJSON, MBTiles formats", "Export System", "/icons/export.svg", false },
                    { 4, "Advanced map analytics and reporting", "Advanced Analytics", "/icons/analytics.svg", true },
                    { 5, "Share maps and collaborate with team members", "Team Collaboration", "/icons/collaboration.svg", true },
                    { 6, "Access to REST API for integration", "API Access", "/icons/api.svg", true }
                });

            migrationBuilder.InsertData(
                table: "account_statuses",
                columns: new[] { "status_id", "name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000004"), "Active" },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "Inactive" },
                    { new Guid("00000000-0000-0000-0000-000000000006"), "Suspended" },
                    { new Guid("00000000-0000-0000-0000-000000000007"), "Pending Verification" }
                });

            migrationBuilder.InsertData(
                table: "annotation_types",
                columns: new[] { "type_id", "type_name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000008"), "Marker" },
                    { new Guid("00000000-0000-0000-0000-000000000009"), "Line" },
                    { new Guid("00000000-0000-0000-0000-000000000010"), "Polygon" },
                    { new Guid("00000000-0000-0000-0000-000000000011"), "Circle" },
                    { new Guid("00000000-0000-0000-0000-000000000012"), "Rectangle" },
                    { new Guid("00000000-0000-0000-0000-000000000013"), "Text Label" }
                });

            migrationBuilder.InsertData(
                table: "collaboration_permissions",
                columns: new[] { "permission_id", "created_at", "description", "is_active", "level_order", "permission_name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000014"), new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Can view maps and layers", true, 1, "View" },
                    { new Guid("00000000-0000-0000-0000-000000000015"), new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Can edit maps and layers", true, 2, "Edit" },
                    { new Guid("00000000-0000-0000-0000-000000000016"), new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Can manage maps, layers, and permissions", true, 3, "Manage" }
                });

            migrationBuilder.InsertData(
                table: "collaboration_target_types",
                columns: new[] { "target_type_id", "created_at", "description", "is_active", "type_name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000017"), new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Share entire maps with team members", true, "Map" },
                    { new Guid("00000000-0000-0000-0000-000000000018"), new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Share specific layers with team members", true, "Layer" },
                    { new Guid("00000000-0000-0000-0000-000000000019"), new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Share organization resources", true, "Organization" }
                });

            migrationBuilder.InsertData(
                table: "export_types",
                columns: new[] { "type_id", "name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000020"), "PDF" },
                    { new Guid("00000000-0000-0000-0000-000000000021"), "PNG" },
                    { new Guid("00000000-0000-0000-0000-000000000022"), "SVG" },
                    { new Guid("00000000-0000-0000-0000-000000000023"), "GeoJSON" },
                    { new Guid("00000000-0000-0000-0000-000000000024"), "MBTiles" }
                });

            migrationBuilder.InsertData(
                table: "faqs",
                columns: new[] { "faq_id", "answer", "category", "created_at", "question" },
                values: new object[,]
                {
                    { 1, "To create a map, log in to your account, click 'Create New Map', select your desired OSM area using the bounding box tool, add layers like roads, buildings, and POIs, then customize the styling to your preference.", "Map Creation", new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "How do I create a map?" },
                    { 2, "You can upload GeoJSON, KML, and CSV files up to 50MB in size. Make sure your CSV files contain coordinate columns for proper mapping.", "Data Management", new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "What file formats can I upload?" },
                    { 3, "You can export your maps in PDF, PNG, SVG, GeoJSON, and MBTiles formats. Resolution options range from 72 to 300 DPI depending on your subscription plan.", "Export System", new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "What export formats are available?" },
                    { 4, "Use the collaboration feature to share maps and layers with team members. You can set permissions for view, edit, or manage access levels.", "Collaboration", new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "How do I share maps with my team?" },
                    { 5, "We accept payments through VNPay, PayPal, and bank transfers. All transactions are secured with PCI-DSS compliance.", "Billing", new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "What payment methods are accepted?" },
                    { 6, "CustomMapOSM is compatible with Chrome, Firefox, and Edge browsers. For best performance, we recommend using the latest version of these browsers.", "Technical", new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "What browsers are supported?" }
                });

            migrationBuilder.InsertData(
                table: "layer_sources",
                columns: new[] { "source_type_id", "name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000025"), "OpenStreetMap" },
                    { new Guid("00000000-0000-0000-0000-000000000026"), "User Upload" },
                    { new Guid("00000000-0000-0000-0000-000000000027"), "External API" },
                    { new Guid("00000000-0000-0000-0000-000000000028"), "Database" },
                    { new Guid("00000000-0000-0000-0000-000000000029"), "Web Service" }
                });

            migrationBuilder.InsertData(
                table: "layer_types",
                columns: new[] { "layer_type_id", "created_at", "description", "icon_url", "is_active", "type_name" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Street and road networks from OpenStreetMap", "/icons/roads.svg", true, "Roads" },
                    { 2, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Building footprints and structures", "/icons/buildings.svg", true, "Buildings" },
                    { 3, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Points of Interest including amenities and landmarks", "/icons/poi.svg", true, "POIs" },
                    { 4, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "User uploaded GeoJSON data layers", "/icons/geojson.svg", true, "GeoJSON" },
                    { 5, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "User uploaded KML data layers", "/icons/kml.svg", true, "KML" },
                    { 6, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "User uploaded CSV data with coordinates", "/icons/csv.svg", true, "CSV" }
                });

            migrationBuilder.InsertData(
                table: "membership_statuses",
                columns: new[] { "status_id", "name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000030"), "Active" },
                    { new Guid("00000000-0000-0000-0000-000000000031"), "Expired" },
                    { new Guid("00000000-0000-0000-0000-000000000032"), "Suspended" },
                    { new Guid("00000000-0000-0000-0000-000000000033"), "Pending Payment" },
                    { new Guid("00000000-0000-0000-0000-000000000034"), "Cancelled" }
                });

            migrationBuilder.InsertData(
                table: "organization_location_statuses",
                columns: new[] { "status_id", "name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000035"), "Active" },
                    { new Guid("00000000-0000-0000-0000-000000000036"), "Inactive" },
                    { new Guid("00000000-0000-0000-0000-000000000037"), "Under Construction" },
                    { new Guid("00000000-0000-0000-0000-000000000038"), "Temporary Closed" }
                });

            migrationBuilder.InsertData(
                table: "organization_member_types",
                columns: new[] { "type_id", "name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000039"), "Owner" },
                    { new Guid("00000000-0000-0000-0000-000000000040"), "Admin" },
                    { new Guid("00000000-0000-0000-0000-000000000041"), "Member" },
                    { new Guid("00000000-0000-0000-0000-000000000042"), "Viewer" }
                });

            migrationBuilder.InsertData(
                table: "payment_gateways",
                columns: new[] { "gateway_id", "name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000048"), "VNPay" },
                    { new Guid("00000000-0000-0000-0000-000000000049"), "PayPal" },
                    { new Guid("00000000-0000-0000-0000-000000000050"), "Stripe" },
                    { new Guid("00000000-0000-0000-0000-000000000051"), "Bank Transfer" }
                });

            migrationBuilder.InsertData(
                table: "plans",
                columns: new[] { "plan_id", "created_at", "description", "duration_months", "export_quota", "features", "is_active", "map_quota", "max_custom_layers", "max_locations_per_org", "max_maps_per_month", "max_organizations", "max_users_per_org", "plan_name", "price_monthly", "priority_support", "updated_at" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Basic features for individual users", 1, 5, "{\"templates\": true, \"basic_export\": true, \"public_maps\": true}", true, 10, 3, 1, 5, 1, 1, "Free", 0.00m, false, null },
                    { 2, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Essential features for small teams", 1, 50, "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true}", true, 50, 10, 5, 25, 2, 5, "Basic", 9.99m, false, null },
                    { 3, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Advanced features for growing businesses", 1, 200, "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"analytics\": true, \"version_history\": true}", true, 200, 50, 20, 100, 5, 20, "Pro", 29.99m, true, null },
                    { 4, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Full-featured solution for large organizations", 1, -1, "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"analytics\": true, \"version_history\": true, \"api_access\": true, \"white_label\": true, \"sso\": true}", true, -1, -1, -1, -1, -1, -1, "Enterprise", 99.99m, true, null }
                });

            migrationBuilder.InsertData(
                table: "ticket_statuses",
                columns: new[] { "status_id", "name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000043"), "Open" },
                    { new Guid("00000000-0000-0000-0000-000000000044"), "In Progress" },
                    { new Guid("00000000-0000-0000-0000-000000000045"), "Waiting for Customer" },
                    { new Guid("00000000-0000-0000-0000-000000000046"), "Resolved" },
                    { new Guid("00000000-0000-0000-0000-000000000047"), "Closed" }
                });

            migrationBuilder.InsertData(
                table: "user_roles",
                columns: new[] { "role_id", "name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "Staff" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "Registered User" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "Administrator" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_annotations_map_id",
                table: "annotations",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_annotations_type_id",
                table: "annotations",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_bookmarks_map_id",
                table: "bookmarks",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_bookmarks_user_id",
                table: "bookmarks",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_collaborations_invited_by",
                table: "collaborations",
                column: "invited_by");

            migrationBuilder.CreateIndex(
                name: "IX_collaborations_permission_id",
                table: "collaborations",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "IX_collaborations_target_type_id",
                table: "collaborations",
                column: "target_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_collaborations_user_id",
                table: "collaborations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_comments_layer_id",
                table: "comments",
                column: "layer_id");

            migrationBuilder.CreateIndex(
                name: "IX_comments_map_id",
                table: "comments",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_comments_user_id",
                table: "comments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_exports_ExportTypeId",
                table: "exports",
                column: "ExportTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_exports_MapId",
                table: "exports",
                column: "MapId");

            migrationBuilder.CreateIndex(
                name: "IX_exports_MembershipId",
                table: "exports",
                column: "MembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_exports_UserId",
                table: "exports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_layers_layer_type_id",
                table: "layers",
                column: "layer_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_layers_source_id",
                table: "layers",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_layers_user_id",
                table: "layers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_layers_layer_id",
                table: "map_layers",
                column: "layer_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_layers_map_id",
                table: "map_layers",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_templates_created_by",
                table: "map_templates",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_maps_org_id",
                table: "maps",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_maps_template_id",
                table: "maps",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_maps_user_id",
                table: "maps",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_memberships_org_id",
                table: "memberships",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_memberships_plan_id",
                table: "memberships",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_memberships_status_id",
                table: "memberships",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_memberships_user_id",
                table: "memberships",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_locations_org_id",
                table: "organization_locations",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_locations_status_id",
                table: "organization_locations",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_members_invited_by",
                table: "organization_members",
                column: "invited_by");

            migrationBuilder.CreateIndex(
                name: "IX_organization_members_org_id",
                table: "organization_members",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_members_role_id",
                table: "organization_members",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_members_user_id",
                table: "organization_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_owner_user_id",
                table: "organizations",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_support_tickets_status_id",
                table: "support_tickets",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_support_tickets_user_id",
                table: "support_tickets",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_export_id",
                table: "transactions",
                column: "export_id");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_membership_id",
                table: "transactions",
                column: "membership_id");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_payment_gateway_id",
                table: "transactions",
                column: "payment_gateway_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_access_tools_AccessToolId",
                table: "user_access_tools",
                column: "AccessToolId");

            migrationBuilder.CreateIndex(
                name: "IX_user_access_tools_UserId",
                table: "user_access_tools",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_favorite_templates_TemplateId",
                table: "user_favorite_templates",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_user_favorite_templates_UserId",
                table: "user_favorite_templates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_preferences_UserId",
                table: "user_preferences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_AccountStatusId",
                table: "users",
                column: "AccountStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_users_RoleId",
                table: "users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "advertisements");

            migrationBuilder.DropTable(
                name: "annotations");

            migrationBuilder.DropTable(
                name: "bookmarks");

            migrationBuilder.DropTable(
                name: "collaborations");

            migrationBuilder.DropTable(
                name: "comments");

            migrationBuilder.DropTable(
                name: "data_source_bookmarks");

            migrationBuilder.DropTable(
                name: "faqs");

            migrationBuilder.DropTable(
                name: "map_histories");

            migrationBuilder.DropTable(
                name: "map_layers");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "organization_locations");

            migrationBuilder.DropTable(
                name: "organization_members");

            migrationBuilder.DropTable(
                name: "support_tickets");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "user_access_tools");

            migrationBuilder.DropTable(
                name: "user_favorite_templates");

            migrationBuilder.DropTable(
                name: "user_preferences");

            migrationBuilder.DropTable(
                name: "annotation_types");

            migrationBuilder.DropTable(
                name: "collaboration_permissions");

            migrationBuilder.DropTable(
                name: "collaboration_target_types");

            migrationBuilder.DropTable(
                name: "layers");

            migrationBuilder.DropTable(
                name: "organization_location_statuses");

            migrationBuilder.DropTable(
                name: "organization_member_types");

            migrationBuilder.DropTable(
                name: "ticket_statuses");

            migrationBuilder.DropTable(
                name: "exports");

            migrationBuilder.DropTable(
                name: "payment_gateways");

            migrationBuilder.DropTable(
                name: "access_tools");

            migrationBuilder.DropTable(
                name: "layer_sources");

            migrationBuilder.DropTable(
                name: "layer_types");

            migrationBuilder.DropTable(
                name: "export_types");

            migrationBuilder.DropTable(
                name: "maps");

            migrationBuilder.DropTable(
                name: "memberships");

            migrationBuilder.DropTable(
                name: "map_templates");

            migrationBuilder.DropTable(
                name: "membership_statuses");

            migrationBuilder.DropTable(
                name: "organizations");

            migrationBuilder.DropTable(
                name: "plans");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "account_statuses");

            migrationBuilder.DropTable(
                name: "user_roles");
        }
    }
}
