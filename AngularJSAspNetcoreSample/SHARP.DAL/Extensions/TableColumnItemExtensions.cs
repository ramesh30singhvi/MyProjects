using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class TableColumnItemExtensions
    {
        public static void BuildTableColumnItemEntity(this ModelBuilder builder)
        {
            builder.Entity<TableColumnItem>(entity =>
            {
                entity.HasKey(tableColumnItem => tableColumnItem.Id);

                entity.Property(tableColumnItem => tableColumnItem.TableColumnId)
                    .IsRequired();

                entity.Property(tableColumnItem => tableColumnItem.Value)
                    .IsRequired();

                entity.Property(tableColumnItem => tableColumnItem.Sequence)
                    .IsRequired();

                entity.HasOne(tableColumnItem => tableColumnItem.TrackerOption)
                    .WithMany(tableColumn => tableColumn.Items)
                    .HasForeignKey(tableColumnItem => tableColumnItem.TableColumnId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
