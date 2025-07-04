using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Extensions
{
    public static class ReportCategoryExtensions
    {
        public static void BuildReportCategoryEntity(this ModelBuilder builder)
        {
            builder.Entity<ReportCategory>(entity =>
            {
                entity.HasKey(reportCategory => reportCategory.Id);

                entity.Property(reportCategory => reportCategory.Id)
                    .HasColumnName("ID");

                entity.Property(reportCategory => reportCategory.ReportCategoryName)
                    .IsRequired();
            });
        }
    }
}
