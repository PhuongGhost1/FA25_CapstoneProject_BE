using CusomMapOSM_Domain.Entities.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.SessionConfig;

internal class StudentResponseConfiguration : IEntityTypeConfiguration<StudentResponse>
{
    public void Configure(EntityTypeBuilder<StudentResponse> builder)
    {
        builder.ToTable("student_responses");

        builder.HasKey(sr => sr.StudentResponseId);

        builder.Property(sr => sr.StudentResponseId)
            .HasColumnName("student_response_id")
            .IsRequired();

        builder.Property(sr => sr.SessionQuestionId)
            .HasColumnName("session_question_id")
            .IsRequired();

        builder.Property(sr => sr.SessionParticipantId)
            .HasColumnName("session_participant_id")
            .IsRequired();

        builder.Property(sr => sr.QuestionOptionId)
            .HasColumnName("question_option_id");

        builder.Property(sr => sr.ResponseText)
            .HasColumnName("response_text")
            .HasMaxLength(1000);

        builder.Property(sr => sr.ResponseLatitude)
            .HasColumnName("response_latitude")
            .HasColumnType("decimal(10,7)");

        builder.Property(sr => sr.ResponseLongitude)
            .HasColumnName("response_longitude")
            .HasColumnType("decimal(10,7)");

        builder.Property(sr => sr.IsCorrect)
            .HasColumnName("is_correct")
            .HasDefaultValue(false);

        builder.Property(sr => sr.PointsEarned)
            .HasColumnName("points_earned")
            .HasDefaultValue(0);

        builder.Property(sr => sr.ResponseTimeSeconds)
            .HasColumnName("response_time_seconds")
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0);

        builder.Property(sr => sr.UsedHint)
            .HasColumnName("used_hint")
            .HasDefaultValue(false);

        builder.Property(sr => sr.DistanceErrorMeters)
            .HasColumnName("distance_error_meters")
            .HasColumnType("decimal(10,2)");

        builder.Property(sr => sr.SubmittedAt)
            .HasColumnName("submitted_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(sr => sr.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(sr => sr.SessionQuestion)
            .WithMany()
            .HasForeignKey(sr => sr.SessionQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sr => sr.SessionParticipant)
            .WithMany(sp => sp.StudentResponses)
            .HasForeignKey(sr => sr.SessionParticipantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sr => sr.QuestionOption)
            .WithMany()
            .HasForeignKey(sr => sr.QuestionOptionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(sr => sr.SessionQuestionId)
            .HasDatabaseName("IX_StudentResponse_SessionQuestionId");

        builder.HasIndex(sr => sr.SessionParticipantId)
            .HasDatabaseName("IX_StudentResponse_SessionParticipantId");

        builder.HasIndex(sr => sr.QuestionOptionId)
            .HasDatabaseName("IX_StudentResponse_QuestionOptionId");

        builder.HasIndex(sr => sr.IsCorrect)
            .HasDatabaseName("IX_StudentResponse_IsCorrect");

        // Composite index for analytics queries
        builder.HasIndex(sr => new { sr.SessionQuestionId, sr.IsCorrect })
            .HasDatabaseName("IX_StudentResponse_SessionQuestionId_IsCorrect");

        // Prevent duplicate responses from same participant for same question
        builder.HasIndex(sr => new { sr.SessionQuestionId, sr.SessionParticipantId })
            .IsUnique()
            .HasDatabaseName("UX_StudentResponse_SessionQuestionId_ParticipantId");
    }
}