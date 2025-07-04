using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Extensions
{
    public static class SendReportToUserExtensions
    {
        public static void BuildSendReportToUserEntity(this ModelBuilder builder)
        {
            builder.Entity<SendReportToUser>(entity =>
             {
                 entity.HasKey(i => i.Id);

                 entity.Property(i => i.Id)
                        .HasColumnName("ID");

                 entity.Property(i => i.PortalReportId)
                            .HasColumnName("PortalReportID");

                 entity.Property(i => i.PortalReportId)
                    .IsRequired();


                 entity.Property(i => i.UserId)
                        .HasColumnName("UserID");

                 //entity.Property(i => i.UserId)
                 //   .IsRequired();

                 entity.Property(i => i.SendByUserId)
                         .HasColumnName("SendByUserID");

                 entity.Property(i => i.SendByUserId)
                    .IsRequired();


                 entity.Property(i => i.FacilityId)
                         .HasColumnName("FacilityID");

                 entity.Property(i => i.FacilityId)
                    .IsRequired();

                 entity.Property(i => i.CreatedAt)
                                   .HasConversion(
                                     to => to,
                                     from => DateTime.SpecifyKind(from, DateTimeKind.Utc))
                                 .IsRequired();

                 //entity.HasOne(i => i.User)
                 //   .WithMany()
                 //   .HasForeignKey(i => i.UserId);

                 entity.HasOne(i => i.SendByUser)
                   .WithMany()
                   .HasForeignKey(i => i.SendByUserId);

                 entity.HasOne(i => i.PortalReport).WithMany().
                    HasForeignKey(i => i.PortalReportId);

             });

        }
    }

}
