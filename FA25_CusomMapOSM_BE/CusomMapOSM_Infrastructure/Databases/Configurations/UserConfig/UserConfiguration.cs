using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.UserConfig;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
       public void Configure(EntityTypeBuilder<User> builder)
       {
              builder.ToTable("users");

              builder.HasKey(u => u.UserId);

              builder.Property(u => u.UserId)
                     .HasColumnName("user_id")
                     .IsRequired();

              builder.Property(u => u.Email)
                     .HasColumnName("email")
                     .IsRequired()
                     .HasMaxLength(100);

              builder.Property(u => u.PasswordHash)
                     .HasColumnName("password_hash")
                     .IsRequired();

              builder.Property(u => u.FullName)
                     .HasColumnName("full_name")
                     .HasMaxLength(100);

              builder.Property(u => u.Phone)
                     .HasColumnName("phone")
                     .HasMaxLength(20);

              builder.Property(u => u.CreatedAt)
                     .HasColumnName("created_at")
                     .HasColumnType("datetime")
                     .HasDefaultValueSql("CURRENT_TIMESTAMP");

              builder.HasOne(u => u.Role)
                     .WithMany()
                     .HasForeignKey(u => u.RoleId);

              builder.HasOne(u => u.AccountStatus)
                     .WithMany()
                     .HasForeignKey(u => u.AccountStatusId);
       }
}
