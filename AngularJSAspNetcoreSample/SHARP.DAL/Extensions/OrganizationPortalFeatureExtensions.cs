using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Extensions
{
    public static class OrganizationPortalFeatureExtensions
    {
        public static void BuildOrganizationPortalFeatureEntity(this ModelBuilder builder)
        {
            builder.Entity<OrganizationPortalFeature>(entity =>
            {
                entity.HasKey(orgPortalFeature => orgPortalFeature.Id);

                entity.Property(highAlertStatus => highAlertStatus.Id)
                    .HasColumnName("ID");

                entity.Property(orgPortalFeature => orgPortalFeature.OrganizationId).HasColumnName("OrganizationID")
                .IsRequired();

                entity.Property(orgPortalFeature => orgPortalFeature.PortalFeatureId).HasColumnName("PortalFeatureID")
                            .IsRequired();
            });
        }
    }
}
