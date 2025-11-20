using CusomMapOSM_Domain.Entities.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.SessionConfig;

internal class SessionParticipantConfiguration : IEntityTypeConfiguration<SessionParticipant>
{
    public void Configure(EntityTypeBuilder<SessionParticipant> builder)
    {
        builder.ToTable("session_participants");

        builder.HasKey(sp => sp.SessionParticipantId);

        builder.Property(sp => sp.SessionParticipantId)
            .HasColumnName("session_participant_id")
            .IsRequired();

        builder.Property(sp => sp.SessionId)
            .HasColumnName("session_id")
            .IsRequired();

        builder.Property(sp => sp.UserId)
            .HasColumnName("user_id");

        builder.Property(sp => sp.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(sp => sp.IsGuest)
            .HasColumnName("is_guest")
            .HasDefaultValue(false);

        builder.Property(sp => sp.JoinedAt)
            .HasColumnName("joined_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(sp => sp.LeftAt)
            .HasColumnName("left_at")
            .HasColumnType("datetime");

        builder.Property(sp => sp.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(sp => sp.TotalScore)
            .HasColumnName("total_score")
            .HasDefaultValue(0);

        builder.Property(sp => sp.TotalCorrect)
            .HasColumnName("total_correct")
            .HasDefaultValue(0);

        builder.Property(sp => sp.TotalAnswered)
            .HasColumnName("total_answered")
            .HasDefaultValue(0);

        builder.Property(sp => sp.AverageResponseTime)
            .HasColumnName("average_response_time")
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0);

        builder.Property(sp => sp.Rank)
            .HasColumnName("rank")
            .HasDefaultValue(0);

        builder.Property(sp => sp.DeviceInfo)
            .HasColumnName("device_info")
            .HasMaxLength(500);

        builder.Property(sp => sp.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(50);

        builder.Property(sp => sp.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(sp => sp.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        // Relationships
        builder.HasOne(sp => sp.Session)
            .WithMany(s => s.SessionParticipants)
            .HasForeignKey(sp => sp.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sp => sp.User)
            .WithMany()
            .HasForeignKey(sp => sp.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(sp => sp.StudentResponses)
            .WithOne(sr => sr.SessionParticipant)
            .HasForeignKey(sr => sr.SessionParticipantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(sp => sp.SessionId)
            .HasDatabaseName("IX_SessionParticipant_SessionId");

        builder.HasIndex(sp => sp.UserId)
            .HasDatabaseName("IX_SessionParticipant_UserId");

        // Composite index for leaderboard queries
        builder.HasIndex(sp => new { sp.SessionId, sp.TotalScore })
            .HasDatabaseName("IX_SessionParticipant_SessionId_TotalScore");

        // Ensure one user can only join a session once
        builder.HasIndex(sp => new { sp.SessionId, sp.UserId })
            .IsUnique()
            .HasFilter("user_id IS NOT NULL")
            .HasDatabaseName("UX_SessionParticipant_SessionId_UserId");

        builder.HasIndex(sp => sp.IsActive)
            .HasDatabaseName("IX_SessionParticipant_IsActive");
    }
}