using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.UserConfig;

internal class UserFavoriteTemplateConfiguration : IEntityTypeConfiguration<UserFavoriteTemplate>
{
       public void Configure(EntityTypeBuilder<UserFavoriteTemplate> builder)
       {
              builder.ToTable("user_favorite_templates");

              builder.HasKey(uft => uft.UserFavoriteTemplateId);

              builder.Property(uft => uft.UserFavoriteTemplateId)
                     .HasColumnName("user_favorite_template_id")
                     .IsRequired()
                     .ValueGeneratedOnAdd();

              builder.Property(uft => uft.UserId)
                     .HasColumnName("user_id")
                     .IsRequired();

              builder.Property(uft => uft.TemplateId)
                     .HasColumnName("template_id")
                     .IsRequired();

              builder.Property(uft => uft.FavoriteAt)
                     .HasColumnName("favorite_at")
                     .HasColumnType("datetime")
                     .IsRequired();
              
              builder.HasOne(uft => uft.User)
                     .WithMany()
                     .HasForeignKey(uft => uft.UserId)
                     .OnDelete(DeleteBehavior.Cascade);

              builder.HasOne(uft => uft.Template)
                     .WithMany()
                     .HasForeignKey(uft => uft.TemplateId)
                     .OnDelete(DeleteBehavior.Cascade);
              
       }
}
