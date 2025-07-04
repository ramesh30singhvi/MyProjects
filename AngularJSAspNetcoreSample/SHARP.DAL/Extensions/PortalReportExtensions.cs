using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Extensions
{
    public static class PortalReportExtensions
    {

        public static void BuildPortalReportEntity(this ModelBuilder builder)
        {
            builder.Entity<PortalReport>(entity =>
            {
                entity.HasKey(portalReport => portalReport.Id);

                entity.Property(portalReport => portalReport.Id)
                    .HasColumnName("ID");

                entity.Property(portalReport => portalReport.Name)
                    .IsRequired();

                entity.Property(portalReport => portalReport.ReportTypeId)
                    .HasColumnName("ReportType")
                    .IsRequired();
                entity.Property(portalReport => portalReport.ReportCategoryId)
                     .HasColumnName("ReportCategory")
                    .IsRequired();
                entity.Property(portalReport => portalReport.ReportRequestId)
                    .HasColumnName("ReportRequestID");

                //[ReportRequestID]

                entity.Property(portalReport => portalReport.CreatedAt)
                      .HasConversion(
                        to => to,
                        from => DateTime.SpecifyKind(from, DateTimeKind.Utc))
                    .IsRequired();
                entity.Property(portalReport => portalReport.CreatedByUserID)
                      .IsRequired();
                entity.Property(portalReport => portalReport.OrganizationId)
                    .IsRequired();
                entity.Property(portalReport => portalReport.FacilityId)
                     .IsRequired();

                entity.HasOne(portalReport => portalReport.Facility)
                    .WithMany(facility => facility.PortalReport)
                    .HasForeignKey(portalReport => portalReport.FacilityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(portalReport => portalReport.User)
                       .WithMany()
                       .HasForeignKey(portalReport => portalReport.CreatedByUserID);

                entity.HasOne(portalReport => portalReport.Audit)
                    .WithOne(audit => audit.PortalReport);
            });
        }
    }
}
