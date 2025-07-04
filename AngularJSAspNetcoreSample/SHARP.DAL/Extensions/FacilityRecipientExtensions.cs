using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class FacilityRecipientExtensions
    {
        public static void BuildFacilityRecipientEntity(this ModelBuilder builder)
        {
            builder.Entity<FacilityRecipient>(entity =>
            {
                entity.HasKey(recipient => recipient.Id);

                entity.Property(recipient => recipient.Email)
                    .IsRequired();

                entity.Property(recipient => recipient.FacilityId)
                    .IsRequired();

                entity.HasOne(recipient => recipient.Facility)
                    .WithMany(org => org.Recipients)
                    .HasForeignKey(facility => facility.FacilityId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
