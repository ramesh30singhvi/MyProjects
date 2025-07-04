using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Extensions
{
    public static class PortalFeatureExtensions
    {
        public static void BuildPortalFeatureEntity(this ModelBuilder builder)
        {
            builder.Entity<PortalFeature>(entity =>
            {
                entity.HasKey(portalFeature => portalFeature.Id);

                entity.Property(portalFeature => portalFeature.Id)
                    .HasColumnName("ID");

                entity.Property(portalFeature => portalFeature.Name)
                    .IsRequired();
            });
        }
    }
}
