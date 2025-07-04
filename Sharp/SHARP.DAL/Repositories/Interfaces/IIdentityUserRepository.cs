using SHARP.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IIdentityUserRepository : IRepository<ApplicationUser>
    {
        public Task<ApplicationUser[]> GetAsync(IEnumerable<string> ids);
    }
}
