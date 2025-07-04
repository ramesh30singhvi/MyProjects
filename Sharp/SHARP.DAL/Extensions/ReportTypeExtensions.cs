using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Extensions
{
    public static class ReportTypeExtensions
    {
        public static void BuildReportTypeEntity(this ModelBuilder builder)
        {
            builder.Entity<ReportType>(entity =>
            {
                entity.HasKey(reportType => reportType.Id);

                entity.Property(reportType => reportType.Id)
                    .HasColumnName("ID");

                entity.Property(reportType => reportType.TypeName)
                    .IsRequired();
            });
        }
    }
}
