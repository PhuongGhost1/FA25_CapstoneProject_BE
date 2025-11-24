using CusomMapOSM_Domain.Entities.QuestionBanks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.QuestionBankConfig;

internal class MapQuestionBankConfiguration : IEntityTypeConfiguration<MapQuestionBank>
{
    public void Configure(EntityTypeBuilder<MapQuestionBank> builder)
    {
        builder.ToTable("map_question_banks");

        builder.HasKey(mqb => new { mqb.MapId, mqb.QuestionBankId });

        builder.Property(mqb => mqb.MapId)
            .HasColumnName("map_id")
            .IsRequired();

        builder.Property(mqb => mqb.QuestionBankId)
            .HasColumnName("question_bank_id")
            .IsRequired();

        builder.Property(mqb => mqb.AssignedAt)
            .HasColumnName("assigned_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(mqb => mqb.Map)
            .WithMany(m => m.MapQuestionBanks)
            .HasForeignKey(mqb => mqb.MapId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mqb => mqb.QuestionBank)
            .WithMany()
            .HasForeignKey(mqb => mqb.QuestionBankId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
