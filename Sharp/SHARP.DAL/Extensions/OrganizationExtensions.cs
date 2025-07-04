using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;

namespace SHARP.DAL.Extensions
{
    public static class OrganizationExtensions
    {
        public static void BuildOrganizationEntity(this ModelBuilder builder)
        {
            builder.Entity<Organization>(entity =>
            {
                entity.HasKey(organization => organization.Id);

                entity.Property(organization => organization.Id)
                    .HasColumnName("ID");

                entity.Property(organization => organization.Name)
                    .IsRequired();

                entity.Property(organization => organization.OrgUuid)
                    .HasConversion(
                        to => to.ToString(),
                        from => Guid.Parse(from));

                entity
                    .HasMany(organization => organization.Recipients)
                    .WithOne()
                    .HasForeignKey(recipient => recipient.OrganizationId);

                entity
                    .HasMany(organization => organization.UserOrganizations)
                    .WithOne(userOrganization => userOrganization.Organization)
                    .HasForeignKey(userOrganization => userOrganization.OrganizationId);

                entity.HasMany(organization => organization.DashboardInputTables)
                    .WithOne(dit => dit.Organization)
                    .HasForeignKey(dit => dit.OrganizationId)
                    .HasPrincipalKey(org => org.Id);
            });
        }
    }
}
