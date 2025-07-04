using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class AuditTypeExtensions
    {
        public static void BuildAuditTypeEntity(this ModelBuilder builder)
        {
            builder.Entity<AuditType>(entity =>
            {
                entity.HasKey(auditType => auditType.Id);

                entity.Property(auditType => auditType.Id)
                    .HasColumnName("ID");

                entity.Property(auditType => auditType.Name)
                    .IsRequired();
            });
        }
    }
}
