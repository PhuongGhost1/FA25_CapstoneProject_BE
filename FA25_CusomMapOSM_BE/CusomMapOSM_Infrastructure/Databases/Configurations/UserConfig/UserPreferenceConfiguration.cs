using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.UserConfig;

internal class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
       public void Configure(EntityTypeBuilder<UserPreference> builder)
       {
              builder.ToTable("user_preferences");

              builder.HasKey(up => up.UserPreferenceId);

              builder.Property(up => up.UserPreferenceId)
                     .HasColumnName("user_preference_id")
                     .IsRequired()
                     .ValueGeneratedOnAdd();

              builder.Property(up => up.Language)
                     .HasColumnName("language")
                     .IsRequired()
                     .HasMaxLength(10)
                     .HasDefaultValue("en");

              builder.Property(up => up.DefaultMapStyle)
                     .HasColumnName("default_map_style")
                     .IsRequired()
                     .HasMaxLength(50)
                     .HasDefaultValue("default");

              builder.Property(up => up.MeasurementUnit)
                     .HasColumnName("measurement_unit")
                     .IsRequired()
                     .HasMaxLength(10)
                     .HasDefaultValue("metric");

              builder.HasOne<User>()
                     .WithMany()
                     .HasForeignKey(up => up.UserId);
       }
}
