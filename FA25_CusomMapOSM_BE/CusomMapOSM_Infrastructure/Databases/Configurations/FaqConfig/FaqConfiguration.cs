using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Faqs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.FaqConfig;

internal class FaqConfiguration : IEntityTypeConfiguration<Faq>
{
    public void Configure(EntityTypeBuilder<Faq> builder)
    {
        builder.ToTable("faqs");

        builder.HasKey(f => f.FaqId);

        builder.Property(f => f.FaqId)
            .HasColumnName("faq_id")
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(f => f.Question)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("question");

        builder.Property(f => f.Answer)
            .IsRequired()
            .HasColumnType("text") // MySQL supports TEXT for long text
            .HasColumnName("answer");

        builder.Property(f => f.Category)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("category");

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Sample FAQ data based on project requirements and documentation
        builder.HasData(
            new Faq
            {
                FaqId = 1,
                Question = "How do I create a custom map?",
                Answer = "To create a map, log in to your account, click 'Create New Map', select your desired OpenStreetMap area using the bounding box tool, add layers like roads, buildings, and POIs, customize layer styles (colors, icons, transparency), and annotate with markers, lines, or polygons as needed.",
                Category = "Map Creation",
                CreatedAt = new DateTime(2025, 01, 15, 1, 0, 0, DateTimeKind.Utc)
            },
            new Faq
            {
                FaqId = 2,
                Question = "What file formats can I upload for my maps?",
                Answer = "You can upload GeoJSON, KML, and CSV files up to 50MB in size. Make sure your CSV files contain coordinate columns for proper mapping. The system validates all uploaded data to ensure compatibility.",
                Category = "Data Management",
                CreatedAt = new DateTime(2025, 01, 15, 1, 0, 0, DateTimeKind.Utc)
            },
            new Faq
            {
                FaqId = 3,
                Question = "What export formats are available?",
                Answer = "You can export your maps in PDF, PNG, SVG, GeoJSON, and MBTiles formats. Resolution options range from 72 to 300 DPI depending on your subscription plan. Export quotas are plan-limited to ensure fair usage.",
                Category = "Export System",
                CreatedAt = new DateTime(2025, 01, 15, 1, 0, 0, DateTimeKind.Utc)
            },
            new Faq
            {
                FaqId = 4,
                Question = "How do I collaborate with my team on maps?",
                Answer = "Use the real-time collaboration feature to share maps and layers with team members. You can set permissions for view, edit, or manage access levels. The system tracks map version history and supports WebSocket-based real-time updates for seamless collaboration.",
                Category = "Collaboration",
                CreatedAt = new DateTime(2025, 01, 15, 1, 0, 0, DateTimeKind.Utc)
            },
            new Faq
            {
                FaqId = 5,
                Question = "What payment methods are accepted?",
                Answer = "We accept payments through VNPay, PayOS, Stripe, and PayPal. All transactions are secured with PCI-DSS compliance and processed through our secure payment gateway integration.",
                Category = "Billing",
                CreatedAt = new DateTime(2025, 01, 15, 1, 0, 0, DateTimeKind.Utc)
            },
            new Faq
            {
                FaqId = 6,
                Question = "What browsers and devices are supported?",
                Answer = "CustomMapOSM is compatible with Chrome, Firefox, and Edge browsers on desktop and mobile devices. For best performance, we recommend using the latest version of these browsers. The platform is built with Next.js 14 and React 18 for optimal user experience.",
                Category = "Technical",
                CreatedAt = new DateTime(2025, 01, 15, 1, 0, 0, DateTimeKind.Utc)
            },
            new Faq
            {
                FaqId = 7,
                Question = "How do subscription plans work?",
                Answer = "We offer various subscription plans with different quotas for map creation, exports, and collaboration features. You can upgrade or downgrade your plan at any time. Plans include auto-renewal options and usage tracking to help you monitor your consumption.",
                Category = "Membership",
                CreatedAt = new DateTime(2025, 01, 15, 1, 0, 0, DateTimeKind.Utc)
            },
            new Faq
            {
                FaqId = 8,
                Question = "Can I purchase additional features or add-ons?",
                Answer = "Yes, you can purchase add-ons like extra exports, advanced analytics, or API access. Add-ons are available in different quantities and take effect immediately upon successful payment. They complement your existing membership plan.",
                Category = "Membership",
                CreatedAt = new DateTime(2025, 01, 15, 1, 0, 0, DateTimeKind.Utc)
            },
            new Faq
            {
                FaqId = 9,
                Question = "How do I manage my organization and team members?",
                Answer = "As an organization owner, you can invite team members, set their roles (Owner, Admin, Member, Viewer), and manage organization locations. Each organization can have multiple members with different permission levels for maps and collaboration features.",
                Category = "Organization",
                CreatedAt = new DateTime(2025, 01, 15, 1, 0, 0, DateTimeKind.Utc)
            },
            new Faq
            {
                FaqId = 10,
                Question = "What is the performance and scalability of the platform?",
                Answer = "The platform is designed for high performance with map loads under 2 seconds and exports under 30 seconds. It can support up to 1000 concurrent users and uses MySQL 8.0 with GIS extensions for spatial data processing and Azure Blob Storage for file management.",
                Category = "Technical",
                CreatedAt = new DateTime(2025, 01, 15, 1, 0, 0, DateTimeKind.Utc)
            },
            new Faq
            {
                FaqId = 11,
                Question = "How do I get support if I encounter issues?",
                Answer = "You can submit support tickets through the platform, and our team will respond promptly. We also provide comprehensive documentation and FAQs. For urgent issues, please include detailed information about the problem and steps to reproduce it.",
                Category = "Support",
                CreatedAt = new DateTime(2025, 01, 15, 1, 0, 0, DateTimeKind.Utc)
            },
            new Faq
            {
                FaqId = 12,
                Question = "Is my data secure and private?",
                Answer = "Yes, we implement comprehensive security measures including JWT authentication, RBAC (Role-Based Access Control), data encryption at rest and in-transit, and audit logging for sensitive operations. Your maps can be set to private or public based on your preferences.",
                Category = "Security",
                CreatedAt = new DateTime(2025, 01, 15, 1, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
