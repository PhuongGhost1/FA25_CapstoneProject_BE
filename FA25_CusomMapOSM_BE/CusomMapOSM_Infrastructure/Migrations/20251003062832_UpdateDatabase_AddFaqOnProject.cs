using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase_AddFaqOnProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.UpdateData(
                table: "faqs",
                keyColumn: "faq_id",
                keyValue: 1,
                columns: new[] { "answer", "created_at", "question" },
                values: new object[] { "To create a map, log in to your account, click 'Create New Map', select your desired OpenStreetMap area using the bounding box tool, add layers like roads, buildings, and POIs, customize layer styles (colors, icons, transparency), and annotate with markers, lines, or polygons as needed.", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "How do I create a custom map?" });

            migrationBuilder.UpdateData(
                table: "faqs",
                keyColumn: "faq_id",
                keyValue: 2,
                columns: new[] { "answer", "created_at", "question" },
                values: new object[] { "You can upload GeoJSON, KML, and CSV files up to 50MB in size. Make sure your CSV files contain coordinate columns for proper mapping. The system validates all uploaded data to ensure compatibility.", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "What file formats can I upload for my maps?" });

            migrationBuilder.UpdateData(
                table: "faqs",
                keyColumn: "faq_id",
                keyValue: 3,
                columns: new[] { "answer", "created_at" },
                values: new object[] { "You can export your maps in PDF, PNG, SVG, GeoJSON, and MBTiles formats. Resolution options range from 72 to 300 DPI depending on your subscription plan. Export quotas are plan-limited to ensure fair usage.", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "faqs",
                keyColumn: "faq_id",
                keyValue: 4,
                columns: new[] { "answer", "created_at", "question" },
                values: new object[] { "Use the real-time collaboration feature to share maps and layers with team members. You can set permissions for view, edit, or manage access levels. The system tracks map version history and supports WebSocket-based real-time updates for seamless collaboration.", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "How do I collaborate with my team on maps?" });

            migrationBuilder.UpdateData(
                table: "faqs",
                keyColumn: "faq_id",
                keyValue: 5,
                columns: new[] { "answer", "created_at" },
                values: new object[] { "We accept payments through VNPay, PayOS, Stripe, and PayPal. All transactions are secured with PCI-DSS compliance and processed through our secure payment gateway integration.", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "faqs",
                keyColumn: "faq_id",
                keyValue: 6,
                columns: new[] { "answer", "created_at", "question" },
                values: new object[] { "CustomMapOSM is compatible with Chrome, Firefox, and Edge browsers on desktop and mobile devices. For best performance, we recommend using the latest version of these browsers. The platform is built with Next.js 14 and React 18 for optimal user experience.", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "What browsers and devices are supported?" });

            migrationBuilder.InsertData(
                table: "faqs",
                columns: new[] { "faq_id", "answer", "category", "created_at", "question" },
                values: new object[,]
                {
                    { 7, "We offer various subscription plans with different quotas for map creation, exports, and collaboration features. You can upgrade or downgrade your plan at any time. Plans include auto-renewal options and usage tracking to help you monitor your consumption.", "Membership", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "How do subscription plans work?" },
                    { 8, "Yes, you can purchase add-ons like extra exports, advanced analytics, or API access. Add-ons are available in different quantities and take effect immediately upon successful payment. They complement your existing membership plan.", "Membership", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "Can I purchase additional features or add-ons?" },
                    { 9, "As an organization owner, you can invite team members, set their roles (Owner, Admin, Member, Viewer), and manage organization locations. Each organization can have multiple members with different permission levels for maps and collaboration features.", "Organization", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "How do I manage my organization and team members?" },
                    { 10, "The platform is designed for high performance with map loads under 2 seconds and exports under 30 seconds. It can support up to 1000 concurrent users and uses MySQL 8.0 with GIS extensions for spatial data processing and Azure Blob Storage for file management.", "Technical", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "What is the performance and scalability of the platform?" },
                    { 11, "You can submit support tickets through the platform, and our team will respond promptly. We also provide comprehensive documentation and FAQs. For urgent issues, please include detailed information about the problem and steps to reproduce it.", "Support", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "How do I get support if I encounter issues?" },
                    { 12, "Yes, we implement comprehensive security measures including JWT authentication, RBAC (Role-Based Access Control), data encryption at rest and in-transit, and audit logging for sensitive operations. Your maps can be set to private or public based on your preferences.", "Security", new DateTime(2025, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), "Is my data secure and private?" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "faqs",
                keyColumn: "faq_id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "faqs",
                keyColumn: "faq_id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "faqs",
                keyColumn: "faq_id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "faqs",
                keyColumn: "faq_id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "faqs",
                keyColumn: "faq_id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "faqs",
                keyColumn: "faq_id",
                keyValue: 12);

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

            migrationBuilder.UpdateData(
                table: "faqs",
                keyColumn: "faq_id",
                keyValue: 1,
                columns: new[] { "answer", "created_at", "question" },
                values: new object[] { "To create a map, log in to your account, click 'Create New Map', select your desired OSM area using the bounding box tool, add layers like roads, buildings, and POIs, then customize the styling to your preference.", new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "How do I create a map?" });

            migrationBuilder.UpdateData(
                table: "faqs",
                keyColumn: "faq_id",
                keyValue: 2,
                columns: new[] { "answer", "created_at", "question" },
                values: new object[] { "You can upload GeoJSON, KML, and CSV files up to 50MB in size. Make sure your CSV files contain coordinate columns for proper mapping.", new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "What file formats can I upload?" });

            migrationBuilder.UpdateData(
                table: "faqs",
                keyColumn: "faq_id",
                keyValue: 3,
                columns: new[] { "answer", "created_at" },
                values: new object[] { "You can export your maps in PDF, PNG, SVG, GeoJSON, and MBTiles formats. Resolution options range from 72 to 300 DPI depending on your subscription plan.", new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "faqs",
                keyColumn: "faq_id",
                keyValue: 4,
                columns: new[] { "answer", "created_at", "question" },
                values: new object[] { "Use the collaboration feature to share maps and layers with team members. You can set permissions for view, edit, or manage access levels.", new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "How do I share maps with my team?" });

            migrationBuilder.UpdateData(
                table: "faqs",
                keyColumn: "faq_id",
                keyValue: 5,
                columns: new[] { "answer", "created_at" },
                values: new object[] { "We accept payments through VNPay, PayPal, and bank transfers. All transactions are secured with PCI-DSS compliance.", new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "faqs",
                keyColumn: "faq_id",
                keyValue: 6,
                columns: new[] { "answer", "created_at", "question" },
                values: new object[] { "CustomMapOSM is compatible with Chrome, Firefox, and Edge browsers. For best performance, we recommend using the latest version of these browsers.", new DateTime(2025, 8, 6, 1, 0, 0, 0, DateTimeKind.Utc), "What browsers are supported?" });
        }
    }
}
