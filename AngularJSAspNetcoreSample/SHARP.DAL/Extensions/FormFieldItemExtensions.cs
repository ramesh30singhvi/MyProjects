using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class FormFieldItemExtensions
    {
        public static void BuildFormFieldItemEntity(this ModelBuilder builder)
        {
            builder.Entity<FormFieldItem>(entity =>
            {
                entity.HasKey(formFieldItem => formFieldItem.Id);

                entity.Property(formFieldItem => formFieldItem.FormFieldId)
                    .IsRequired();

                entity.Property(formFieldItem => formFieldItem.Value)
                    .IsRequired();

                entity.Property(formFieldItem => formFieldItem.Sequence)
                    .IsRequired();

                entity.HasOne(formFieldItem => formFieldItem.FormField)
                    .WithMany(formField => formField.Items)
                    .HasForeignKey(formFieldItem => formFieldItem.FormFieldId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
