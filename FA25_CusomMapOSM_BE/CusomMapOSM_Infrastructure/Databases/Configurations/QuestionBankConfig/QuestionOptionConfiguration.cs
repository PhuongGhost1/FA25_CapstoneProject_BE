using CusomMapOSM_Domain.Entities.QuestionBanks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.QuestionBankConfig;

internal class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.ToTable("question_options");

        builder.HasKey(qo => qo.QuestionOptionId);

        builder.Property(qo => qo.QuestionOptionId)
            .HasColumnName("question_option_id")
            .IsRequired();

        builder.Property(qo => qo.QuestionId)
            .HasColumnName("question_id")
            .IsRequired();

        builder.Property(qo => qo.OptionText)
            .HasColumnName("option_text")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(qo => qo.OptionImageUrl)
            .HasColumnName("option_image_url")
            .HasMaxLength(500);

        builder.Property(qo => qo.IsCorrect)
            .HasColumnName("is_correct")
            .HasDefaultValue(false);

        builder.Property(qo => qo.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(qo => qo.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(qo => qo.Question)
            .WithMany(q => q.QuestionOptions)
            .HasForeignKey(qo => qo.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}