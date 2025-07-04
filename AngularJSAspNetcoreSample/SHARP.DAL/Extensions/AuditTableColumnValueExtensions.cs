using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;

namespace SHARP.DAL.Extensions
{
    public static class AuditTableColumnValueExtensions
    {
        public static void BuildAuditTableColumnValueEntity(this ModelBuilder builder)
        {
            builder.Entity<AuditTableColumnValue>(entity =>
            {
                entity.HasKey(tableColumnValue => tableColumnValue.Id);

                entity.Property(tableColumnValue => tableColumnValue.AuditId)
                    .IsRequired();

                entity.Property(tableColumnValue => tableColumnValue.TableColumnId)
                    .IsRequired();

                entity.HasOne(tableColumnValue => tableColumnValue.Audit)
                    .WithMany(audit => audit.Values)
                    .HasForeignKey(tableColumnValue => tableColumnValue.AuditId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tableColumnValue => tableColumnValue.Column)
                    .WithMany(tableColumn => tableColumn.TableColumnValues)
                    .HasForeignKey(tableColumnValue => tableColumnValue.TableColumnId)
                    .OnDelete(DeleteBehavior.Restrict);


            });
        }
    }
}
