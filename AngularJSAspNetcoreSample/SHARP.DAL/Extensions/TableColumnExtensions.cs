using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class TableColumnExtensions
    {
        public static void BuildTableColumnEntity(this ModelBuilder builder)
        {
            builder.Entity<TableColumn>(tableColumn =>
            {
                tableColumn.HasKey(tc => tc.Id);

                tableColumn.Property(tc => tc.Id).HasColumnName("ID");

                tableColumn.Property(tc => tc.FormVersionId).IsRequired();

                tableColumn.Property(tc => tc.GroupId).HasColumnName("GroupID");

                tableColumn.Property(tc => tc.LegacyFormRowId).HasColumnName("LegacyFormRowID");

                tableColumn.Property(tc => tc.LegacyRowId).HasColumnName("LegacyRowID");

                tableColumn.Property(tc => tc.LegacyFormColumnId).HasColumnName("LegacyFormColumnID");

                tableColumn
                    .HasOne(column => column.FormVersion)
                    .WithMany(form => form.Columns)
                    .OnDelete(DeleteBehavior.Cascade);

                tableColumn
                    .HasOne(column => column.Group)
                    .WithMany(group => group.TableColumns)
                    .HasForeignKey(column => column.GroupId);

                tableColumn.HasMany(column => column.KeywordTrigger).
                            WithOne(keyword => keyword.Keyword).
                            HasForeignKey(keyword => keyword.KeywordId).
                            OnDelete(DeleteBehavior.Cascade); 

                tableColumn
                    .HasOne(column => column.Parent)
                    .WithMany(column => column.SubQuestions)
                    .HasForeignKey(column => column.ParentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
