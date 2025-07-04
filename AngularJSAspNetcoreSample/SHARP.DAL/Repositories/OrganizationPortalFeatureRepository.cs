using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Repositories
{
    public class OrganizationPortalFeatureRepository : GenericRepository<OrganizationPortalFeature>, IOrganizationPortalFeatureRepository
    {
        public OrganizationPortalFeatureRepository(SHARPContext context) : base(context)
        {
        }
    }
}
