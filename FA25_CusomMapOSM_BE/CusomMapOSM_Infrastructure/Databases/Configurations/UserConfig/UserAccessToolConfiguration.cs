using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.UserConfig;

internal class UserAccessToolConfiguration : IEntityTypeConfiguration<UserAccessTool>
{
       public void Configure(EntityTypeBuilder<UserAccessTool> builder)
       {
              builder.ToTable("user_access_tools");

              builder.HasKey(uat => uat.UserAccessToolId);

              builder.Property(uat => uat.UserAccessToolId)
                     .HasColumnName("user_access_tool_id")
                     .IsRequired()
                     .ValueGeneratedOnAdd();

              builder.Property(uat => uat.GrantedAt)
                     .HasColumnName("granted_at")
                     .HasColumnType("datetime")
                     .IsRequired();

              builder.HasOne(uat => uat.User)
                     .WithMany()
                     .HasForeignKey(uat => uat.UserId);

              builder.HasOne(uat => uat.AccessTool)
                     .WithMany()
                     .HasForeignKey(uat => uat.AccessToolId);
       }
}
