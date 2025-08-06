using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.UserConfig;

internal class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");

        builder.HasKey(r => r.RoleId);

        builder.Property(r => r.RoleId)
               .HasColumnName("role_id")
               .IsRequired();

        builder.Property(r => r.Name)
               .HasColumnName("name")
               .IsRequired()
               .HasMaxLength(50);

        // Sample data based on URD requirements
        builder.HasData(
            new UserRole { RoleId = SeedDataConstants.StaffRoleId, Name = "Staff" },
            new UserRole { RoleId = SeedDataConstants.RegisteredUserRoleId, Name = "Registered User" },
            new UserRole { RoleId = SeedDataConstants.AdminRoleId, Name = "Administrator" }
        );
    }
}