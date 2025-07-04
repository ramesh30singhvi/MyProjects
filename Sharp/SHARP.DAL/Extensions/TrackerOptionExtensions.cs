using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class TrackerOptionExtensions
    {
        public static void BuildTrackerOptionEntity(this ModelBuilder builder)
        {
            builder.Entity<TrackerOption>(entity =>
            {
                entity.HasKey(criteriaOption => criteriaOption.TableColumnId);

                entity.Property(criteriaOption => criteriaOption.FieldTypeId)
                    .IsRequired();

                entity.Property(criteriaOption => criteriaOption.IsRequired)
                    .IsRequired();

                entity.Property(criteriaOption => criteriaOption.Compliance)
                    .IsRequired();

                entity.Property(criteriaOption => criteriaOption.Quality)
                    .IsRequired();

                entity.Property(criteriaOption => criteriaOption.Priority)
                    .IsRequired();

                entity.HasOne(trackerOption => trackerOption.TableColumn)
                    .WithOne(tableColumn => tableColumn.TrackerOption)
                    .HasForeignKey<TrackerOption>(trackerOption => trackerOption.TableColumnId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(trackerOption => trackerOption.FieldType)
                    .WithMany()
                    .HasForeignKey(trackerOption => trackerOption.FieldTypeId);
            });
        }
    }
}
