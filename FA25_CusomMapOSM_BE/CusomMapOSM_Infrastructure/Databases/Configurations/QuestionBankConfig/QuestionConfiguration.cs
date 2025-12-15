using CusomMapOSM_Domain.Entities.QuestionBanks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.QuestionBankConfig;

internal class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("questions");

        builder.HasKey(q => q.QuestionId);

        builder.Property(q => q.QuestionId)
            .HasColumnName("question_id")
            .IsRequired();

        builder.Property(q => q.QuestionBankId)
            .HasColumnName("question_bank_id")
            .IsRequired();

        builder.Property(q => q.LocationId)
            .HasColumnName("location_id");

        builder.Property(q => q.QuestionType)
            .HasColumnName("question_type")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(q => q.QuestionText)
            .HasColumnName("question_text")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(q => q.QuestionImageUrl)
            .HasColumnName("question_image_url")
            .HasMaxLength(500);

        builder.Property(q => q.QuestionAudioUrl)
            .HasColumnName("question_audio_url")
            .HasMaxLength(500);

        builder.Property(q => q.Points)
            .HasColumnName("points")
            .HasDefaultValue(100);

        builder.Property(q => q.TimeLimit)
            .HasColumnName("time_limit")
            .HasDefaultValue(30);

        builder.Property(q => q.CorrectAnswerText)
            .HasColumnName("correct_answer_text")
            .HasMaxLength(500);

        builder.Property(q => q.CorrectLatitude)
            .HasColumnName("correct_latitude")
            .HasColumnType("decimal(10,7)");

        builder.Property(q => q.CorrectLongitude)
            .HasColumnName("correct_longitude")
            .HasColumnType("decimal(10,7)");

        builder.Property(q => q.AcceptanceRadiusMeters)
            .HasColumnName("acceptance_radius_meters");

        builder.Property(q => q.HintText)
            .HasColumnName("hint_text")
            .HasColumnType("text");

        builder.Property(q => q.Explanation)
            .HasColumnName("explanation")
            .HasColumnType("text");

        builder.Property(q => q.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(q => q.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(q => q.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(q => q.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        // Relationships
        builder.HasOne(q => q.QuestionBank)
            .WithMany()
            .HasForeignKey(q => q.QuestionBankId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(q => q.Location)
            .WithMany()
            .HasForeignKey(q => q.LocationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}