using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;

namespace SHARP.DAL.Extensions
{
    public static class AuditStatusHistoryExtensions
    {
        public static void BuildAuditStatusHistoryEntity(this ModelBuilder builder)
        {
            builder.Entity<AuditStatusHistory>(entity =>
            {
                entity.HasKey(auditStatusHistory => auditStatusHistory.Id);

                entity.Property(auditStatusHistory => auditStatusHistory.AuditId)
                    .IsRequired();

                entity.Property(auditStatusHistory => auditStatusHistory.UserId)
                    .IsRequired();

                entity.Property(auditStatusHistory => auditStatusHistory.Status)
                    .IsRequired();

                entity.Property(auditStatusHistory => auditStatusHistory.Date)
                    .HasConversion(
                        to => to,
                        from => DateTime.SpecifyKind(from, DateTimeKind.Utc))
                    .IsRequired();

                entity.HasOne(auditStatusHistory => auditStatusHistory.Audit)
                    .WithMany(audits => audits.AuditStatusHistory)
                    .HasForeignKey(audit => audit.AuditId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(auditStatusHistory => auditStatusHistory.User)
                    .WithMany(user => user.AuditStatusHistory)
                    .HasForeignKey(auditStatusHistory => auditStatusHistory.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
