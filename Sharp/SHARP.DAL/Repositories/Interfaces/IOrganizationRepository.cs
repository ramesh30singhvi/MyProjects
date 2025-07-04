using System.Collections.Generic;
using System.Threading.Tasks;
using SHARP.DAL.Models;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IOrganizationRepository : IRepository<Organization>
    {
        public Task<Organization> SingleAsync(int id);
        public Task<ICollection<Organization>> GetFullAsync(ICollection<int> userOrganizationIds);
        public Task<Organization> GetOneAsync(int id);

    }
}
