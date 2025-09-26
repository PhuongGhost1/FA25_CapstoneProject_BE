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

              builder.Property(u => u.AccountStatus)
                     .HasColumnName("account_status")
                     .HasConversion<int>()
                     .IsRequired();

              builder.Property(u => u.MonthlyTokenUsage)
                     .HasColumnName("monthly_token_usage")
                     .HasDefaultValue(0);

              builder.Property(u => u.LastTokenReset)
                     .HasColumnName("last_token_reset")
                     .HasColumnType("datetime");

              builder.Property(u => u.CreatedAt)
                     .HasColumnName("created_at")
                     .HasColumnType("datetime")
                     .HasDefaultValueSql("CURRENT_TIMESTAMP");

              builder.Property(u => u.LastLogin)
                     .HasColumnName("last_login")
                     .HasColumnType("datetime");

              builder.HasOne(u => u.Role)
                     .WithMany()
                     .HasForeignKey(u => u.RoleId);
       }
}
