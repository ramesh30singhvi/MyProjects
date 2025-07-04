using System;
using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class DashboardInputGroupsExtensions
    {
        public static void BuildDashboardInputGroups(this ModelBuilder builder)
        {
            builder.Entity<DashboardInputGroups>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id);

                entity.Property(dashboardInputTable => dashboardInputTable.Name).IsRequired();

                entity.Property(e => e.TableId).IsRequired();

                entity.HasMany(e => e.DashboardInputElements).WithOne(die => die.DashboardInputGroups).HasForeignKey(e => e.GroupId).HasPrincipalKey(e => e.Id);

            });
        }
    }
}

