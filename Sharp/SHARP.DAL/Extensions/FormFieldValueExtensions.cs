using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class FormFieldValueExtensions
    {
        public static void BuildFormFieldValueEntity(this ModelBuilder builder)
        {
            builder.Entity<AuditFieldValue>(entity =>
            {
                entity.HasKey(formFieldValue => formFieldValue.Id);

                entity.Property(formFieldValue => formFieldValue.Id)
                    .HasColumnName("ID");

                entity.Property(formFieldValue => formFieldValue.FormFieldId)
                    .HasColumnName("FormFieldID")
                    .IsRequired();

                entity.Property(formFieldValue => formFieldValue.AuditId)
                    .HasColumnName("AuditID")
                    .IsRequired();

                entity.Property(formFieldValue => formFieldValue.Value)
                    .IsRequired();

                entity.HasOne(formFieldValue => formFieldValue.Audit)
                    .WithMany(audit => audit.AuditFieldValues)
                    .HasForeignKey(formFieldValue => formFieldValue.AuditId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(formFieldValue => formFieldValue.FormField)
                    .WithMany(formField => formField.Values)
                    .HasForeignKey(formFieldValue => formFieldValue.FormFieldId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
