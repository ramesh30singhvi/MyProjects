using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Extensions
{
    public static class AuditAIKeywordSummaryExtensions
    {

        public static void BuildAuditAIKeywordSummaryEntity(this ModelBuilder builder)
        {
            builder.Entity<AuditAIKeywordSummary>(entity =>
            {
                entity.HasKey(reportAIContent => reportAIContent.Id);

                entity.Property(reportAIContent => reportAIContent.AuditAIPatientPdfNotesID)
                    .IsRequired();


            });
        }
    }
}
