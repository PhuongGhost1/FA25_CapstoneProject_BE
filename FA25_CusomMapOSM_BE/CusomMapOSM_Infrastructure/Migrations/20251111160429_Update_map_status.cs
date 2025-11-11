using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_map_status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Advertisements",
                columns: table => new
                {
                    AdvertisementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AdvertisementTitle = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdvertisementContent = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Advertisements", x => x.AdvertisementId);
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
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faqs", x => x.faq_id);
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
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membership_usages", x => x.usage_id);
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
                    MaxInteractionsPerMap = table.Column<int>(type: "int", nullable: false),
                    MaxMediaFileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MaxVideoFileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MaxAudioFileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MaxConnectionsPerMap = table.Column<int>(type: "int", nullable: false),
                    Allow3DEffects = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowVideoContent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowAudioContent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowAnimatedConnections = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plans", x => x.plan_id);
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
                    role = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    account_status = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_login = table.Column<DateTime>(type: "datetime", nullable: true),
                    monthly_token_usage = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    last_token_reset = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "zones",
                columns: table => new
                {
                    zone_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    external_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    zone_code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    zone_type = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    admin_level = table.Column<int>(type: "int", nullable: false),
                    parent_zone_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    geometry = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    simplified_geometry = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    centroid = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bounding_box = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    last_synced_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zones", x => x.zone_id);
                    table.ForeignKey(
                        name: "FK_zones_zones_parent_zone_id",
                        column: x => x.parent_zone_id,
                        principalTable: "zones",
                        principalColumn: "zone_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "animated_layer_presets",
                columns: table => new
                {
                    animated_layer_preset_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_by = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tags = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    media_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_url = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thumbnail_url = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    default_coordinates = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    default_is_screen_overlay = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    default_screen_position = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    default_scale = table.Column<double>(type: "double", nullable: false, defaultValue: 1.0),
                    default_opacity = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 1.0m),
                    default_auto_play = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    default_loop = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    is_system_preset = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    is_public = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    usage_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_animated_layer_presets", x => x.animated_layer_preset_id);
                    table.ForeignKey(
                        name: "FK_animated_layer_presets_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
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
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                name: "organization_invitations",
                columns: table => new
                {
                    invitation_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    org_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    invited_by = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    role = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    invitation_token = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    invited_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    responded_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    message = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_invitations", x => x.invitation_id);
                    table.ForeignKey(
                        name: "FK_organization_invitations_organizations_org_id",
                        column: x => x.org_id,
                        principalTable: "organizations",
                        principalColumn: "org_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organization_invitations_users_invited_by",
                        column: x => x.invited_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "workspaces",
                columns: table => new
                {
                    workspace_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    org_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    created_by = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    workspace_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icon = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    access = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workspaces", x => x.workspace_id);
                    table.ForeignKey(
                        name: "FK_workspaces_organizations_org_id",
                        column: x => x.org_id,
                        principalTable: "organizations",
                        principalColumn: "org_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workspaces_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "support_ticket_messages",
                columns: table => new
                {
                    message_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ticket_id = table.Column<int>(type: "int", nullable: false),
                    message = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_from_user = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_support_ticket_messages", x => x.message_id);
                    table.ForeignKey(
                        name: "FK_support_ticket_messages_support_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "support_tickets",
                        principalColumn: "ticket_id",
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
                    role = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    invitation_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    invited_by = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    joined_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    left_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    leave_reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_members", x => x.member_id);
                    table.ForeignKey(
                        name: "FK_organization_members_organization_invitations_invitation_id",
                        column: x => x.invitation_id,
                        principalTable: "organization_invitations",
                        principalColumn: "invitation_id",
                        onDelete: ReferentialAction.SetNull);
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
                name: "maps",
                columns: table => new
                {
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    workspace_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
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
                    map_status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    published_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                        name: "FK_maps_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_maps_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "workspace_id",
                        onDelete: ReferentialAction.SetNull);
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
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
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
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    export_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exports", x => x.export_id);
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
                name: "layers",
                columns: table => new
                {
                    layer_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    layer_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    layer_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_path = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data_store_key = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    layer_data = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    layer_style = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_public = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    feature_count = table.Column<int>(type: "int", nullable: true),
                    data_size_kb = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    data_bounds = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "map_histories",
                columns: table => new
                {
                    history_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    history_version = table.Column<int>(type: "int", nullable: false),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    snapshot_data = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_map_histories", x => x.history_id);
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
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
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
                name: "segments",
                columns: table => new
                {
                    segment_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_by = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    story_content = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    camera_state = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    auto_advance = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    duration_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 6000),
                    require_user_action = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_segments", x => x.segment_id);
                    table.ForeignKey(
                        name: "FK_segments_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_segments_users_created_by",
                        column: x => x.created_by,
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
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    membership_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    export_id = table.Column<int>(type: "int", nullable: true),
                    purpose = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content = table.Column<string>(type: "text", nullable: true)
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
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    mongo_document_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                name: "animated_layers",
                columns: table => new
                {
                    animated_layer_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_by = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    layer_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    segment_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    media_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_url = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thumbnail_url = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    coordinates = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_screen_overlay = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    screen_position = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rotation_deg = table.Column<double>(type: "double", nullable: false, defaultValue: 0.0),
                    scale = table.Column<double>(type: "double", nullable: false, defaultValue: 1.0),
                    opacity = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 1.0m),
                    z_index = table.Column<int>(type: "int", nullable: false, defaultValue: 1000),
                    css_filter = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    auto_play = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    loop = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    playback_speed = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    start_time_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    end_time_ms = table.Column<int>(type: "int", nullable: true),
                    entry_delay_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    entry_duration_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 400),
                    entry_effect = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, defaultValue: "fade")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    exit_delay_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    exit_duration_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 400),
                    exit_effect = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, defaultValue: "fade")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    enable_click = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    on_click_action = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_visible = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_animated_layers", x => x.animated_layer_id);
                    table.ForeignKey(
                        name: "FK_animated_layers_layers_layer_id",
                        column: x => x.layer_id,
                        principalTable: "layers",
                        principalColumn: "layer_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_animated_layers_segments_segment_id",
                        column: x => x.segment_id,
                        principalTable: "segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_animated_layers_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    location_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    segment_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_by = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    subtitle = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    location_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    marker_geometry = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icon_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icon_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icon_color = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icon_size = table.Column<int>(type: "int", nullable: false, defaultValue: 32),
                    z_index = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    show_tooltip = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    tooltip_content = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    open_popup_on_click = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    popup_content = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    media_urls = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    play_audio_on_click = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    audio_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    entry_delay_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    entry_duration_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 400),
                    exit_delay_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    exit_duration_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 400),
                    entry_effect = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, defaultValue: "fade")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    exit_effect = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, defaultValue: "fade")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    linked_segment_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    linked_location_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    external_url = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_visible = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locations", x => x.location_id);
                    table.ForeignKey(
                        name: "FK_locations_locations_linked_location_id",
                        column: x => x.linked_location_id,
                        principalTable: "locations",
                        principalColumn: "location_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_locations_segments_linked_segment_id",
                        column: x => x.linked_segment_id,
                        principalTable: "segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_locations_segments_segment_id",
                        column: x => x.segment_id,
                        principalTable: "segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_locations_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "segment_layers",
                columns: table => new
                {
                    segment_layer_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    segment_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    layer_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_visible = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    opacity = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 1.0m),
                    z_index = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    entry_delay_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    entry_duration_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 400),
                    exit_delay_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    exit_duration_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 400),
                    entry_effect = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, defaultValue: "fade")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    exit_effect = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, defaultValue: "fade")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    style_override = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_segment_layers", x => x.segment_layer_id);
                    table.ForeignKey(
                        name: "FK_segment_layers_layers_layer_id",
                        column: x => x.layer_id,
                        principalTable: "layers",
                        principalColumn: "layer_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_segment_layers_segments_segment_id",
                        column: x => x.segment_id,
                        principalTable: "segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "segment_zones",
                columns: table => new
                {
                    segment_zone_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    segment_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    zone_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_visible = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    z_index = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    highlight_boundary = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    boundary_color = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    boundary_width = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    fill_zone = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    fill_color = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fill_opacity = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 0.3m),
                    show_label = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    label_override = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    label_style = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    entry_delay_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    entry_duration_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 400),
                    exit_delay_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    exit_duration_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 400),
                    entry_effect = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, defaultValue: "fade")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    exit_effect = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, defaultValue: "fade")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fit_bounds_on_entry = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    camera_override = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_segment_zones", x => x.segment_zone_id);
                    table.ForeignKey(
                        name: "FK_segment_zones_segments_segment_id",
                        column: x => x.segment_id,
                        principalTable: "segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_segment_zones_zones_zone_id",
                        column: x => x.zone_id,
                        principalTable: "zones",
                        principalColumn: "zone_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "timeline_transitions",
                columns: table => new
                {
                    timeline_transition_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    from_segment_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    to_segment_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    transition_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    duration_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 1000),
                    transition_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    animate_camera = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    camera_animation_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    camera_animation_duration_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 1000),
                    show_overlay = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    overlay_content = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    auto_trigger = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    require_user_action = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    trigger_button_text = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, defaultValue: "Next")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timeline_transitions", x => x.timeline_transition_id);
                    table.ForeignKey(
                        name: "FK_timeline_transitions_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_timeline_transitions_segments_from_segment_id",
                        column: x => x.from_segment_id,
                        principalTable: "segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_timeline_transitions_segments_to_segment_id",
                        column: x => x.to_segment_id,
                        principalTable: "segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                columns: new[] { "plan_id", "Allow3DEffects", "AllowAnimatedConnections", "AllowAudioContent", "AllowVideoContent", "created_at", "description", "duration_months", "export_quota", "features", "is_active", "map_quota", "MaxAudioFileSizeBytes", "MaxConnectionsPerMap", "max_custom_layers", "MaxInteractionsPerMap", "max_locations_per_org", "max_maps_per_month", "MaxMediaFileSizeBytes", "max_organizations", "max_users_per_org", "MaxVideoFileSizeBytes", "monthly_tokens", "plan_name", "price_monthly", "priority_support", "updated_at" },
                values: new object[,]
                {
                    { 1, false, true, true, true, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Basic features for individual users", 1, 5, "{\"templates\": true, \"basic_export\": true, \"public_maps\": true}", true, 10, 20971520L, 100, 3, 50, 1, 5, 10485760L, 1, 1, 104857600L, 5000, "Free", 0.00m, false, null },
                    { 2, false, true, true, true, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Advanced features for growing businesses", 1, 200, "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"analytics\": true, \"version_history\": true}", true, 200, 20971520L, 100, 50, 50, 20, 100, 10485760L, 5, 20, 104857600L, 50000, "Pro", 29.99m, true, null },
                    { 3, false, true, true, true, new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "Full-featured solution for large organizations", 1, -1, "{\"templates\": true, \"all_export_formats\": true, \"collaboration\": true, \"data_import\": true, \"analytics\": true, \"version_history\": true, \"api_access\": true, \"white_label\": true, \"sso\": true}", true, -1, 20971520L, 100, -1, 50, -1, -1, 10485760L, -1, -1, 104857600L, 200000, "Enterprise", 99.99m, true, null }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "user_id", "account_status", "created_at", "email", "full_name", "last_login", "last_token_reset", "password_hash", "phone", "role" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@cusommaposm.com", "System Administrator", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "7aaea8cd5f395868fe32e08a7cb9bb060149f6b3fc8c6695c78ca9bf403f47d8", "+1234567890", "Admin" });

            migrationBuilder.CreateIndex(
                name: "IX_animated_layer_presets_created_by",
                table: "animated_layer_presets",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_animated_layers_created_by",
                table: "animated_layers",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_animated_layers_layer_id",
                table: "animated_layers",
                column: "layer_id");

            migrationBuilder.CreateIndex(
                name: "IX_animated_layers_segment_id",
                table: "animated_layers",
                column: "segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_bookmarks_map_id",
                table: "bookmarks",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_bookmarks_user_id",
                table: "bookmarks",
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
                name: "IX_layers_map_id",
                table: "layers",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_layers_user_id",
                table: "layers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_locations_created_by",
                table: "locations",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_locations_linked_location_id",
                table: "locations",
                column: "linked_location_id");

            migrationBuilder.CreateIndex(
                name: "IX_locations_linked_segment_id",
                table: "locations",
                column: "linked_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_locations_segment_id",
                table: "locations",
                column: "segment_id");

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
                name: "IX_maps_parent_map_id",
                table: "maps",
                column: "parent_map_id");

            migrationBuilder.CreateIndex(
                name: "IX_maps_user_id",
                table: "maps",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_maps_workspace_id",
                table: "maps",
                column: "workspace_id");

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
                name: "IX_organization_invitations_email",
                table: "organization_invitations",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_organization_invitations_expires_at",
                table: "organization_invitations",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_organization_invitations_invited_by",
                table: "organization_invitations",
                column: "invited_by");

            migrationBuilder.CreateIndex(
                name: "IX_organization_invitations_org_id",
                table: "organization_invitations",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_invitations_status",
                table: "organization_invitations",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_organization_members_invitation_id",
                table: "organization_members",
                column: "invitation_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_members_invited_by",
                table: "organization_members",
                column: "invited_by");

            migrationBuilder.CreateIndex(
                name: "IX_organization_members_org_id_user_id",
                table: "organization_members",
                columns: new[] { "org_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_members_status",
                table: "organization_members",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_organization_members_user_id",
                table: "organization_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_owner_user_id",
                table: "organizations",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_segment_layers_layer_id",
                table: "segment_layers",
                column: "layer_id");

            migrationBuilder.CreateIndex(
                name: "IX_segment_layers_segment_id",
                table: "segment_layers",
                column: "segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_segment_zones_segment_id",
                table: "segment_zones",
                column: "segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_segment_zones_zone_id",
                table: "segment_zones",
                column: "zone_id");

            migrationBuilder.CreateIndex(
                name: "IX_segments_created_by",
                table: "segments",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_segments_map_id",
                table: "segments",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_support_ticket_messages_created_at",
                table: "support_ticket_messages",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_support_ticket_messages_ticket_id",
                table: "support_ticket_messages",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "IX_support_tickets_user_id",
                table: "support_tickets",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_timeline_transitions_from_segment_id",
                table: "timeline_transitions",
                column: "from_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_timeline_transitions_map_id",
                table: "timeline_transitions",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_timeline_transitions_to_segment_id",
                table: "timeline_transitions",
                column: "to_segment_id");

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
                name: "IX_workspaces_created_by",
                table: "workspaces",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_workspaces_org_id",
                table: "workspaces",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_zones_parent_zone_id",
                table: "zones",
                column: "parent_zone_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Advertisements");

            migrationBuilder.DropTable(
                name: "animated_layer_presets");

            migrationBuilder.DropTable(
                name: "animated_layers");

            migrationBuilder.DropTable(
                name: "bookmarks");

            migrationBuilder.DropTable(
                name: "comments");

            migrationBuilder.DropTable(
                name: "faqs");

            migrationBuilder.DropTable(
                name: "locations");

            migrationBuilder.DropTable(
                name: "map_features");

            migrationBuilder.DropTable(
                name: "map_histories");

            migrationBuilder.DropTable(
                name: "map_images");

            migrationBuilder.DropTable(
                name: "membership_usages");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "organization_members");

            migrationBuilder.DropTable(
                name: "segment_layers");

            migrationBuilder.DropTable(
                name: "segment_zones");

            migrationBuilder.DropTable(
                name: "support_ticket_messages");

            migrationBuilder.DropTable(
                name: "timeline_transitions");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "organization_invitations");

            migrationBuilder.DropTable(
                name: "layers");

            migrationBuilder.DropTable(
                name: "zones");

            migrationBuilder.DropTable(
                name: "support_tickets");

            migrationBuilder.DropTable(
                name: "segments");

            migrationBuilder.DropTable(
                name: "exports");

            migrationBuilder.DropTable(
                name: "payment_gateways");

            migrationBuilder.DropTable(
                name: "maps");

            migrationBuilder.DropTable(
                name: "memberships");

            migrationBuilder.DropTable(
                name: "workspaces");

            migrationBuilder.DropTable(
                name: "plans");

            migrationBuilder.DropTable(
                name: "organizations");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
