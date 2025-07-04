using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;

namespace SHARP.DAL.Extensions
{
    public static class FacilityExtensions
    {
        public static void BuildFacilityEntity(this ModelBuilder builder)
        {
            builder.Entity<Facility>(entity =>
            {
                entity.HasKey(facility => facility.Id);

                entity.Property(facility => facility.Id)
                    .HasColumnName("ID");

                entity.Property(facility => facility.Name)
                    .IsRequired();

                entity.Property(facility => facility.OrganizationId)
                    .HasColumnName("OrganizationID")
                    .IsRequired();

                entity.Property(facility => facility.Active)
                    .IsRequired();

                entity.Property(facility => facility.TimeZoneId)
                    .IsRequired();

                entity.Property(facility => facility.LegacyId)
                    .HasColumnName("LegacyID");

                entity.HasOne(facility => facility.Organization)
                    .WithMany(org => org.Facilities)
                    .HasForeignKey(facility => facility.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(facility => facility.TimeZone)
                    .WithMany()
                    .HasForeignKey(facility => facility.TimeZoneId);
            });
        }
    }
}
