using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Bookmarks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.BookmarkConfig;

internal class BookmarkConfiguration : IEntityTypeConfiguration<Bookmark>
{
       public void Configure(EntityTypeBuilder<Bookmark> builder)
       {
              builder.ToTable("bookmarks");

              // Primary key
              builder.HasKey(b => b.BookmarkId);

              builder.Property(b => b.BookmarkId)
                     .HasColumnName("bookmark_id")
                     .IsRequired()
                     .ValueGeneratedOnAdd();

              builder.Property(b => b.MapId)
                     .HasColumnName("map_id")
                     .IsRequired();

              builder.Property(b => b.UserId)
                     .HasColumnName("user_id")
                     .IsRequired();

              builder.Property(b => b.Name)
                     .HasColumnName("name")
                     .HasMaxLength(100)
                     .IsRequired(false);

              builder.Property(b => b.ViewState)
                     .HasColumnName("view_state")
                     .HasColumnType("json") // or "longtext" if not querying json
                     .IsRequired(false);

              builder.Property(b => b.CreatedAt)
                     .HasColumnName("created_at")
                     .HasColumnType("datetime")
                     .IsRequired();

              // Relationships
              builder.HasOne(b => b.Map)
                     .WithMany()
                     .HasForeignKey(b => b.MapId)
                     .OnDelete(DeleteBehavior.Cascade);

              builder.HasOne(b => b.User)
                     .WithMany()
                     .HasForeignKey(b => b.UserId)
                     .OnDelete(DeleteBehavior.Cascade);
       }
}
