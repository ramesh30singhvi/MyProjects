using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;

namespace SHARP.DAL.Extensions
{
    public static class UserActivityExtensions
    {
        public static void BuildUserActivityEntity(this ModelBuilder builder)
        {
            builder.Entity<UserActivity>(entity =>
            {
                entity.HasKey(i => i.Id);

                entity.Property(i => i.UserId)
                    .IsRequired(false);

                entity.Property(i => i.ActionType)
                    .IsRequired();

                entity.Property(i => i.ActionTime)
                .HasConversion(
                        to => to,
                        from => DateTime.SpecifyKind(from, DateTimeKind.Utc))
                    .IsRequired();

                entity.Property(i => i.UserAgent)
                    .IsRequired();

                entity.Property(i => i.IP)
                    .IsRequired();

                entity.HasOne(i => i.User)
                    .WithMany(user => user.Activities)
                    .HasForeignKey(i => i.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(i => i.IP)
                    .IsRequired(false);

                entity.Property(i => i.LoginUsername)
                    .IsRequired(false);
            });
        }
    }
}
