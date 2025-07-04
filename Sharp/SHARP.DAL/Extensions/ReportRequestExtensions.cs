using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;

namespace SHARP.DAL.Extensions
{
    public static class ReportRequestExtensions
    {
        public static void BuildReportRequestEntity(this ModelBuilder builder)
        {
            builder.Entity<ReportRequest>(entity =>
            {
                entity.HasKey(i => i.Id);

                entity.Property(i => i.UserId)
                    .IsRequired();

                entity.Property(i => i.AuditType)
                    .IsRequired();

                entity.Property(i => i.OrganizationId)
                    .IsRequired();

                entity.Property(i => i.FormId)
                    .IsRequired();

                entity.Property(i => i.FromDate)
                    .IsRequired();

                entity.Property(i => i.RequestedTime)
                    .HasConversion(
                        to => to,
                        from => DateTime.SpecifyKind(from, DateTimeKind.Utc))
                    .IsRequired();

                entity.Property(i => i.GeneratedTime)
                    .HasConversion(
                        to => to,
                        from => from.HasValue ? DateTime.SpecifyKind(from.Value, DateTimeKind.Utc) : from);

                entity.Property(i => i.Status)
                    .IsRequired();

                entity.HasOne(i => i.User)
                    .WithMany()
                    .HasForeignKey(j => j.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.Organization)
                    .WithMany()
                    .HasForeignKey(j => j.OrganizationId);

                entity.HasOne(i => i.Facility)
                    .WithMany()
                    .HasForeignKey(j => j.FacilityId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.Form)
                    .WithMany()
                    .HasForeignKey(j => j.FormId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
