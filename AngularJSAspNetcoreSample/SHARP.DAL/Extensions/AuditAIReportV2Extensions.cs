using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;

namespace SHARP.DAL.Extensions
{
    public static class AuditAIReportV2Extensions
    {
        public static void BuildAuditAIReportV2Entity(this ModelBuilder builder)
        {
            builder.Entity<AuditAIReportV2>(entity =>
            {
                entity.HasKey(reportAIContent => reportAIContent.Id);

                entity.Property(reportAIContent => reportAIContent.OrganizationId)
                    .IsRequired();

                entity.Property(reportAIContent => reportAIContent.CreatedAt)
                         .HasConversion(
                             to => to,
                             from => DateTime.SpecifyKind(from, DateTimeKind.Utc))
                         .IsRequired();


            });
        }
    }
}
