using CusomMapOSM_Domain.Entities.Sessions;
using CusomMapOSM_Domain.Entities.Sessions.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.SessionConfig;

internal class SessionQuestionConfiguration : IEntityTypeConfiguration<SessionQuestion>
{
    public void Configure(EntityTypeBuilder<SessionQuestion> builder)
    {
        builder.ToTable("session_questions");

        builder.HasKey(sq => sq.SessionQuestionId);

        builder.Property(sq => sq.SessionQuestionId)
            .HasColumnName("session_question_id")
            .IsRequired();

        builder.Property(sq => sq.SessionId)
            .HasColumnName("session_id")
            .IsRequired();

        builder.Property(sq => sq.QuestionId)
            .HasColumnName("question_id")
            .IsRequired();

        builder.Property(sq => sq.QueueOrder)
            .HasColumnName("queue_order")
            .HasDefaultValue(0);

        builder.Property(sq => sq.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .HasDefaultValue(SessionQuestionStatusEnum.QUEUED);

        builder.Property(sq => sq.PointsOverride)
            .HasColumnName("points_override");

        builder.Property(sq => sq.TimeLimitOverride)
            .HasColumnName("time_limit_override");

        builder.Property(sq => sq.TimeLimitExtensions)
            .HasColumnName("time_limit_extensions")
            .HasDefaultValue(0);

        builder.Property(sq => sq.StartedAt)
            .HasColumnName("started_at")
            .HasColumnType("datetime");

        builder.Property(sq => sq.EndedAt)
            .HasColumnName("ended_at")
            .HasColumnType("datetime");

        builder.Property(sq => sq.TotalResponses)
            .HasColumnName("total_responses")
            .HasDefaultValue(0);

        builder.Property(sq => sq.CorrectResponses)
            .HasColumnName("correct_responses")
            .HasDefaultValue(0);

        builder.Property(sq => sq.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(sq => sq.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        // Relationships
        builder.HasOne(sq => sq.Session)
            .WithMany()
            .HasForeignKey(sq => sq.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sq => sq.Question)
            .WithMany()
            .HasForeignKey(sq => sq.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(sq => sq.SessionId)
            .HasDatabaseName("IX_SessionQuestion_SessionId");

        builder.HasIndex(sq => sq.QuestionId)
            .HasDatabaseName("IX_SessionQuestion_QuestionId");

        builder.HasIndex(sq => new { sq.SessionId, sq.QueueOrder })
            .IsUnique()
            .HasDatabaseName("UX_SessionQuestion_SessionId_QueueOrder");

        builder.HasIndex(sq => sq.Status)
            .HasDatabaseName("IX_SessionQuestion_Status");
    }
}