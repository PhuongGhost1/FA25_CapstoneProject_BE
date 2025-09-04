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
                  // Free Tools (1-11)
                  new AccessTool
                  {
                         AccessToolId = 1,
                         AccessToolName = "Pin",
                         AccessToolDescription = "Add pin markers to maps",
                         IconUrl = "/icons/pin.svg",
                         RequiredMembership = false
                  },
                  new AccessTool
                  {
                         AccessToolId = 2,
                         AccessToolName = "Line",
                         AccessToolDescription = "Draw lines on maps",
                         IconUrl = "/icons/line.svg",
                         RequiredMembership = false
                  },
                  new AccessTool
                  {
                         AccessToolId = 3,
                         AccessToolName = "Route",
                         AccessToolDescription = "Create and display routes",
                         IconUrl = "/icons/route.svg",
                         RequiredMembership = false
                  },
                  new AccessTool
                  {
                         AccessToolId = 4,
                         AccessToolName = "Polygon",
                         AccessToolDescription = "Draw polygon shapes on maps",
                         IconUrl = "/icons/polygon.svg",
                         RequiredMembership = false
                  },
                  new AccessTool
                  {
                         AccessToolId = 5,
                         AccessToolName = "Circle",
                         AccessToolDescription = "Draw circular areas on maps",
                         IconUrl = "/icons/circle.svg",
                         RequiredMembership = false
                  },
                  new AccessTool
                  {
                         AccessToolId = 6,
                         AccessToolName = "Marker",
                         AccessToolDescription = "Add custom markers to maps",
                         IconUrl = "/icons/marker.svg",
                         RequiredMembership = false
                  },
                  new AccessTool
                  {
                         AccessToolId = 7,
                         AccessToolName = "Highlighter",
                         AccessToolDescription = "Highlight areas on maps",
                         IconUrl = "/icons/highlighter.svg",
                         RequiredMembership = false
                  },
                  new AccessTool
                  {
                         AccessToolId = 8,
                         AccessToolName = "Text",
                         AccessToolDescription = "Add text annotations to maps",
                         IconUrl = "/icons/text.svg",
                         RequiredMembership = false
                  },
                  new AccessTool
                  {
                         AccessToolId = 9,
                         AccessToolName = "Note",
                         AccessToolDescription = "Add notes to map locations",
                         IconUrl = "/icons/note.svg",
                         RequiredMembership = false
                  },
                  new AccessTool
                  {
                         AccessToolId = 10,
                         AccessToolName = "Link",
                         AccessToolDescription = "Add clickable links to map elements",
                         IconUrl = "/icons/link.svg",
                         RequiredMembership = false
                  },
                  new AccessTool
                  {
                         AccessToolId = 11,
                         AccessToolName = "Video",
                         AccessToolDescription = "Embed videos in map popups",
                         IconUrl = "/icons/video.svg",
                         RequiredMembership = false
                  },
                  // Pro Tools (12-28)
                  new AccessTool
                  {
                         AccessToolId = 12,
                         AccessToolName = "Bounds",
                         AccessToolDescription = "Calculate and display map bounds",
                         IconUrl = "/icons/bounds.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 13,
                         AccessToolName = "Buffer",
                         AccessToolDescription = "Create buffer zones around features",
                         IconUrl = "/icons/buffer.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 14,
                         AccessToolName = "Centroid",
                         AccessToolDescription = "Calculate centroids of features",
                         IconUrl = "/icons/centroid.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 15,
                         AccessToolName = "Dissolve",
                         AccessToolDescription = "Dissolve overlapping features",
                         IconUrl = "/icons/dissolve.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 16,
                         AccessToolName = "Clip",
                         AccessToolDescription = "Clip features to specified boundaries",
                         IconUrl = "/icons/clip.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 17,
                         AccessToolName = "Count Points",
                         AccessToolDescription = "Count points within areas",
                         IconUrl = "/icons/count-points.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 18,
                         AccessToolName = "Intersect",
                         AccessToolDescription = "Find intersections between features",
                         IconUrl = "/icons/intersect.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 19,
                         AccessToolName = "Join",
                         AccessToolDescription = "Join data from different sources",
                         IconUrl = "/icons/join.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 20,
                         AccessToolName = "Subtract",
                         AccessToolDescription = "Subtract one feature from another",
                         IconUrl = "/icons/subtract.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 21,
                         AccessToolName = "Statistic",
                         AccessToolDescription = "Generate statistical analysis",
                         IconUrl = "/icons/statistic.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 22,
                         AccessToolName = "Bar Chart",
                         AccessToolDescription = "Create bar charts from map data",
                         IconUrl = "/icons/bar-chart.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 23,
                         AccessToolName = "Histogram",
                         AccessToolDescription = "Generate histograms from data",
                         IconUrl = "/icons/histogram.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 24,
                         AccessToolName = "Filter",
                         AccessToolDescription = "Filter map data by criteria",
                         IconUrl = "/icons/filter.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 25,
                         AccessToolName = "Time Series",
                         AccessToolDescription = "Analyze data over time",
                         IconUrl = "/icons/time-series.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 26,
                         AccessToolName = "Find",
                         AccessToolDescription = "Search and find features",
                         IconUrl = "/icons/find.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 27,
                         AccessToolName = "Measure",
                         AccessToolDescription = "Measure distances and areas",
                         IconUrl = "/icons/measure.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 28,
                         AccessToolName = "Spatial Filter",
                         AccessToolDescription = "Filter by spatial relationships",
                         IconUrl = "/icons/spatial-filter.svg",
                         RequiredMembership = true
                  },
                  // Premium Tools (29-31)
                  new AccessTool
                  {
                         AccessToolId = 29,
                         AccessToolName = "Custom Extension",
                         AccessToolDescription = "Create custom map extensions",
                         IconUrl = "/icons/custom-extension.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 30,
                         AccessToolName = "Custom Popup",
                         AccessToolDescription = "Design custom popup templates",
                         IconUrl = "/icons/custom-popup.svg",
                         RequiredMembership = true
                  },
                  new AccessTool
                  {
                         AccessToolId = 31,
                         AccessToolName = "AI Suggestion",
                         AccessToolDescription = "Get AI-powered map suggestions",
                         IconUrl = "/icons/ai-suggestion.svg",
                         RequiredMembership = true
                  }
              );
       }
}
