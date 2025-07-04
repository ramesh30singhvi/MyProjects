using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class ReportExtensions
    {
        public static void BuildReportEntity(this ModelBuilder builder)
        {
            builder.Entity<Report>(entity =>
            {
                entity.ToTable("Report");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Name).HasColumnName("Name").IsUnicode(false);
                entity.Property(e => e.TableauUrl).HasColumnName("TableauUrl").IsUnicode(false);
            });
        }
    }
}
