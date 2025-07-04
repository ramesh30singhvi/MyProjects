using System;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
	public class DashboardInputTableRepository: GenericRepository<DashboardInputTable>, IDashboardInputTableRepository
    {
		public DashboardInputTableRepository(SHARPContext context) : base(context)
        {
        }
    }
}

