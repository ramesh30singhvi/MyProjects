using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class OrganizationMemoExtensions
    {
        public static void BuildOrganizationMemoEntity(this ModelBuilder builder)
        {
            builder.Entity<OrganizationMemo>(entity =>
            {
                entity.HasKey(nameof(OrganizationMemo.OrganizationId), nameof(OrganizationMemo.MemoId));

                entity.Property(organizationMemo => organizationMemo.OrganizationId)
                    .IsRequired();

                entity.Property(organizationMemo => organizationMemo.MemoId)
                    .IsRequired();

                entity.HasOne(organizationMemo => organizationMemo.Organization)
                    .WithMany(organization => organization.OrganizationMemos)
                    .HasForeignKey(organizationMemo => organizationMemo.OrganizationId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(organizationMemo => organizationMemo.Memo)
                    .WithMany(memo => memo.OrganizationMemos)
                    .HasForeignKey(organizationMemo => organizationMemo.MemoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
