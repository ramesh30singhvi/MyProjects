using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class AuditSettingExtensions
    {
        public static void BuildAuditSettingEntity(this ModelBuilder builder)
        {
            builder.Entity<AuditSetting>(entity =>
            {
                entity.HasKey(i => i.Id);

                entity.Property(i => i.AuditId)
                    .IsRequired();

                entity.Property(i => i.Type)
                    .IsRequired();

                entity.HasOne(i => i.Audit)
                    .WithMany(audit => audit.Settings)
                    .HasForeignKey(i => i.AuditId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
