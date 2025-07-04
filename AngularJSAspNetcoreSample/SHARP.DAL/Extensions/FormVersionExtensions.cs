using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;

namespace SHARP.DAL.Extensions
{
    public static class FormVersionExtensions
    {
        public static void BuildFormVersionEntity(this ModelBuilder builder)
        {
            builder.Entity<FormVersion>(entity =>
            {
                entity.HasKey(formVersion => formVersion.Id);

                entity.Property(formVersion => formVersion.FormId)
                    .IsRequired();

                entity.Property(formVersion => formVersion.Version)
                    .IsRequired();

                entity.Property(formVersion => formVersion.Status)
                    .IsRequired();

                entity.Property(audit => audit.CreatedDate)
                   .HasConversion(
                       to => to,
                       from => from.HasValue ? DateTime.SpecifyKind(from.Value, DateTimeKind.Utc) : from);

                entity.HasOne(formVersion => formVersion.Form)
                    .WithMany(form => form.Versions)
                    .HasForeignKey(formVersion => formVersion.FormId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(formVersion => formVersion.CreatedBy)
                    .WithMany()
                    .HasForeignKey(formVersion => formVersion.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
