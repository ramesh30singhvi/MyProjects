using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class TwoFATokenExtensions
    {
        public static void BuildTwoFATokenEntity(this ModelBuilder builder)
        {
            builder.Entity<TwoFAToken>(entity =>
            {
                entity.ToTable("TwoFAToken");

                entity.Property(e => e.Id).HasColumnName("Id").HasMaxLength(450).IsUnicode(true);
                entity.Property(e => e.Token).HasColumnName("Token").HasMaxLength(70).IsUnicode(true);
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");

                entity
                    .HasOne(e => e.User)
                    .WithOne(user => user.Token)
                    .HasForeignKey<TwoFAToken>(e => e.Id)
                    .HasPrincipalKey<ApplicationUser>(user => user.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
