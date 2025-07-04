using SHARP.Common.Filtration;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IHighAlertAuditValueRepository : IRepository<HighAlertAuditValue>
    {
        Task<HighAlertAuditValue> GetHighAlertAuditValueAsync(int id);

        Task<HighAlertAuditValue[]> GetHighAlertAuditValueForAuditAsync(int auditId);
        Task<Tuple<HighAlertAuditValue[], int>> GetHighAlertsAsyncByPage(HighAlertPortalFilter filter, Expression<Func<HighAlertAuditValue, object>> orderBySelector);

        Task<Tuple<HighAlertAuditValue[], int>> GetHighAlertsAsync(HighAlertPortalFilter filter);
        void  RemoveHighAlertByAuditValueId(int auditValueId);
    }
}
