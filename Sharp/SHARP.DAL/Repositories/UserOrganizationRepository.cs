using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
    public class UserOrganizationRepository : GenericRepository<UserOrganization>, IUserOrganizationRepository
    {
        public UserOrganizationRepository(SHARPContext context) : base(context)
        {
        }

        public IEnumerable<UserOrganization> GetUsersForOrganization(int organizationId)
        {
            return _context.UserOrganization.Include(organization => organization.User)
                .Where(x => x.OrganizationId == organizationId );
               
        }
    }
}
