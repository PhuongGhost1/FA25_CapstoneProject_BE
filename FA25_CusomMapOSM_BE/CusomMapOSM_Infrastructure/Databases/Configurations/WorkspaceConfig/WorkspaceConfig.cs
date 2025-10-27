using CusomMapOSM_Domain.Entities.Workspaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.WorkspaceConfig;

internal class WorkspaceConfig : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("workspaces");

        builder.HasKey(w => w.WorkspaceId);

        builder.Property(w => w.WorkspaceId)
            .HasColumnName("workspace_id")
            .IsRequired();

        builder.Property(w => w.OrgId)
            .HasColumnName("org_id")
            .IsRequired();

        builder.Property(w => w.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(w => w.WorkspaceName)
            .HasColumnName("workspace_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(w => w.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(w => w.Icon)
            .HasColumnName("icon")
            .HasMaxLength(500);

        builder.Property(w => w.Access)
            .HasColumnName("access")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(w => w.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(w => w.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.HasOne(w => w.Organization)
            .WithMany()
            .HasForeignKey(w => w.OrgId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Creator)
            .WithMany()
            .HasForeignKey(w => w.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}