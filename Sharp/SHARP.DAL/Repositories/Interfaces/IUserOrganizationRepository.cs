using System.Collections.Generic;
using System.Threading.Tasks;
using SHARP.DAL.Models;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IUserOrganizationRepository : IRepository<UserOrganization>
    {
        IEnumerable<UserOrganization> GetUsersForOrganization(int organizationId);
    }
}
