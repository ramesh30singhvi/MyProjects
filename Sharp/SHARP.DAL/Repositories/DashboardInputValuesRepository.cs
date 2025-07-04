using System;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
	public class DashboardInputValuesRepository: GenericRepository<DashboardInputValues>, IDashboardInputValuesRepository
    {
		public DashboardInputValuesRepository(SHARPContext context) : base(context)
        {
        }
    }
}

