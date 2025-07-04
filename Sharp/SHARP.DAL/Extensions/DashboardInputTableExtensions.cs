using System;
using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
	public static class DashboardInputTableExtensions
	{
		public static void BuildDashboardInputTable(this ModelBuilder builder)
		{
			builder.Entity<DashboardInputTable>(entity =>
			{
				entity
					.HasKey(e => e.Id);

				entity
					.Property(e => e.Id);

				entity
					.Property(e => e.Name)
					.IsRequired();

                entity.Property(e => e.OrganizationId)
                    .IsRequired();

				entity.HasMany(e => e.DashboardInputGroups)
					.WithOne(dig => dig.DashboardInputTable)
					.HasForeignKey(dig => dig.TableId)
					.HasPrincipalKey(e => e.Id);
            });
		}
	}
}

