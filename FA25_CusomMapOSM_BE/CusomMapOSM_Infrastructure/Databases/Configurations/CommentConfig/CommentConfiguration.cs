using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Comments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CusomMapOSM_Infrastructure.Databases.Configurations.CommentConfig;

internal class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
       public void Configure(EntityTypeBuilder<Comment> builder)
       {
              builder.ToTable("comments");

              builder.HasKey(c => c.CommentId);

              builder.Property(c => c.CommentId)
                     .HasColumnName("comment_id")
                     .IsRequired()
                     .ValueGeneratedOnAdd();

              builder.Property(c => c.MapId)
                     .HasColumnName("map_id")
                     .IsRequired();

              builder.Property(c => c.LayerId)
                     .HasColumnName("layer_id");

              builder.Property(c => c.UserId)
                     .HasColumnName("user_id");

              builder.Property(c => c.Content)
                     .IsRequired()
                     .HasColumnName("content")
                     .HasMaxLength(1000);

              builder.Property(c => c.Position)
                     .HasColumnName("position")
                     .HasMaxLength(255);

              builder.Property(c => c.CreatedAt)
                     .HasColumnName("created_at")
                     .HasColumnType("datetime")
                     .HasDefaultValueSql("CURRENT_TIMESTAMP");

              builder.Property(c => c.UpdatedAt)
                     .HasColumnName("updated_at")
                     .HasColumnType("datetime");

              builder.HasOne(c => c.Map)
                     .WithMany()
                     .HasForeignKey(c => c.MapId)
                     .OnDelete(DeleteBehavior.Restrict);

              builder.HasOne(c => c.Layer)
                     .WithMany()
                     .HasForeignKey(c => c.LayerId)
                     .OnDelete(DeleteBehavior.Restrict);

              builder.HasOne(c => c.User)
                     .WithMany()
                     .HasForeignKey(c => c.UserId)
                     .OnDelete(DeleteBehavior.SetNull);
       }
}
