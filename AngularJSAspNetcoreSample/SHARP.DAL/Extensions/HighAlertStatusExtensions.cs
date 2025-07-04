using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Extensions
{
    public static class HighAlertStatusExtensions
    {
        public static void BuildHighAlertStatusEntity(this ModelBuilder builder)
        {
            builder.Entity<HighAlertStatus>(entity =>
            {
                entity.HasKey(highAlertStatus => highAlertStatus.Id);

                entity.Property(highAlertStatus => highAlertStatus.Id)
                    .HasColumnName("ID");

                entity.Property(highAlertStatus => highAlertStatus.Name)
                    .IsRequired();
            });
        }
    }
}
