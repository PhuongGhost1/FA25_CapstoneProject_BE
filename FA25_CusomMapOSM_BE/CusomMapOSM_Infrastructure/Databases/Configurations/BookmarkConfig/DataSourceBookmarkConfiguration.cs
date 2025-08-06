using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Bookmarks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.BookmarkConfig;

internal class DataSourceBookmarkConfiguration : IEntityTypeConfiguration<DataSourceBookmark>
{
       public void Configure(EntityTypeBuilder<DataSourceBookmark> builder)
       {
              builder.ToTable("data_source_bookmarks");

              // Primary key
              builder.HasKey(ds => ds.DataSourceBookmarkId);

              builder.Property(ds => ds.DataSourceBookmarkId)
                     .HasColumnName("data_source_bookmark_id")
                     .IsRequired()
                     .ValueGeneratedOnAdd();

              builder.Property(ds => ds.UserId)
                     .HasColumnName("user_id")
                     .IsRequired();

              builder.Property(ds => ds.OsmQuery)
                     .HasColumnName("osm_query")
                     .HasColumnType("text") // Use "json" if you plan to parse/query by key
                     .IsRequired();

              builder.Property(ds => ds.Name)
                     .HasColumnName("name")
                     .HasMaxLength(100)
                     .IsRequired();

              builder.Property(ds => ds.CreatedAt)
                     .HasColumnName("created_at")
                     .HasColumnType("datetime")
                     .IsRequired();
       }
}
