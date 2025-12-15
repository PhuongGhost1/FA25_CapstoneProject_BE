using CusomMapOSM_Domain.Entities.Groups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.GroupConfig;

internal class SessionGroupMemberConfiguration : IEntityTypeConfiguration<SessionGroupMember>
{
    public void Configure(EntityTypeBuilder<SessionGroupMember> builder)
    {
        builder.ToTable("session_group_members");

        builder.HasKey(m => m.GroupMemberId);

        builder.Property(m => m.GroupMemberId)
            .HasColumnName("group_member_id")
            .IsRequired();

        builder.Property(m => m.GroupId)
            .HasColumnName("group_id")
            .IsRequired();

        builder.Property(m => m.SessionParticipantId)
            .HasColumnName("session_participant_id")
            .IsRequired();

        builder.Property(m => m.IsLeader)
            .HasColumnName("is_leader")
            .HasDefaultValue(false);

        builder.Property(m => m.JoinedAt)
            .HasColumnName("joined_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(m => m.Group)
            .WithMany()
            .HasForeignKey(m => m.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.SessionParticipant)
            .WithMany()
            .HasForeignKey(m => m.SessionParticipantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
