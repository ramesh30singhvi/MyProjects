using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Extensions
{
    public static class KeywordTriggerExtensions
    {
        public static void BuildKeywordTriggerEntity(this ModelBuilder builder)
        {
            builder.Entity<KeywordTrigger>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Id)
                  .HasColumnName("ID");

                entity.Property(tc => tc.KeywordFormId).HasColumnName("FormKeywordID");

                entity.Property(i => i.KeywordFormId)
                    .IsRequired();

                entity.Property(tc => tc.KeywordId).HasColumnName("KeywordID");

                entity.Property(i => i.KeywordId)
                    .IsRequired();

                entity.Property(tc => tc.FormTriggerId).HasColumnName("FormTriggerID");

                entity.Property(i => i.FormTriggerId)
                    .IsRequired();

                entity.HasOne(tc => tc.FormTriggeredByKeyword).WithMany().HasForeignKey(x => x.FormTriggerId);

            });
        }
    }
}
