using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Extensions
{
    public static class HighAlertCategoryToPotentialAreasExtensions
    {
        public static void BuildHighAlertCategoryToPotentialAreasEntity(this ModelBuilder builder)
        {
            builder.Entity<HighAlertCategoryToPotentialAreas>(entity =>
            {
                entity.HasKey(highAlertCategory => highAlertCategory.Id);

                entity.Property(highAlertCategory => highAlertCategory.Id)
                    .HasColumnName("ID");

            });
        }
    }
}
