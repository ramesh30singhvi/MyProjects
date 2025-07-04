using SHARP.Common.Filtration;
using SHARP.DAL.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IFormRepository : IRepository<Form>
    {
        Task<IReadOnlyCollection<Form>> GetFormsByOrganizationAsync(int organizationId, string auditType, CancellationToken ct = default);

        Task<List<Form>> GetFallFormsByOrganizationAsync(int organizationId);

        Task<List<Form>> GetWoundFormsByOrganizationAsync(int organizationId);

        Task<IReadOnlyCollection<Form>> GetFormOptionsAsync(FormOptionFilter filter);
    }
}
