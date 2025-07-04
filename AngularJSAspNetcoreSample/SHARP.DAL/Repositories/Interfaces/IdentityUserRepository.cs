using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public class IdentityUserRepository : GenericRepository<ApplicationUser>, IIdentityUserRepository
    {
        public IdentityUserRepository(SHARPContext context) : base(context)
        {
        }

        public async Task<ApplicationUser[]> GetAsync(IEnumerable<string> ids)
        {
            return await _context.Users
                .Include(user => user.UserRoles)
                    .ThenInclude(userRole => userRole.Role)
                .Where(user => ids.Contains(user.Id))
                .ToArrayAsync();
        }
    }
}
