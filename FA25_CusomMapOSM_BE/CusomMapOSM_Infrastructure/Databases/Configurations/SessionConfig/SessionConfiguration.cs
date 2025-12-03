using CusomMapOSM_Domain.Entities.Sessions;
using CusomMapOSM_Domain.Entities.Sessions.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.SessionConfig;

internal class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("sessions");

        builder.HasKey(s => s.SessionId);

        builder.Property(s => s.SessionId)
            .HasColumnName("session_id")
            .IsRequired();

        builder.Property(s => s.MapId)
            .HasColumnName("map_id")
            .IsRequired();

        builder.Property(s => s.HostUserId)
            .HasColumnName("host_user_id")
            .IsRequired();

        builder.Property(s => s.SessionCode)
            .HasColumnName("session_code")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(s => s.SessionName)
            .HasColumnName("session_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(s => s.SessionType)
            .HasColumnName("session_type")
            .HasConversion<int>()
            .HasDefaultValue(SessionTypeEnum.LIVE);

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .HasDefaultValue(SessionStatusEnum.WAITING);

        builder.Property(s => s.MaxParticipants)
            .HasColumnName("max_participants")
            .HasDefaultValue(0);

        builder.Property(s => s.AllowLateJoin)
            .HasColumnName("allow_late_join")
            .HasDefaultValue(true);

        builder.Property(s => s.ShowLeaderboard)
            .HasColumnName("show_leaderboard")
            .HasDefaultValue(true);

        builder.Property(s => s.ShowCorrectAnswers)
            .HasColumnName("show_correct_answers")
            .HasDefaultValue(true);

        builder.Property(s => s.ShuffleQuestions)
            .HasColumnName("shuffle_questions")
            .HasDefaultValue(false);

        builder.Property(s => s.ShuffleOptions)
            .HasColumnName("shuffle_options")
            .HasDefaultValue(false);

        builder.Property(s => s.EnableHints)
            .HasColumnName("enable_hints")
            .HasDefaultValue(true);

        builder.Property(s => s.PointsForSpeed)
            .HasColumnName("points_for_speed")
            .HasDefaultValue(true);

        builder.Property(s => s.ScheduledStartTime)
            .HasColumnName("scheduled_start_time")
            .HasColumnType("datetime");

        builder.Property(s => s.ActualStartTime)
            .HasColumnName("actual_start_time")
            .HasColumnType("datetime");

        builder.Property(s => s.EndTime)
            .HasColumnName("end_time")
            .HasColumnType("datetime");

        builder.Property(s => s.TotalParticipants)
            .HasColumnName("total_participants")
            .HasDefaultValue(0);

        builder.Property(s => s.TotalResponses)
            .HasColumnName("total_responses")
            .HasDefaultValue(0);

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime");

        // Relationships
        builder.HasOne(s => s.Map)
            .WithMany()
            .HasForeignKey(s => s.MapId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.HostUser)
            .WithMany()
            .HasForeignKey(s => s.HostUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(s => s.SessionCode)
            .IsUnique()
            .HasDatabaseName("UX_Session_SessionCode");

        builder.HasIndex(s => s.MapId)
            .HasDatabaseName("IX_Session_MapId");

        builder.HasIndex(s => s.HostUserId)
            .HasDatabaseName("IX_Session_HostUserId");

        builder.HasIndex(s => s.Status)
            .HasDatabaseName("IX_Session_Status");

        builder.HasIndex(s => s.SessionType)
            .HasDatabaseName("IX_Session_SessionType");

        builder.HasIndex(s => new { s.HostUserId, s.Status })
            .HasDatabaseName("IX_Session_HostUserId_Status");
    }
}