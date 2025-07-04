using System;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
	public class DashboardInputElementRepository: GenericRepository<DashboardInputElement>, IDashboardInputElementRepository
    {
		public DashboardInputElementRepository(SHARPContext context) : base(context)
        {
        }
    }
}

