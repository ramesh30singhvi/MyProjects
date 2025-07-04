using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Extensions
{
    public static class AuditTriggeredByKeywordExtensions
    {
        public static void BuildAuditTriggeredByKeywordEntity(this ModelBuilder builder)
        {
            builder.Entity<AuditTriggeredByKeyword>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Id)
                  .HasColumnName("ID");

                entity.Property(tc => tc.AuditId).HasColumnName("AuditID");

                entity.Property(i => i.AuditId)
                    .IsRequired();

                entity.Property(tc => tc.AuditTableColumnValueId).HasColumnName("AuditTableColumnValueID");

                entity.Property(i => i.AuditTableColumnValueId)
                    .IsRequired();

                entity.Property(tc => tc.CreatedAuditId).HasColumnName("CreatedAuditID");

                entity.Property(i => i.CreatedAuditId)
                    .IsRequired();

                entity.HasOne(audit => audit.CreatedAuditByKeyword)
                      .WithMany(template => template.AuditsTriggeredByKeyword)
                      .HasForeignKey(audit => audit.CreatedAuditId)
                      .OnDelete(DeleteBehavior.Cascade);

            });
        }
    }

}