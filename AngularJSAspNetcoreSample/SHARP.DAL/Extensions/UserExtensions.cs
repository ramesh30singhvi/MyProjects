using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class UserExtensions
    {
        public static void BuildUserEntity(this ModelBuilder builder)
        {
            builder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.FirstName).HasColumnName("FirstName").HasMaxLength(255).IsUnicode(false);
                entity.Property(e => e.LastName).HasColumnName("LastName").HasMaxLength(255).IsUnicode(false);
                entity.Property(e => e.Email).HasColumnName("Email").HasMaxLength(450).IsUnicode(false);
                entity.Property(e => e.UserId).HasColumnName("UserID").HasMaxLength(255).IsUnicode(false);
                entity.Property(e => e.FullName).HasColumnName("FullName").HasMaxLength(511).IsUnicode(false);
                entity.Property(e => e.Status).HasColumnName("Status");
                entity.Property(e => e.TimeZone).IsRequired();

                entity.HasMany(user => user.UserOrganizations)
                    .WithOne(organization => organization.User)
                    .HasForeignKey(user => user.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(shar => shar.IdentityUser)
                    .WithOne(identity => identity.SharUser)
                    .HasForeignKey<User>(shar => shar.UserId)
                    .HasPrincipalKey<ApplicationUser>(identity => identity.Id)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
