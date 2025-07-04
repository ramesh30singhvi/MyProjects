using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class TrustedFEClientExtensions
    {
        public static void BuildTrustedFEClientEntity(this ModelBuilder builder)
        {
            builder.Entity<TrustedFEClient>(clientBuilder =>
            {
                clientBuilder.ToTable("TrustedFEClient");

                clientBuilder
                    .Property(client => client.ID)
                    .HasColumnName("ID");
                clientBuilder.Property(client => client.UserID)
                    .HasColumnName("UserID")
                    .IsRequired();

                clientBuilder.HasOne(client => client.User)
                    .WithMany(org => org.TrustedFEClient)
                    .HasForeignKey(client => client.UserID)
                    .OnDelete(DeleteBehavior.Restrict);

                clientBuilder
                    .Property(client => client.UserAgent)
                    .HasColumnName("UserAgent")
                    .HasColumnType("NVARCHAR(MAX)")
                    .IsRequired();

                clientBuilder
                    .Property(client => client.IP)
                    .HasColumnName("IP")
                    .HasMaxLength(45)
                    .IsRequired();
            });
        }
    }
}


