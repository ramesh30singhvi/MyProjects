using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Extensions
{
    public static class ReportRangeExtensions
    {
        public static void BuildReportRangeEntity(this ModelBuilder builder)
        {
            builder.Entity<ReportRange>(entity =>
            {
                entity.HasKey(reportRange => reportRange.Id);

                entity.Property(reportRange => reportRange.Id)
                    .HasColumnName("ID");

                entity.Property(reportRange => reportRange.RangeName)
                    .IsRequired();
            });
        }
    }
}
