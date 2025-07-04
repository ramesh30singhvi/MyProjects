using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Repositories
{
    public class HighAlertPotentialAreasRepository : GenericRepository<HighAlertPotentialAreas>, IHighAlertPotentialAreasRepository
    {
        public HighAlertPotentialAreasRepository(SHARPContext context) : base(context)
        {
        }
    }
}
