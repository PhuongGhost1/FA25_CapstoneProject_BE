using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.AccessTools;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.AccessToolConfig;

internal class AccessToolConfiguration : IEntityTypeConfiguration<AccessTool>
{
       public void Configure(EntityTypeBuilder<AccessTool> builder)
       {
              builder.ToTable("access_tools");

              builder.HasKey(at => at.AccessToolId);
              builder.Property(at => at.AccessToolId)
                     .HasColumnName("access_tool_id")
                     .IsRequired()
                     .ValueGeneratedOnAdd();

              builder.Property(at => at.AccessToolName)
                     .IsRequired()
                     .HasMaxLength(100)
                     .HasColumnName("access_tool_name");

              builder.Property(at => at.AccessToolDescription)
                     .IsRequired()
                     .HasMaxLength(500)
                     .HasColumnName("access_tool_description");

              builder.Property(at => at.IconUrl)
                     .IsRequired()
                     .HasMaxLength(255)
                     .HasColumnName("icon_url");

              builder.Property(at => at.RequiredMembership)
       .IsRequired()
       .HasColumnName("required_membership");

              // Sample data for access tools based on system features
              builder.HasData(
                  new AccessTool
                  {
                         AccessToolId = 1,
                         AccessToolName = "Map Creation",
                         AccessToolDescription = "Create and customize maps with OSM data",
                         IconUrl = "/icons/map-create.svg",
                         RequiredMembership = false
                  },
                  new AccessTool
                  {
                         AccessToolId = 2,
                         AccessToolName = "Data Import",
                         AccessToolDescription = "Upload GeoJSON, KML, and CSV files (max 50MB)",
                         IconUrl = "/icons/data-import.svg",
                         RequiredMembership = false
                  },
                  new AccessTool
                  {
                         AccessToolId = 3,
                         AccessToolName = "Export System",
                         AccessToolDescription = "Export maps in PDF, PNG, SVG, GeoJSON, MBTiles formats",
                         IconUrl = "/icons/export.svg",
                         RequiredMembership = false
                  },
                  new AccessTool
                  {
                         AccessToolId = 4,
                         AccessToolName = "Advanced Analytics",
                         AccessToolDescription = "Advanced map analytics and reporting",
                         IconUrl = "/icons/analytics.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 5,
                         AccessToolName = "Team Collaboration",
                         AccessToolDescription = "Share maps and collaborate with team members",
                         IconUrl = "/icons/collaboration.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 6,
                         AccessToolName = "API Access",
                         AccessToolDescription = "Access to REST API for integration",
                         IconUrl = "/icons/api.svg",
                         RequiredMembership = true
                  }
              );
       }
}
