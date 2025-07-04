using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class FacilityTimeZoneExtensions
    {
        public static void BuildFacilityTimeZoneEntity(this ModelBuilder builder)
        {
            builder.Entity<FacilityTimeZone>(entity =>
            {
                entity.HasKey(timeZone => timeZone.Id);

                entity.Property(timeZone => timeZone.Name)
                    .IsRequired();

                entity.Property(timeZone => timeZone.DisplayName)
                    .IsRequired();

                entity.Property(timeZone => timeZone.ShortName)
                    .IsRequired();
            });
        }
    }
}
