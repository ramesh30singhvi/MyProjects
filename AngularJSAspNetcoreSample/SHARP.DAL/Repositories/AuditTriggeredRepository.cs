using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories
{
    public class AuditTriggeredRepository : GenericRepository<AuditTriggeredByKeyword>, IAuditTriggeredRepository
    {
        public AuditTriggeredRepository(SHARPContext context) : base(context)
        {
        }

        public async Task<AuditTriggeredByKeyword[]> GetAsync(int auditId, int tableColumnId)
        {
            return await _context.AuditTriggeredByKeyword.Where(x => x.AuditId == auditId && x.AuditTableColumnValueId == tableColumnId).ToArrayAsync();
        }

        public async Task<AuditTriggeredByKeyword[]> GetAsync(int auditId)
        {
            return await _context.AuditTriggeredByKeyword.Where(x => x.AuditId == auditId).ToArrayAsync();
        }

        public async Task<AuditTriggeredByKeyword[]> GetByAuditTableColumnValue(int auditValueId)
        {
            return await _context.AuditTriggeredByKeyword.Where(x => x.AuditTableColumnValueId == auditValueId).ToArrayAsync();
        }

        public async Task<bool> IsAuditCreatedByKeywordTrigger(int auditId)
        {
            var audit = await _context.AuditTriggeredByKeyword.FirstOrDefaultAsync(x => x.CreatedAuditId == auditId);
            return audit != null;
        }
    }
}
