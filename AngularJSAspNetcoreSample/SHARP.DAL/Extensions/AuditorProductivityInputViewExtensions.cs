using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models.Views;
using System;

namespace SHARP.DAL.Extensions
{
    public static class AuditorProductivityInputViewExtensions
    {
        public static void BuildAuditorProductivityInputViewEntity(this ModelBuilder builder)
        {
            builder.Entity<AuditorProductivityInputView>(entity =>
            {
                entity.HasNoKey();

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId);

                entity.HasOne(e => e.Facility)
                    .WithMany()
                    .HasForeignKey(e => e.FacilityId);

                entity.HasOne(e => e.AuditType)
                    .WithMany()
                    .HasForeignKey(e => e.AuditTypeId);

                entity.Property(auditorProductivityInput => auditorProductivityInput.StartTime)
                         .HasConversion(
                             to => to,
                             from => from.HasValue ? (DateTime?)DateTime.SpecifyKind((DateTime)from, DateTimeKind.Utc) : null
                         );

                entity.Property(auditorProductivityInput => auditorProductivityInput.CompletionTime)
                         .HasConversion(
                             to => to,
                             from => from.HasValue ? (DateTime?)DateTime.SpecifyKind((DateTime)from, DateTimeKind.Utc) : null
                         );

            });
        }
    }
}
