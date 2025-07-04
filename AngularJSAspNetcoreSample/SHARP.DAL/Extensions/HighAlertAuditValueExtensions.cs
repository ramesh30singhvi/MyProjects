using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Extensions
{
    public static class HighAlertAuditValueExtensions
    {
        public static void BuildHighAlertAuditValueEntity(this ModelBuilder builder)
        {
            builder.Entity<HighAlertAuditValue>(entity =>
            {
                entity.HasKey(highAlertAuditValue => highAlertAuditValue.Id);

                entity.Property(highAlertAuditValue => highAlertAuditValue.Id)
                    .HasColumnName("ID");
                entity.Property(highAlertAuditValue => highAlertAuditValue.HighAlertNotes)
                    .HasColumnName("Notes");
                entity.Property(highAlertAuditValue => highAlertAuditValue.HighAlertDescription)
                      .HasColumnName("Description");

                entity.Property(highAlertAuditValue => highAlertAuditValue.AuditTableColumnValueId)
                      .HasColumnName("AuditTableColumnValueID");


                entity.Property(highAlertAuditValue => highAlertAuditValue.HighAlertCategoryId)
                         .HasColumnName("HighAlertCategoryID");
                entity.Property(highAlertAuditValue => highAlertAuditValue.AuditId)
                       .HasColumnName("AuditID");
                entity.Property(highAlertAuditValue => highAlertAuditValue.ReportAiId)
                       .HasColumnName("ReportAiID");
                entity.Property(highAlertAuditValue => highAlertAuditValue.HighAlertDescription)
                    .IsRequired();


                entity.Property(highAlertAuditValue => highAlertAuditValue.HighAlertCategoryId)
                     .IsRequired();


            });
        }
    }
}
