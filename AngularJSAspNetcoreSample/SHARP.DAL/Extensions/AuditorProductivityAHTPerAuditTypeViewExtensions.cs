using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models.Views;
using System;

namespace SHARP.DAL.Extensions
{
    public static class AuditorProductivityAHTPerAuditTypeViewExtensions
    {
        public static void BuildAuditorProductivityAHTPerAuditTypeViewEntity(this ModelBuilder builder)
        {
            builder.Entity<AuditorProductivityAHTPerAuditTypeView>(entity =>
            {
                entity.HasNoKey();

                entity.Property(auditorProductivityAHTPerAuditType => auditorProductivityAHTPerAuditType.StartTime)
                         .HasConversion(
                             to => to,
                             from => from.HasValue ? (DateTime?)DateTime.SpecifyKind((DateTime)from, DateTimeKind.Utc) : null
                         );

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId);

                entity.HasOne(e => e.Facility)
                   .WithMany()
                   .HasForeignKey(e => e.FacilityId);

                entity.HasOne(e => e.AuditType)
                    .WithMany()
                    .HasForeignKey(e => e.AuditTypeId);
            });
        }
    }
}
