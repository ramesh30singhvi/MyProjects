using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class TableColumnGroupExtensions
    {
        public static void BuildTableColumnGroupEntity(this ModelBuilder builder)
        {
            builder.Entity<TableColumnGroup>(group =>
            {
                group.HasKey(gr => gr.Id);

                group.Property(gr => gr.Id).HasColumnName("ID");

                group
                    .HasOne(group => group.FormVersion)
                    .WithMany(formVersion => formVersion.Groups)
                    .HasForeignKey(group => group.FormVersionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
