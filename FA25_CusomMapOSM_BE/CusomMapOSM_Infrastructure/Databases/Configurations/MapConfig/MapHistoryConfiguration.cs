using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.MapConfig;

internal class MapHistoryConfiguration : IEntityTypeConfiguration<MapHistory>
{
       public void Configure(EntityTypeBuilder<MapHistory> builder)
       {
              builder.ToTable("map_histories");

                             builder.HasKey(mh => mh.HistoryId);

               builder.Property(mh => mh.HistoryId)
                      .HasColumnName("history_id")
                      .IsRequired();
                      
                builder.Property(mh => mh.HistoryVersion)
                       .HasColumnName("history_version")
                       .IsRequired();

              builder.Property(mh => mh.MapId)
                     .HasColumnName("map_id")
                     .IsRequired();

              builder.Property(mh => mh.UserId)
                     .HasColumnName("user_id")
                     .IsRequired();

              builder.Property(mh => mh.SnapshotData)
                     .HasColumnName("snapshot_data")
                     .IsRequired();

              builder.Property(mh => mh.CreatedAt)
                     .HasColumnName("created_at")
                     .HasColumnType("datetime")
                     .IsRequired();
              builder.HasOne(mh => mh.Map)
                     .WithMany()
                     .HasForeignKey(mh => mh.MapId)
                     .OnDelete(DeleteBehavior.Cascade);

              builder.HasOne(mh => mh.Creator)
                     .WithMany()
                     .HasForeignKey(mh => mh.UserId)
                     .OnDelete(DeleteBehavior.Restrict);
       }
}
