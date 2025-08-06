using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Advertisements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.AdvertisementConfig;

internal class AdvertisementConfiguration : IEntityTypeConfiguration<Advertisement>
{
       public void Configure(EntityTypeBuilder<Advertisement> builder)
       {
              builder.ToTable("advertisements");

              builder.HasKey(ad => ad.AdvertisementId);
              builder.Property(ad => ad.AdvertisementId)
                     .HasColumnName("advertisement_id")
                     .IsRequired()
                     .ValueGeneratedOnAdd();

              builder.Property(ad => ad.AdvertisementTitle)
                     .IsRequired()
                     .HasMaxLength(200)
                     .HasColumnName("advertisement_title");

              builder.Property(ad => ad.AdvertisementContent)
                     .IsRequired()
                     .HasMaxLength(1000)
                     .HasColumnName("advertisement_content");

              builder.Property(ad => ad.ImageUrl)
                     .IsRequired()
                     .HasMaxLength(500)
                     .HasColumnName("image_url");

              builder.Property(ad => ad.StartDate)
                     .IsRequired()
                     .HasColumnName("start_date")
                     .HasColumnType("datetime");

              builder.Property(ad => ad.EndDate)
                     .IsRequired()
                     .HasColumnName("end_date")
                     .HasColumnType("datetime");

              builder.Property(ad => ad.IsActive)
                     .IsRequired()
                     .HasColumnName("is_active");
       }
}
