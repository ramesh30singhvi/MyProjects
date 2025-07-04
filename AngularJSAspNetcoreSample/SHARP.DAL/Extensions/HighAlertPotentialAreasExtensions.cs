using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Extensions
{
    public static class HighAlertPotentialAreasExtensions
    {
        public static void BuildHighAlertPotentialAreasEntity(this ModelBuilder builder)
        {
            builder.Entity<HighAlertPotentialAreas>(entity =>
            {
                entity.HasKey(highAlertCategory => highAlertCategory.Id);

                entity.Property(highAlertCategory => highAlertCategory.Id)
                    .HasColumnName("ID");

            });
        }
    }
}
