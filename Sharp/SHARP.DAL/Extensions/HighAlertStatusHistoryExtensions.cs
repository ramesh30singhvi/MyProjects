using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Extensions
{
    public static class HighAlertStatusHistoryExtensions
    {
        public static void BuildHighAlertStatusHistoryEntity(this ModelBuilder builder)
        {
            builder.Entity<HighAlertStatusHistory>(entity =>
            {
                entity.HasKey(highAlertStatusHistory => highAlertStatusHistory.Id);

                entity.Property(highAlertStatusHistory => highAlertStatusHistory.Id)
                    .HasColumnName("ID");
                entity.Property(highAlertStatusHistory => highAlertStatusHistory.HighAlertAuditValueId)
                    .HasColumnName("HighAlertAuditValueID");
                entity.Property(highAlertStatusHistory => highAlertStatusHistory.HighAlertStatusId)
                      .HasColumnName("HighAlertStatusID");
                entity.Property(highAlertStatusHistory => highAlertStatusHistory.ChangedBy)
                      .HasColumnName("ChangedBy");

                entity.Property(highAlertStatusHistory => highAlertStatusHistory.HighAlertStatusId)
                    .IsRequired();

                entity.Property(highAlertStatusHistory => highAlertStatusHistory.HighAlertAuditValueId)
                     .IsRequired();

            });
        }
    }
}
