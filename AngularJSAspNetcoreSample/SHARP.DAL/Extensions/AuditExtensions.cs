using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;

namespace SHARP.DAL.Extensions
{
    public static class AuditExtensions
    {
        public static void BuildAuditEntity(this ModelBuilder builder)
        {
            builder.Entity<Audit>(entity =>
            {
                entity.HasKey(audit => audit.Id);

                entity.Property(audit => audit.SubmittedByUserId)
                    .IsRequired();

                entity.Property(audit => audit.SubmittedDate)
                    .HasConversion(
                        to => to,
                        from => DateTime.SpecifyKind(from, DateTimeKind.Utc))
                    .IsRequired();

                entity.Property(audit => audit.Status)
                    .IsRequired();

                entity.Property(audit => audit.State)
                    .IsRequired();

                entity.Property(audit => audit.LastUnarchivedDate);

                entity.Property(audit => audit.FacilityId)
                    .IsRequired();

                entity.Property(audit => audit.FormVersionId)
                    .IsRequired();

                entity.HasOne(audit => audit.SubmittedByUser)
                    .WithMany(user => user.Audits)
                    .HasForeignKey(audit => audit.SubmittedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(audit => audit.Facility)
                    .WithMany(facility => facility.Audits)
                    .HasForeignKey(audit => audit.FacilityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(audit => audit.FormVersion)
                    .WithMany(template => template.Audits)
                    .HasForeignKey(audit => audit.FormVersionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(audit => audit.DeletedByUser)
                    .WithMany(user => user.DeletedAudits)
                    .HasForeignKey(audit => audit.DeletedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(audit => audit.SentForApprovalDate)
                    .HasConversion(
                        to => to,
                        from => from.HasValue ? (DateTime?)DateTime.SpecifyKind((DateTime)from, DateTimeKind.Utc) : null);
            });
        }
    }
}
