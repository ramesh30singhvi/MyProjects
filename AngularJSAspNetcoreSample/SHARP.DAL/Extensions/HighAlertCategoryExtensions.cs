using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Extensions
{
    public static class HighAlertCategoryExtensions
    {
        public static void BuildHighAlertCategoryEntity(this ModelBuilder builder)
        {
            builder.Entity<HighAlertCategory>(entity =>
            {
                entity.HasKey(highAlertCategory => highAlertCategory.Id);

                entity.Property(highAlertCategory => highAlertCategory.Id)
                    .HasColumnName("ID");

                entity.Property(highAlertCategory => highAlertCategory.Name)
                    .IsRequired();
            });
        }
    }
}
