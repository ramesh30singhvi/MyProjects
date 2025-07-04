using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
    public class FacilityTimeZoneRepository : GenericRepository<FacilityTimeZone>, IFacilityTimeZoneRepository
    {
        public FacilityTimeZoneRepository(SHARPContext context) : base(context)
        {
        }
    }
}