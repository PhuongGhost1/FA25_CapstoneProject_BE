using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Segments.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.SegmentConfig;

internal class SegmentConfiguration : IEntityTypeConfiguration<Segment>
{
    public void Configure(EntityTypeBuilder<Segment> builder)
    {
        builder.ToTable("segments");

        builder.HasKey(s => s.SegmentId);

        builder.Property(s => s.SegmentId)
            .HasColumnName("segment_id")
            .IsRequired();

        builder.Property(s => s.MapId)
            .HasColumnName("map_id")
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(s => s.StoryContent)
            .HasColumnName("story_content")
            .HasColumnType("TEXT");

        builder.Property(s => s.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.CameraState)
            .HasColumnName("camera_state")
            .IsRequired()
            .HasColumnType("TEXT");

        // Playback settings
        builder.Property(s => s.AutoAdvance)
            .HasColumnName("auto_advance")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.DurationMs)
            .HasColumnName("duration_ms")
            .IsRequired()
            .HasDefaultValue(6000);

        builder.Property(s => s.RequireUserAction)
            .HasColumnName("require_user_action")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        // Relationships
        builder.HasOne(s => s.Map)
            .WithMany()
            .HasForeignKey(s => s.MapId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Creator)
            .WithMany()
            .HasForeignKey(s => s.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

    }
}
