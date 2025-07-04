using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;

namespace SHARP.DAL.Extensions
{
    public static class ReportAIContentExtensions
    {
        public static void BuildReportAIContentEntity(this ModelBuilder builder)
        {
            builder.Entity<ReportAIContent>(entity =>
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
