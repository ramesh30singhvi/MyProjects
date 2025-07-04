using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IAuditTriggeredRepository : IRepository<AuditTriggeredByKeyword>
    {
        Task<AuditTriggeredByKeyword[]> GetAsync(int auditId,int tableColumnId);

        Task<AuditTriggeredByKeyword[]> GetAsync(int auditId);

        Task<bool> IsAuditCreatedByKeywordTrigger(int auditId);
        Task<AuditTriggeredByKeyword[]> GetByAuditTableColumnValue(int auditValueId);
    }
}
