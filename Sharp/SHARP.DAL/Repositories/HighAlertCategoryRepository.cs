using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories
{
    public class HighAlertCategoryRepository : GenericRepository<HighAlertCategory>, IHighAlertCategoryRepository
    {
        public HighAlertCategoryRepository(SHARPContext context) : base(context)
        {
        }
        public async Task<HighAlertCategory[]> GetAllActiveCategories()
        {
            return await _context.HighAlertCategory.
                Include( x => x.HighAlertCategoryToPotentialAreas)
                    .ThenInclude( a => a.HighAlertPotentialAreas)
                .Where(x => x.Active).OrderBy(x => x.Name).ToArrayAsync();
        }

    }
}
