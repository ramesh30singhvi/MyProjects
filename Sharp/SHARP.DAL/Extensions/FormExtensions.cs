using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;

namespace SHARP.DAL.Extensions
{
    public static class FormExtensions
    {
        public static void BuildFormEntity(this ModelBuilder builder)
        {
            builder.Entity<Form>(entity =>
            {
                entity.HasKey(form => form.Id);

                entity.Property(form => form.Id)
                    .HasColumnName("ID");

                entity.Property(form => form.Name)
                    .IsRequired();

                entity.Property(form => form.AuditTypeId)
                    .HasColumnName("AuditTypeID")
                    .IsRequired();

                        
                entity.Property(form => form.IsActive)
                    .IsRequired();

                entity.Property(form => form.LegacyFormId)
                    .HasColumnName("LegacyFormID");

                entity.HasOne(form => form.AuditType)
                    .WithMany()
                    .HasForeignKey(form => form.AuditTypeId)
                    .OnDelete(DeleteBehavior.Restrict);


                entity.HasOne(form => form.Organization)
                    .WithMany(org => org.Forms)
                    .HasForeignKey(form => form.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
