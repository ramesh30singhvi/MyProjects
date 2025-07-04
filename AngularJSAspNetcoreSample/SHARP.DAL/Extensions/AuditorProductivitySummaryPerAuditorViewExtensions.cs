using System;
using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models.Views;

namespace SHARP.DAL.Extensions
{
    public static class AuditorProductivitySummaryPerAuditorViewExtensions
    {
        public static void BuildAuditorProductivitySummaryPerAuditorViewEntity(this ModelBuilder builder)
        {
            builder.Entity<AuditorProductivitySummaryPerAuditorView>(entity =>
            {
                entity.HasNoKey();

                entity.Property(auditorProductivitySummaryPerAuditor => auditorProductivitySummaryPerAuditor.StartTime)
                         .HasConversion(
                             to => to,
                             from => from.HasValue ? (DateTime?)DateTime.SpecifyKind((DateTime)from, DateTimeKind.Utc) : null
                         );

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId);

                entity.HasOne(e => e.AuditType)
                    .WithMany()
                    .HasForeignKey(e => e.AuditTypeId);

                entity.HasOne(e => e.Facility)
                    .WithMany()
                    .HasForeignKey(e => e.FacilityId);
            });
        }
    }
}
