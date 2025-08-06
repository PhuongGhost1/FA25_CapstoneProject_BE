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
            .IsRequired();

        // Sample FAQ data based on URD requirements
        builder.HasData(
            new Faq
            {
                FaqId = 1,
                Question = "How do I create a map?",
                Answer = "To create a map, log in to your account, click 'Create New Map', select your desired OSM area using the bounding box tool, add layers like roads, buildings, and POIs, then customize the styling to your preference.",
                Category = "Map Creation",
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            },
            new Faq
            {
                FaqId = 2,
                Question = "What file formats can I upload?",
                Answer = "You can upload GeoJSON, KML, and CSV files up to 50MB in size. Make sure your CSV files contain coordinate columns for proper mapping.",
                Category = "Data Management",
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            },
            new Faq
            {
                FaqId = 3,
                Question = "What export formats are available?",
                Answer = "You can export your maps in PDF, PNG, SVG, GeoJSON, and MBTiles formats. Resolution options range from 72 to 300 DPI depending on your subscription plan.",
                Category = "Export System",
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            },
            new Faq
            {
                FaqId = 4,
                Question = "How do I share maps with my team?",
                Answer = "Use the collaboration feature to share maps and layers with team members. You can set permissions for view, edit, or manage access levels.",
                Category = "Collaboration",
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            },
            new Faq
            {
                FaqId = 5,
                Question = "What payment methods are accepted?",
                Answer = "We accept payments through VNPay, PayPal, and bank transfers. All transactions are secured with PCI-DSS compliance.",
                Category = "Billing",
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            },
            new Faq
            {
                FaqId = 6,
                Question = "What browsers are supported?",
                Answer = "CustomMapOSM is compatible with Chrome, Firefox, and Edge browsers. For best performance, we recommend using the latest version of these browsers.",
                Category = "Technical",
                CreatedAt = new DateTime(2025, 08, 06, 1, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
