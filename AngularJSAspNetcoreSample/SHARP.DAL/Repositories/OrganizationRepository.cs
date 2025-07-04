using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
    public class OrganizationRepository : GenericRepository<Organization>, IOrganizationRepository
    {
        public OrganizationRepository(SHARPContext context) : base(context)
        {
        }

        public async Task<Organization> SingleAsync(int id) => await _entities
                .Include(organization => organization.Recipients)
                .Include(organization => organization.Facilities)
                .Include(organization => organization.PortalFeatures)
                .SingleAsync(organization => organization.Id == id);

        public async Task<ICollection<Organization>> GetFullAsync(ICollection<int> userOrganizationIds)
        {

            var organizations = _context.Organization.AsQueryable();


            organizations = organizations
                .Include(o => o.DashboardInputTables)
                    .ThenInclude(dit => dit.DashboardInputGroups)
                        .ThenInclude(dig => dig.DashboardInputElements)
                            .ThenInclude(die => die.DashboardInputValues.Where(div => div.Date == DateTime.Today))
                                .ThenInclude(div => div.Facility)
                .Include(o => o.Facilities.Where(facilty => facilty.Active == true))
                .Include(o => o.PortalFeatures);
            if (organizations.Any())
            {
                organizations = organizations.Where(org => userOrganizationIds.Contains(org.Id));
            }


            return await organizations.AsNoTracking().ToListAsync();
        }

        async Task<Organization> IOrganizationRepository.GetOneAsync(int id)
        {
            return await _entities.Include(o => o.DashboardInputTables)
                .ThenInclude(dit => dit.DashboardInputGroups)
                .ThenInclude(dig => dig.DashboardInputElements)
                .ThenInclude(die => die.DashboardInputValues.Where(div => div.Date == DateTime.Today))
                .ThenInclude(div => div.Facility)
                .Include(o => o.PortalFeatures.Where(x => x.Available))
                    .ThenInclude(feature => feature.PortalFeature)
                .Include(o => o.Facilities.Where(facilty => facilty.Active == true))
                .SingleAsync(organization => organization.Id == id);
        }
    }
}