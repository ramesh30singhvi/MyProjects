using System;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
	public class DashboardInputGroupsRepository: GenericRepository<DashboardInputGroups>, IDashboardInputGroupsRepository
    {
		public DashboardInputGroupsRepository(SHARPContext context) : base(context)
        {
        }
    }
}

