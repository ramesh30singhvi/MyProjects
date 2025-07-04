using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;

namespace SHARP.DAL.Extensions
{
    public static class DownloadsTrackingExtensions
    {
        public static void BuildDownloadsTrackingEntity(this ModelBuilder builder)
        {
            builder.Entity<DownloadsTracking>(entity =>
            {
                entity.HasKey(i => i.Id);

                entity.Property(i => i.UserId)
                    .IsRequired();

                entity.Property(i => i.PortalReportId)
                    .IsRequired();

                entity.Ignore(i => i.NumberOfDownloads);

                entity.Property(i => i.DateAndTime)
                    .HasConversion(
                        to => to,
                        from => from.HasValue ? (DateTime?)DateTime.SpecifyKind((DateTime)from, DateTimeKind.Utc) : null)
                    .IsRequired(false);

                entity.Ignore(i => i.PortalReportCreatedAt);

                entity.HasOne(i => i.User)
                    .WithMany(user => user.DownloadsTracking)
                    .HasForeignKey(i => i.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.PortalReport)
                    .WithMany(portalReport => portalReport.DownloadsTracking)
                    .HasForeignKey(i => i.PortalReportId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Ignore(i => i.DownloadsTrackingDetails);
            });
        }
    }
}
