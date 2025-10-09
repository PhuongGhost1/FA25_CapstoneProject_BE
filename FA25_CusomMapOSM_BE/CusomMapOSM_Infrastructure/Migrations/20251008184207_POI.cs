using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class POI : Migration
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
                name: "administrative_zones",
                columns: table => new
                {
                    zone_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    external_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    zone_code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    admin_level = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    parent_zone_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    geometry = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    simplified_geometry = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    centroid = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bounding_box = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    last_synced_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_administrative_zones", x => x.zone_id);
                    table.ForeignKey(
                        name: "FK_administrative_zones_administrative_zones_parent_zone_id",
                        column: x => x.parent_zone_id,
                        principalTable: "administrative_zones",
                        principalColumn: "zone_id",
                        onDelete: ReferentialAction.SetNull);
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
                name: "failed_emails",
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
                    table.PrimaryKey("PK_failed_emails", x => x.Id);
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
                name: "layer_animation_presets",
                columns: table => new
                {
                    animation_preset_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    preset_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    animation_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    default_easing = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    default_duration_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 600),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    config_schema = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_system_preset = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_layer_animation_presets", x => x.animation_preset_id);
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
                name: "membership_addons",
                columns: table => new
                {
                    addon_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    membership_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    org_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    addon_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    quantity = table.Column<int>(type: "int", nullable: true),
                    feature_payload = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    purchased_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    effective_from = table.Column<DateTime>(type: "datetime", nullable: true),
                    effective_until = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membership_addons", x => x.addon_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "membership_usages",
                columns: table => new
                {
                    usage_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    membership_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    org_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    maps_created_this_cycle = table.Column<int>(type: "int", nullable: false),
                    exports_this_cycle = table.Column<int>(type: "int", nullable: false),
                    active_users_in_org = table.Column<int>(type: "int", nullable: false),
                    feature_flags = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cycle_start_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    cycle_end_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membership_usages", x => x.usage_id);
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
                    monthly_tokens = table.Column<int>(type: "int", nullable: false, defaultValue: 10000),
                    priority_support = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    features = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    access_tool_ids = table.Column<string>(type: "json", nullable: true)
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
                name: "zone_insights",
                columns: table => new
                {
                    zone_insight_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    zone_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    insight_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    summary = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_url = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    external_url = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    location = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    metadata = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zone_insights", x => x.zone_insight_id);
                    table.ForeignKey(
                        name: "FK_zone_insights_administrative_zones_zone_id",
                        column: x => x.zone_id,
                        principalTable: "administrative_zones",
                        principalColumn: "zone_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "zone_statistics",
                columns: table => new
                {
                    zone_statistic_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    zone_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    metric_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    numeric_value = table.Column<double>(type: "double", nullable: true),
                    text_value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    unit = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    year = table.Column<int>(type: "int", nullable: true),
                    quarter = table.Column<int>(type: "int", nullable: true),
                    source = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    metadata = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    collected_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zone_statistics", x => x.zone_statistic_id);
                    table.ForeignKey(
                        name: "FK_zone_statistics_administrative_zones_zone_id",
                        column: x => x.zone_id,
                        principalTable: "administrative_zones",
                        principalColumn: "zone_id",
                        onDelete: ReferentialAction.Cascade);
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
                    account_status = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_login = table.Column<DateTime>(type: "datetime", nullable: true),
                    monthly_token_usage = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    last_token_reset = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
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
                    table.ForeignKey(
                        name: "FK_data_source_bookmarks_users_user_id",
                        column: x => x.user_id,
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
                    sent_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    is_read = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    metadata = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
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
                    status = table.Column<int>(type: "int", nullable: false),
                    priority = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValue: "low")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    resolved_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_support_tickets", x => x.ticket_id);
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
                name: "maps",
                columns: table => new
                {
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    org_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    map_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    preview_image = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_template = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    parent_map_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    category = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_featured = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    usage_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    default_bounds = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    base_layer = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, defaultValue: "osm")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    view_state = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_public = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maps", x => x.map_id);
                    table.ForeignKey(
                        name: "FK_maps_maps_parent_map_id",
                        column: x => x.parent_map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_maps_organizations_org_id",
                        column: x => x.org_id,
                        principalTable: "organizations",
                        principalColumn: "org_id",
                        onDelete: ReferentialAction.SetNull);
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
                    status = table.Column<int>(type: "int", nullable: false),
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
                    accepted_at = table.Column<DateTime>(type: "datetime", nullable: true)
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
                name: "layers",
                columns: table => new
                {
                    layer_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    layer_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    layer_type_id = table.Column<int>(type: "int", nullable: false),
                    source_id = table.Column<int>(type: "int", nullable: false),
                    file_path = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    layer_data = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    layer_style = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_public = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_visible = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    z_index = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    layer_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    custom_style = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    filter_config = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    feature_count = table.Column<int>(type: "int", nullable: true),
                    data_size_kb = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    data_bounds = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_layers", x => x.layer_id);
                    table.ForeignKey(
                        name: "FK_layers_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_layers_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
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
                    table.ForeignKey(
                        name: "FK_map_histories_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_map_histories_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "map_images",
                columns: table => new
                {
                    map_image_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    image_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_data = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    latitude = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    longitude = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    width = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    height = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    rotation = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    z_index = table.Column<int>(type: "int", nullable: false, defaultValue: 500),
                    is_visible = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_map_images", x => x.map_image_id);
                    table.ForeignKey(
                        name: "FK_map_images_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "map_segments",
                columns: table => new
                {
                    segment_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_by = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    summary = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    story_content = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    auto_fit_bounds = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    entry_animation_preset_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    exit_animation_preset_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    default_layer_animation_preset_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    playback_mode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_map_segments", x => x.segment_id);
                    table.ForeignKey(
                        name: "FK_map_segments_layer_animation_presets_default_layer_animation~",
                        column: x => x.default_layer_animation_preset_id,
                        principalTable: "layer_animation_presets",
                        principalColumn: "animation_preset_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_map_segments_layer_animation_presets_entry_animation_preset_~",
                        column: x => x.entry_animation_preset_id,
                        principalTable: "layer_animation_presets",
                        principalColumn: "animation_preset_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_map_segments_layer_animation_presets_exit_animation_preset_id",
                        column: x => x.exit_animation_preset_id,
                        principalTable: "layer_animation_presets",
                        principalColumn: "animation_preset_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_map_segments_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_map_segments_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "map_zone_selections",
                columns: table => new
                {
                    map_zone_selection_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_by = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    selection_geometry = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    included_zone_ids = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    persist_results = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    summary = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_map_zone_selections", x => x.map_zone_selection_id);
                    table.ForeignKey(
                        name: "FK_map_zone_selections_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_map_zone_selections_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_favorite_templates",
                columns: table => new
                {
                    user_favorite_template_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    template_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    favorite_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_favorite_templates", x => x.user_favorite_template_id);
                    table.ForeignKey(
                        name: "FK_user_favorite_templates_maps_template_id",
                        column: x => x.template_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_favorite_templates_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
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
                name: "map_features",
                columns: table => new
                {
                    feature_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    layer_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    feature_category = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    annotation_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    geometry_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    coordinates = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    properties = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    style = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    is_visible = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    z_index = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_map_features", x => x.feature_id);
                    table.ForeignKey(
                        name: "FK_map_features_layers_layer_id",
                        column: x => x.layer_id,
                        principalTable: "layers",
                        principalColumn: "layer_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_map_features_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_map_features_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "map_segment_zones",
                columns: table => new
                {
                    segment_zone_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    segment_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    zone_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    zone_geometry = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    focus_camera_state = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_primary = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_map_segment_zones", x => x.segment_zone_id);
                    table.ForeignKey(
                        name: "FK_map_segment_zones_map_segments_segment_id",
                        column: x => x.segment_id,
                        principalTable: "map_segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "segment_transitions",
                columns: table => new
                {
                    segment_transition_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    from_segment_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    to_segment_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    effect_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    animation_preset_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    duration_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 600),
                    delay_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    auto_play = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    is_skippable = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    transition_config = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    metadata = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_segment_transitions", x => x.segment_transition_id);
                    table.ForeignKey(
                        name: "FK_segment_transitions_layer_animation_presets_animation_preset~",
                        column: x => x.animation_preset_id,
                        principalTable: "layer_animation_presets",
                        principalColumn: "animation_preset_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_segment_transitions_map_segments_from_segment_id",
                        column: x => x.from_segment_id,
                        principalTable: "map_segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_segment_transitions_map_segments_to_segment_id",
                        column: x => x.to_segment_id,
                        principalTable: "map_segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "timeline_steps",
                columns: table => new
                {
                    timeline_step_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    segment_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    subtitle = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    auto_advance = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    duration_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 6000),
                    trigger_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    camera_state = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    overlay_content = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timeline_steps", x => x.timeline_step_id);
                    table.ForeignKey(
                        name: "FK_timeline_steps_map_segments_segment_id",
                        column: x => x.segment_id,
                        principalTable: "map_segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_timeline_steps_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
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
                    purpose = table.Column<string>(type: "text", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "map_locations",
                columns: table => new
                {
                    map_location_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    segment_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    segment_zone_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    subtitle = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    location_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    marker_geometry = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    story_content = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    media_resources = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    highlight_on_enter = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    show_tooltip = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    tooltip_content = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    effect_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    open_slide_on_click = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    slide_content = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    linked_location_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    play_audio_on_click = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    audio_url = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    external_url = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    associated_layer_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    animation_preset_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    animation_overrides = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_map_locations", x => x.map_location_id);
                    table.ForeignKey(
                        name: "FK_map_locations_layer_animation_presets_animation_preset_id",
                        column: x => x.animation_preset_id,
                        principalTable: "layer_animation_presets",
                        principalColumn: "animation_preset_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_map_locations_layers_associated_layer_id",
                        column: x => x.associated_layer_id,
                        principalTable: "layers",
                        principalColumn: "layer_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_map_locations_map_locations_linked_location_id",
                        column: x => x.linked_location_id,
                        principalTable: "map_locations",
                        principalColumn: "map_location_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_map_locations_map_segment_zones_segment_zone_id",
                        column: x => x.segment_zone_id,
                        principalTable: "map_segment_zones",
                        principalColumn: "segment_zone_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_map_locations_map_segments_segment_id",
                        column: x => x.segment_id,
                        principalTable: "map_segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_map_locations_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "map_segment_layers",
                columns: table => new
                {
                    segment_layer_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    segment_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    layer_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    segment_zone_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    expand_to_zone = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    highlight_zone_boundary = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    delay_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    fade_in_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 400),
                    fade_out_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 400),
                    start_opacity = table.Column<double>(type: "double", nullable: false, defaultValue: 0.0),
                    end_opacity = table.Column<double>(type: "double", nullable: false, defaultValue: 1.0),
                    easing = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    animation_preset_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    auto_play_animation = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    repeat_count = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    animation_overrides = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    override_style = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    metadata = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_map_segment_layers", x => x.segment_layer_id);
                    table.ForeignKey(
                        name: "FK_map_segment_layers_layer_animation_presets_animation_preset_~",
                        column: x => x.animation_preset_id,
                        principalTable: "layer_animation_presets",
                        principalColumn: "animation_preset_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_map_segment_layers_layers_layer_id",
                        column: x => x.layer_id,
                        principalTable: "layers",
                        principalColumn: "layer_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_map_segment_layers_map_segment_zones_segment_zone_id",
                        column: x => x.segment_zone_id,
                        principalTable: "map_segment_zones",
                        principalColumn: "segment_zone_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_map_segment_layers_map_segments_segment_id",
                        column: x => x.segment_id,
                        principalTable: "map_segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "timeline_step_layers",
                columns: table => new
                {
                    timeline_step_layer_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    timeline_step_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    layer_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    is_visible = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    opacity = table.Column<double>(type: "double", nullable: false, defaultValue: 1.0),
                    fade_in_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 300),
                    fade_out_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 300),
                    delay_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    display_mode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    style_override = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    metadata = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timeline_step_layers", x => x.timeline_step_layer_id);
                    table.ForeignKey(
                        name: "FK_timeline_step_layers_layers_layer_id",
                        column: x => x.layer_id,
                        principalTable: "layers",
                        principalColumn: "layer_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_timeline_step_layers_timeline_steps_timeline_step_id",
                        column: x => x.timeline_step_id,
                        principalTable: "timeline_steps",
                        principalColumn: "timeline_step_id",
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
                    { new Guid("00000000-0000-0000-0000-000000000013"), "TextLabel" }
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
                    { 1, "To create a map, log in to your account, click 'Create New Map', select your desired OpenStreetMap area using the bounding box tool, add layers like roads, buildings, and POIs, customize layer styles (colors, icons, transparency), and annotate with markers, lines, or polygons as needed.", "Map Creation", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "How do I create a custom map?" },
                    { 2, "You can upload GeoJSON, KML, and CSV files up to 50MB in size. Make sure your CSV files contain coordinate columns for proper mapping. The system validates all uploaded data to ensure compatibility.", "Data Management", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "What file formats can I upload for my maps?" },
                    { 3, "You can export your maps in PDF, PNG, SVG, GeoJSON, and MBTiles formats. Resolution options range from 72 to 300 DPI depending on your subscription plan. Export quotas are plan-limited to ensure fair usage.", "Export System", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "What export formats are available?" },
                    { 4, "Use the real-time collaboration feature to share maps and layers with team members. You can set permissions for view, edit, or manage access levels. The system tracks map version history and supports WebSocket-based real-time updates for seamless collaboration.", "Collaboration", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "How do I collaborate with my team on maps?" },
                    { 5, "We accept payments through VNPay, PayOS, Stripe, and PayPal. All transactions are secured with PCI-DSS compliance and processed through our secure payment gateway integration.", "Billing", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "What payment methods are accepted?" },
                    { 6, "CustomMapOSM is compatible with Chrome, Firefox, and Edge browsers on desktop and mobile devices. For best performance, we recommend using the latest version of these browsers. The platform is built with Next.js 14 and React 18 for optimal user experience.", "Technical", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "What browsers and devices are supported?" },
                    { 7, "We offer various subscription plans with different quotas for map creation, exports, and collaboration features. You can upgrade or downgrade your plan at any time. Plans include auto-renewal options and usage tracking to help you monitor your consumption.", "Membership", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "How do subscription plans work?" },
                    { 8, "Yes, you can purchase add-ons like extra exports, advanced analytics, or API access. Add-ons are available in different quantities and take effect immediately upon successful payment. They complement your existing membership plan.", "Membership", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "Can I purchase additional features or add-ons?" },
                    { 9, "As an organization owner, you can invite team members, set their roles (Owner, Admin, Member, Viewer), and manage organization locations. Each organization can have multiple members with different permission levels for maps and collaboration features.", "Organization", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "How do I manage my organization and team members?" },
                    { 10, "The platform is designed for high performance with map loads under 2 seconds and exports under 30 seconds. It can support up to 1000 concurrent users and uses MySQL 8.0 with GIS extensions for spatial data processing and Azure Blob Storage for file management.", "Technical", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "What is the performance and scalability of the platform?" },
                    { 11, "You can submit support tickets through the platform, and our team will respond promptly. We also provide comprehensive documentation and FAQs. For urgent issues, please include detailed information about the problem and steps to reproduce it.", "Support", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "How do I get support if I encounter issues?" },
                    { 12, "Yes, we implement comprehensive security measures including JWT authentication, RBAC (Role-Based Access Control), data encryption at rest and in-transit, and audit logging for sensitive operations. Your maps can be set to private or public based on your preferences.", "Security", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "Is my data secure and private?" }
                });

            migrationBuilder.InsertData(
                table: "layer_sources",
                columns: new[] { "source_type_id", "name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000025"), "OpenStreetMap" },
                    { new Guid("00000000-0000-0000-0000-000000000026"), "UserUploaded" },
                    { new Guid("00000000-0000-0000-0000-000000000027"), "ExternalAPI" },
                    { new Guid("00000000-0000-0000-0000-000000000028"), "Database" },
                    { new Guid("00000000-0000-0000-0000-000000000029"), "WebMapService" }
                });

            migrationBuilder.InsertData(
                table: "layer_types",
                columns: new[] { "layer_type_id", "created_at", "description", "icon_url", "is_active", "type_name" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Street and road networks from OpenStreetMap", "/icons/roads.svg", true, "GEOJSON" },
                    { 2, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Building footprints and structures", "/icons/buildings.svg", true, "KML" },
                    { 3, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Points of Interest including amenities and landmarks", "/icons/poi.svg", true, "Shapefile" },
                    { 4, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "User uploaded GeoJSON data layers", "/icons/geojson.svg", true, "GEOJSON" },
                    { 5, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "User uploaded KML data layers", "/icons/kml.svg", true, "KML" },
                    { 6, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "User uploaded CSV data with coordinates", "/icons/csv.svg", true, "CSV" }
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
                    { new Guid("00000000-0000-0000-0000-000000000051"), "BankTransfer" },
                    { new Guid("00000000-0000-0000-0000-000000000052"), "PayOS" }
                });

            migrationBuilder.InsertData(
                table: "plans",
                columns: new[] { "plan_id", "access_tool_ids", "created_at", "description", "duration_months", "export_quota", "features", "is_active", "map_quota", "max_custom_layers", "max_locations_per_org", "max_maps_per_month", "max_organizations", "max_users_per_org", "monthly_tokens", "plan_name", "price_monthly", "priority_support", "updated_at" },
                values: new object[,]
                {
                    { 1, "[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11]", new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Basic features for individual users", 1, 5, "{\"templates\": true, \"basic_export\": true, \"public_maps\": true}", true, 10, 3, 1, 5, 1, 1, 5000, "Free", 0.00m, false, null },
                    { 2, "[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28]", new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Advanced features for growing businesses", 1, 200, "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"analytics\": true, \"version_history\": true}", true, 200, 50, 20, 100, 5, 20, 50000, "Pro", 29.99m, true, null },
                    { 3, "[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31]", new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Full-featured solution for large organizations", 1, -1, "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"analytics\": true, \"version_history\": true, \"api_access\": true, \"white_label\": true, \"sso\": true}", true, -1, -1, -1, -1, -1, -1, 200000, "Enterprise", 99.99m, true, null }
                });

            migrationBuilder.InsertData(
                table: "user_roles",
                columns: new[] { "role_id", "name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "Staff" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "RegisteredUser" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "Admin" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_administrative_zones_parent_zone_id",
                table: "administrative_zones",
                column: "parent_zone_id");

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
                name: "IX_data_source_bookmarks_user_id",
                table: "data_source_bookmarks",
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
                name: "IX_failed_emails_CreatedAt",
                table: "failed_emails",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_failed_emails_Status",
                table: "failed_emails",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_failed_emails_Status_RetryCount",
                table: "failed_emails",
                columns: new[] { "Status", "RetryCount" });

            migrationBuilder.CreateIndex(
                name: "IX_failed_emails_ToEmail",
                table: "failed_emails",
                column: "ToEmail");

            migrationBuilder.CreateIndex(
                name: "IX_layers_map_id",
                table: "layers",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_layers_user_id",
                table: "layers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_features_created_by",
                table: "map_features",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_map_features_layer_id",
                table: "map_features",
                column: "layer_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_features_map_id",
                table: "map_features",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_histories_map_id",
                table: "map_histories",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_histories_user_id",
                table: "map_histories",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_images_map_id",
                table: "map_images",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_locations_animation_preset_id",
                table: "map_locations",
                column: "animation_preset_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_locations_associated_layer_id",
                table: "map_locations",
                column: "associated_layer_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_locations_linked_location_id",
                table: "map_locations",
                column: "linked_location_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_locations_map_id",
                table: "map_locations",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_locations_segment_id",
                table: "map_locations",
                column: "segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_locations_segment_zone_id",
                table: "map_locations",
                column: "segment_zone_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_segment_layers_animation_preset_id",
                table: "map_segment_layers",
                column: "animation_preset_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_segment_layers_layer_id",
                table: "map_segment_layers",
                column: "layer_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_segment_layers_segment_id",
                table: "map_segment_layers",
                column: "segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_segment_layers_segment_zone_id",
                table: "map_segment_layers",
                column: "segment_zone_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_segment_zones_segment_id",
                table: "map_segment_zones",
                column: "segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_segments_created_by",
                table: "map_segments",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_map_segments_default_layer_animation_preset_id",
                table: "map_segments",
                column: "default_layer_animation_preset_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_segments_entry_animation_preset_id",
                table: "map_segments",
                column: "entry_animation_preset_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_segments_exit_animation_preset_id",
                table: "map_segments",
                column: "exit_animation_preset_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_segments_map_id",
                table: "map_segments",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_map_zone_selections_created_by",
                table: "map_zone_selections",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_map_zone_selections_map_id",
                table: "map_zone_selections",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_maps_org_id",
                table: "maps",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_maps_parent_map_id",
                table: "maps",
                column: "parent_map_id");

            migrationBuilder.CreateIndex(
                name: "IX_maps_user_id",
                table: "maps",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_membership_addons_membership_id_org_id_addon_key",
                table: "membership_addons",
                columns: new[] { "membership_id", "org_id", "addon_key" });

            migrationBuilder.CreateIndex(
                name: "IX_membership_usages_membership_id_org_id",
                table: "membership_usages",
                columns: new[] { "membership_id", "org_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_memberships_org_id",
                table: "memberships",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_memberships_plan_id",
                table: "memberships",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_memberships_user_id",
                table: "memberships",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id",
                table: "notifications",
                column: "user_id");

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
                name: "IX_segment_transitions_animation_preset_id",
                table: "segment_transitions",
                column: "animation_preset_id");

            migrationBuilder.CreateIndex(
                name: "IX_segment_transitions_from_segment_id",
                table: "segment_transitions",
                column: "from_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_segment_transitions_to_segment_id",
                table: "segment_transitions",
                column: "to_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_support_tickets_user_id",
                table: "support_tickets",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_timeline_step_layers_layer_id",
                table: "timeline_step_layers",
                column: "layer_id");

            migrationBuilder.CreateIndex(
                name: "IX_timeline_step_layers_timeline_step_id",
                table: "timeline_step_layers",
                column: "timeline_step_id");

            migrationBuilder.CreateIndex(
                name: "IX_timeline_steps_map_id",
                table: "timeline_steps",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_timeline_steps_segment_id",
                table: "timeline_steps",
                column: "segment_id");

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
                name: "IX_user_favorite_templates_template_id",
                table: "user_favorite_templates",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_favorite_templates_user_id",
                table: "user_favorite_templates",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_preferences_UserId",
                table: "user_preferences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_RoleId",
                table: "users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_zone_insights_zone_id",
                table: "zone_insights",
                column: "zone_id");

            migrationBuilder.CreateIndex(
                name: "IX_zone_statistics_zone_id",
                table: "zone_statistics",
                column: "zone_id");
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
                name: "failed_emails");

            migrationBuilder.DropTable(
                name: "faqs");

            migrationBuilder.DropTable(
                name: "layer_sources");

            migrationBuilder.DropTable(
                name: "layer_types");

            migrationBuilder.DropTable(
                name: "map_features");

            migrationBuilder.DropTable(
                name: "map_histories");

            migrationBuilder.DropTable(
                name: "map_images");

            migrationBuilder.DropTable(
                name: "map_locations");

            migrationBuilder.DropTable(
                name: "map_segment_layers");

            migrationBuilder.DropTable(
                name: "map_zone_selections");

            migrationBuilder.DropTable(
                name: "membership_addons");

            migrationBuilder.DropTable(
                name: "membership_usages");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "organization_invitation");

            migrationBuilder.DropTable(
                name: "organization_members");

            migrationBuilder.DropTable(
                name: "segment_transitions");

            migrationBuilder.DropTable(
                name: "support_tickets");

            migrationBuilder.DropTable(
                name: "timeline_step_layers");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "user_access_tools");

            migrationBuilder.DropTable(
                name: "user_favorite_templates");

            migrationBuilder.DropTable(
                name: "user_preferences");

            migrationBuilder.DropTable(
                name: "zone_insights");

            migrationBuilder.DropTable(
                name: "zone_statistics");

            migrationBuilder.DropTable(
                name: "annotation_types");

            migrationBuilder.DropTable(
                name: "collaboration_permissions");

            migrationBuilder.DropTable(
                name: "collaboration_target_types");

            migrationBuilder.DropTable(
                name: "map_segment_zones");

            migrationBuilder.DropTable(
                name: "organization_member_types");

            migrationBuilder.DropTable(
                name: "layers");

            migrationBuilder.DropTable(
                name: "timeline_steps");

            migrationBuilder.DropTable(
                name: "exports");

            migrationBuilder.DropTable(
                name: "payment_gateways");

            migrationBuilder.DropTable(
                name: "access_tools");

            migrationBuilder.DropTable(
                name: "administrative_zones");

            migrationBuilder.DropTable(
                name: "map_segments");

            migrationBuilder.DropTable(
                name: "export_types");

            migrationBuilder.DropTable(
                name: "memberships");

            migrationBuilder.DropTable(
                name: "layer_animation_presets");

            migrationBuilder.DropTable(
                name: "maps");

            migrationBuilder.DropTable(
                name: "plans");

            migrationBuilder.DropTable(
                name: "organizations");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "user_roles");
        }
    }
}
