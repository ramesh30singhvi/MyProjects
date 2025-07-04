using SHARP.Common.Filtration;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IAuditAIReportV2Repository : IRepository<AuditAIReportV2>
    {
        Task<AuditAIReportV2> GetAIAuditAsync(int id);
        Task<IReadOnlyCollection<AuditAIReportV2>> GetAuditsAsync(AuditAIReportFilter filter, Expression<Func<AuditAIReportV2, object>> orderBySelector);
    }
}
