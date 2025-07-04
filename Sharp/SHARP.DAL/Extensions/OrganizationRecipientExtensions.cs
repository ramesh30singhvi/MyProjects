using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class OrganizationRecipientExtensions
    {
        public static void BuildOrganizationRecipientEntity(this ModelBuilder builder)
        {
            builder.Entity<OrganizationRecipient>(entity =>
            {
                entity.HasKey(recipient => recipient.Id);

                entity.Property(recipient => recipient.Recipient).HasMaxLength(254).IsRequired();
            });
        }
}
}
