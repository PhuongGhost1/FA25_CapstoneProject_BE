using CusomMapOSM_Domain.Entities.Locations;
using CusomMapOSM_Domain.Entities.Locations.Enums;
using CusomMapOSM_Domain.Entities.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.LocationConfig;

internal class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");

        builder.HasKey(l => l.LocationId);

        builder.Property(l => l.LocationId)
            .HasColumnName("location_id")
            .IsRequired();

        builder.Property(l => l.MapId)
            .HasColumnName("map_id")
            .IsRequired();

        builder.Property(l => l.SegmentId)
            .HasColumnName("segment_id")
            .IsRequired(false);

        builder.Property(l => l.ZoneId)
            .HasColumnName("zone_id")
            .IsRequired(false);

        builder.Property(l => l.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(l => l.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(l => l.Subtitle)
            .HasColumnName("subtitle")
            .HasMaxLength(500);

        builder.Property(l => l.Description)
            .HasColumnName("description")
            .HasColumnType("TEXT");

        builder.Property(l => l.LocationType)
            .HasColumnName("location_type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(l => l.MarkerGeometry)
            .HasColumnName("marker_geometry")
            .IsRequired()
            .HasColumnType("TEXT");

        // Visual settings
        builder.Property(l => l.IconType)
            .HasColumnName("icon_type")
            .HasMaxLength(50);

        builder.Property(l => l.IconUrl)
            .HasColumnName("icon_url")
            .HasMaxLength(1000);

        builder.Property(l => l.IconColor)
            .HasColumnName("icon_color")
            .HasMaxLength(20);

        builder.Property(l => l.IconSize)
            .HasColumnName("icon_size")
            .IsRequired()
            .HasDefaultValue(32);

        builder.Property(l => l.ZIndex)
            .HasColumnName("z_index")
            .IsRequired()
            .HasDefaultValue(100);

        builder.Property(l => l.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(l => l.ShowTooltip)
            .HasColumnName("show_tooltip")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(l => l.TooltipContent)
            .HasColumnName("tooltip_content")
            .HasColumnType("TEXT");

        builder.Property(l => l.OpenPopupOnClick)
            .HasColumnName("open_popup_on_click")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(l => l.PopupContent)
            .HasColumnName("popup_content")
            .HasColumnType("TEXT");

        // Media
        builder.Property(l => l.MediaUrls)
            .HasColumnName("media_urls")
            .HasColumnType("TEXT");

        builder.Property(l => l.PlayAudioOnClick)
            .HasColumnName("play_audio_on_click")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(l => l.AudioUrl)
            .HasColumnName("audio_url")
            .HasMaxLength(1000);

        // Entry/Exit animation
        builder.Property(l => l.EntryDelayMs)
            .HasColumnName("entry_delay_ms")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(l => l.EntryDurationMs)
            .HasColumnName("entry_duration_ms")
            .IsRequired()
            .HasDefaultValue(400);

        builder.Property(l => l.ExitDelayMs)
            .HasColumnName("exit_delay_ms")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(l => l.ExitDurationMs)
            .HasColumnName("exit_duration_ms")
            .IsRequired()
            .HasDefaultValue(400);

        builder.Property(l => l.EntryEffect)
            .HasColumnName("entry_effect")
            .HasMaxLength(50)
            .HasDefaultValue("fade");

        builder.Property(l => l.ExitEffect)
            .HasColumnName("exit_effect")
            .HasMaxLength(50)
            .HasDefaultValue("fade");

        builder.Property(l => l.LinkedLocationId)
            .HasColumnName("linked_location_id");

        builder.Property(l => l.ExternalUrl)
            .HasColumnName("external_url")
            .HasMaxLength(2000);

        // Metadata
        builder.Property(l => l.IsVisible)
            .HasColumnName("is_visible")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        // Relationships
        builder.HasOne<Map>()
            .WithMany()
            .HasForeignKey(l => l.MapId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(l => l.Segment)
            .WithMany()
            .HasForeignKey(l => l.SegmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(l => l.Zone)
            .WithMany()
            .HasForeignKey(l => l.ZoneId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(l => l.Creator)
            .WithMany()
            .HasForeignKey(l => l.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.LinkedLocation)
            .WithMany()
            .HasForeignKey(l => l.LinkedLocationId);
    }
}
