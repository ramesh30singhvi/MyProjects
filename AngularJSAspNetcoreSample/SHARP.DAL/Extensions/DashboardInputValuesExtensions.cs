using System;
using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class DashboardInputValuesExtensions
    {
        public static void BuildDashboardInputValues(this ModelBuilder builder)
        {
            builder.Entity<DashboardInputValues>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id);

                entity.Property(e => e.ElementId);

                entity.Property(e => e.ElementId);

                entity.Property(e => e.Date);

                entity.Property(e => e.Value).IsRequired();

                entity.HasOne(e => e.Facility).WithMany().HasForeignKey(e => e.FacilityId);

            });
        }
    }
}

