using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Extensions
{
    public static class AuditAIPatientPdfNotesExtensions
    {

        public static void BuildAuditAIPatientPdfNotesEntity(this ModelBuilder builder)
        {
            builder.Entity<AuditAIPatientPdfNotes>(entity =>
            {
                entity.HasKey(reportAIContent => reportAIContent.Id);

                entity.Property(reportAIContent => reportAIContent.AuditAIReportV2Id)
                    .IsRequired();


            });
        }
    }
}
