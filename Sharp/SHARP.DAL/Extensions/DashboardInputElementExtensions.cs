using System;
using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class DashboardInputElementExtensions
    {
        public static void BuildDashboardInputElement(this ModelBuilder builder)
        {
            builder.Entity<DashboardInputElement>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id);

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.GroupId).IsRequired();

                entity.HasMany(e => e.DashboardInputValues).WithOne(e => e.DashboardInputElement).HasForeignKey(e => e.ElementId).HasPrincipalKey(e => e.Id);
            });
        }
    }
}

