using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Extensions
{
    public static class UserTeamExtensions
    {
        public static void BuildUserTeamEntity(this ModelBuilder builder)
        {
            builder.Entity<UserTeam>(entity =>
            {
                entity.HasKey(userTeam => userTeam.Id);

                entity.Property(userTeam => userTeam.UserId)
                    .IsRequired();

                entity.Property(userTeam => userTeam.TeamId)
                   .IsRequired();


            });
        }
    }
}
