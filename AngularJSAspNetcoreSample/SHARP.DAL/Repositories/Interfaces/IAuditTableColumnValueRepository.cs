using SHARP.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IAuditTableColumnValueRepository : IRepository<AuditTableColumnValue>
    {
        Task<AuditTableColumnValue[]> GetAsync(int auditId);

        Task<AuditTableColumnValue>  GetById(int auditValueId);

        Task<IReadOnlyCollection<AuditTableColumnValue>> GetTrakerGroupValues(int auditId, string answersGroupId);
    }
}
