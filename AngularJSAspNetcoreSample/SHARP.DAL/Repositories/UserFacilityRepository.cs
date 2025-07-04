using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
    public class UserFacilityRepository : GenericRepository<UserFacility>, IUserFacilityRepository
    {
        public UserFacilityRepository(SHARPContext context) : base(context)
        {
        }
    }
}
