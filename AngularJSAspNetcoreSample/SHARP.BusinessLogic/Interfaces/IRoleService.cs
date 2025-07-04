using SHARP.BusinessLogic.DTO.Role;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAsync();
    }
}
