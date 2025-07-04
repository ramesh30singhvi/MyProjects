using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;

namespace SHARP.DAL.Extensions
{
    public static class FormOrganizations
    {
        public static void BuildFormOrganizationEntity(this ModelBuilder builder)
        {
            builder.Entity<FormOrganization>(entity =>
            {
                entity.HasKey(formOrganization => formOrganization.Id);

                entity.Property(formOrganization => formOrganization.FormId)
                    .IsRequired();

                entity.Property(formOrganization => formOrganization.OrganizationId)
                    .IsRequired();

                entity.HasOne(formOrganization => formOrganization.Form)
                    .WithMany(form => form.FormOrganizations)
                    .HasForeignKey(formOrganization => formOrganization.FormId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(formOrganization => formOrganization.Organization)
                    .WithMany(organization => organization.FormOrganizations)
                    .HasForeignKey(formOrganization => formOrganization.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
