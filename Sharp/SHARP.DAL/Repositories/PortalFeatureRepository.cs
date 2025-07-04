using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Repositories
{
    public class PortalFeatureRepository : GenericRepository<PortalFeature>, IPortalFeatureRepository
    {
        public PortalFeatureRepository(SHARPContext context) : base(context)
        {
        }
    }
}
