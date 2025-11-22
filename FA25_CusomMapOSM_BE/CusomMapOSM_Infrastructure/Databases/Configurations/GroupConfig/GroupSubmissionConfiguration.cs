using CusomMapOSM_Domain.Entities.Groups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.GroupConfig;

internal class GroupSubmissionConfiguration : IEntityTypeConfiguration<GroupSubmission>
{
    public void Configure(EntityTypeBuilder<GroupSubmission> builder)
    {
        builder.ToTable("group_submissions");

        builder.HasKey(s => s.SubmissionId);

        builder.Property(s => s.SubmissionId)
            .HasColumnName("submission_id")
            .IsRequired();

        builder.Property(s => s.GroupId)
            .HasColumnName("group_id")
            .IsRequired();

        builder.Property(s => s.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Content)
            .HasColumnName("content")
            .HasColumnType("text");

        builder.Property(s => s.AttachmentUrls)
            .HasColumnName("attachment_urls")
            .HasColumnType("json");

        builder.Property(s => s.Score)
            .HasColumnName("score");

        builder.Property(s => s.Feedback)
            .HasColumnName("feedback")
            .HasColumnType("text");

        builder.Property(s => s.SubmittedAt)
            .HasColumnName("submitted_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(s => s.GradedAt)
            .HasColumnName("graded_at")
            .HasColumnType("datetime");

        // Relationships
        builder.HasOne(s => s.Group)
            .WithMany()
            .HasForeignKey(s => s.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}
