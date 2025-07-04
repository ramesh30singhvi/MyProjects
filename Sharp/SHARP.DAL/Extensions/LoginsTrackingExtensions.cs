using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;

namespace SHARP.DAL.Extensions
{
    public static class LoginsTrackingExtensions
    {
        public static void BuildLoginsTrackingEntity(this ModelBuilder builder)
        {
            builder.Entity<LoginsTracking>(entity =>
            {
                entity.HasKey(i => i.Id);

                entity.Property(i => i.UserId)
                    .IsRequired();

                entity.Property(i => i.Login)
                .HasConversion(
                        to => to,
                        from => DateTime.SpecifyKind(from, DateTimeKind.Utc))
                    .IsRequired();

                entity.Property(i => i.Duration)
                    .IsRequired(false);

                entity.Property(i => i.Logout)
                    .HasConversion(
                        to => to,
                        from => from.HasValue ? (DateTime?)DateTime.SpecifyKind((DateTime)from, DateTimeKind.Utc) : null)
                    .IsRequired(false);

                entity.HasOne(i => i.User)
                    .WithMany(user => user.LoginsTracking)
                    .HasForeignKey(i => i.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
