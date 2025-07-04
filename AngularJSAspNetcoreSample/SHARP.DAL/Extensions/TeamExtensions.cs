using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Extensions
{
    public static class TeamExtensions
    {
        public static void BuildTeamEntity(this ModelBuilder builder)
        {
            builder.Entity<Team>(entity =>
            {
                entity.HasKey(team => team.Id);
                entity.Property(team => team.Id)
                    .HasColumnName("ID");

                entity.Property(team => team.Name)
                    .IsRequired();


            });
        }
    }
}
