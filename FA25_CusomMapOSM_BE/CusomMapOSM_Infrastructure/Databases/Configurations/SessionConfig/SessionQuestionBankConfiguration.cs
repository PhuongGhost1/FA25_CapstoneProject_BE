using CusomMapOSM_Domain.Entities.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.SessionConfig;

internal class SessionQuestionBankConfiguration : IEntityTypeConfiguration<SessionQuestionBank>
{
    public void Configure(EntityTypeBuilder<SessionQuestionBank> builder)
    {
        builder.ToTable("session_question_banks");

        builder.HasKey(sqb => sqb.SessionQuestionBankId);

        builder.Property(sqb => sqb.SessionQuestionBankId)
            .HasColumnName("session_question_bank_id")
            .IsRequired();

        builder.Property(sqb => sqb.SessionId)
            .HasColumnName("session_id")
            .IsRequired();

        builder.Property(sqb => sqb.QuestionBankId)
            .HasColumnName("question_bank_id")
            .IsRequired();

        builder.Property(sqb => sqb.AttachedAt)
            .HasColumnName("attached_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.HasOne(sqb => sqb.Session)
            .WithMany()
            .HasForeignKey(sqb => sqb.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sqb => sqb.QuestionBank)
            .WithMany()
            .HasForeignKey(sqb => sqb.QuestionBankId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}

