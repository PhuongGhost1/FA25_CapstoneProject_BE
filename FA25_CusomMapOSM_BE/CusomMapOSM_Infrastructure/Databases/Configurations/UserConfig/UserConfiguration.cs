using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Users.Enums;
using CusomMapOSM_Infrastructure.Databases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.UserConfig;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
       private static string HashObject<T>(T obj)
       {
              string json = JsonConvert.SerializeObject(obj);

              using (SHA256 sha256 = SHA256.Create())
              {
                     byte[] bytes = Encoding.UTF8.GetBytes(json);
                     byte[] hashBytes = sha256.ComputeHash(bytes);

                     StringBuilder hashString = new StringBuilder();
                     foreach (byte b in hashBytes)
                     {
                            hashString.Append(b.ToString("x2"));
                     }

                     return hashString.ToString();
              }
       }

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

              builder.Property(u => u.Role)
                     .HasColumnName("role")
                     .HasConversion<string>()
                     .HasMaxLength(50)
                     .IsRequired();

              // Seed admin users
              builder.HasData(
                  new User
                  {
                         UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                         Email = "admin@cusommaposm.com",
                         PasswordHash = HashObject<string>("Admin123!"), // Default password
                         FullName = "System Administrator",
                         Phone = "+1234567890",
                         Role = UserRoleEnum.Admin,
                         AccountStatus = AccountStatusEnum.Active,
                         CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                         MonthlyTokenUsage = 0,
                         LastTokenReset = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                  }
              //     new User
              //     {
              //            UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
              //            Email = "superadmin@cusommaposm.com",
              //            PasswordHash = HashPassword("SuperAdmin123!"), // Default password
              //            FullName = "Super Administrator",
              //            Phone = "+1234567891",
              //            RoleId = SeedDataConstants.AdminRoleId,
              //            AccountStatus = AccountStatusEnum.Active,
              //            CreatedAt = DateTime.UtcNow,
              //            MonthlyTokenUsage = 0,
              //            LastTokenReset = DateTime.UtcNow
              //     },
              //     new User
              //     {
              //            UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
              //            Email = "staff@cusommaposm.com",
              //            PasswordHash = HashPassword("Staff123!"), // Default password
              //            FullName = "Staff Member",
              //            Phone = "+1234567892",
              //            RoleId = SeedDataConstants.StaffRoleId,
              //            AccountStatus = AccountStatusEnum.Active,
              //            CreatedAt = DateTime.UtcNow,
              //            MonthlyTokenUsage = 0,
              //            LastTokenReset = DateTime.UtcNow
              //     }
              );
       }
}
