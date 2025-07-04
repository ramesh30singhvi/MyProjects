using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class CriteriaOptionExtensions
    {
        public static void BuildCriteriaOptionEntity(this ModelBuilder builder)
        {
            builder.Entity<CriteriaOption>(entity =>
            {
                entity.HasKey(criteriaOption => criteriaOption.TableColumnId);

                entity.Property(criteriaOption => criteriaOption.ShowNA)
                    .IsRequired();

                entity.Property(criteriaOption => criteriaOption.Compliance)
                    .IsRequired();

                entity.Property(criteriaOption => criteriaOption.Quality)
                    .IsRequired();

                entity.Property(criteriaOption => criteriaOption.Priority)
                    .IsRequired();

                entity.HasOne(criteriaOption => criteriaOption.TableColumn)
                    .WithOne(tableColumn => tableColumn.CriteriaOption)
                    .HasForeignKey<CriteriaOption>(criteriaOption => criteriaOption.TableColumnId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
