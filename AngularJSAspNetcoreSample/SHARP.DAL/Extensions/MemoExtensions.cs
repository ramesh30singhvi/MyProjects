using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;

namespace SHARP.DAL.Extensions
{
    public static class MemoExtensions
    {
        public static void BuildMemoEntity(this ModelBuilder builder)
        {
            builder.Entity<Memo>(entity =>
            {
                entity.HasKey(memo => memo.Id);

                entity.Property(memo => memo.UserId)
                    .IsRequired();

                entity.Property(memo => memo.Text)
                   .IsRequired();

                entity.Property(memo => memo.CreatedDate)
                    .HasConversion(
                        to => to,
                        from => DateTime.SpecifyKind(from, DateTimeKind.Utc))
                    .IsRequired();

                entity.HasOne(memo => memo.User)
                    .WithMany()
                    .HasForeignKey(audit => audit.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
