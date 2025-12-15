using CusomMapOSM_Domain.Entities.QuestionBanks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.QuestionBankConfig;

internal class QuestionBankConfiguration : IEntityTypeConfiguration<QuestionBank>
{
    public void Configure(EntityTypeBuilder<QuestionBank> builder)
    {
        builder.ToTable("question_banks");

        builder.HasKey(qb => qb.QuestionBankId);

        builder.Property(qb => qb.QuestionBankId)
            .HasColumnName("question_bank_id")
            .IsRequired();

        builder.Property(qb => qb.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(qb => qb.WorkspaceId)
            .HasColumnName("workspace_id");

        builder.Property(qb => qb.BankName)
            .HasColumnName("bank_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(qb => qb.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(qb => qb.Category)
            .HasColumnName("category")
            .HasMaxLength(100);

        builder.Property(qb => qb.Tags)
            .HasColumnName("tags")
            .HasMaxLength(500);

        builder.Property(qb => qb.TotalQuestions)
            .HasColumnName("total_questions")
            .HasDefaultValue(0);

        builder.Property(qb => qb.IsTemplate)
            .HasColumnName("is_template")
            .HasDefaultValue(false);

        builder.Property(qb => qb.IsPublic)
            .HasColumnName("is_public")
            .HasDefaultValue(false);

        builder.Property(qb => qb.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(qb => qb.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(qb => qb.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");
        
        builder.HasOne(qb => qb.User)
            .WithMany()
            .HasForeignKey(qb => qb.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(qb => qb.Workspace)
            .WithMany()
            .HasForeignKey(qb => qb.WorkspaceId);
    }
}