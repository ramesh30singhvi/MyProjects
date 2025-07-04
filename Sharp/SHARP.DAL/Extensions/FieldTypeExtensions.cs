using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class FieldTypeExtensions
    {
        public static void BuildFieldTypeEntity(this ModelBuilder builder)
        {
            builder.Entity<FieldType>(entity =>
            {
                entity.HasKey(fieldType => fieldType.Id);

                entity.Property(fieldType => fieldType.Id)
                    .HasColumnName("ID");

                entity.Property(fieldType => fieldType.Name)
                    .IsRequired();
            });
        }
    }
}
