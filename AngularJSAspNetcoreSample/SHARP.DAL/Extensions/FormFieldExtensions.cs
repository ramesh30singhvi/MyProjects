using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class FormFieldExtensions
    {
        public static void BuildFormFieldEntity(this ModelBuilder builder)
        {
            builder.Entity<FormField>(entity =>
            {
                entity.HasKey(formField => formField.Id);

                entity.Property(formField => formField.Id)
                    .HasColumnName("ID");

                entity.Property(formField => formField.FieldTypeId)
                    .HasColumnName("FieldTypeID")
                    .IsRequired();

                entity.Property(formField => formField.Sequence)
                    .IsRequired();

                entity.Property(formField => formField.FieldName)
                    .IsRequired();

                entity.Property(formField => formField.LabelName)
                    .IsRequired();

                entity.Property(formField => formField.FormVersionId)
                    .IsRequired();

                entity.Property(formField => formField.IsRequired)
                    .IsRequired();

                entity.HasOne(formField => formField.FieldType)
                    .WithMany()
                    .HasForeignKey(formField => formField.FieldTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(formField => formField.FormVersion)
                    .WithMany(org => org.FormFields)
                    .HasForeignKey(formField => formField.FormVersionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
