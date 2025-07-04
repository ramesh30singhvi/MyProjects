using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories
{
    public class AuditTableColumnValueRepository : GenericRepository<AuditTableColumnValue>, IAuditTableColumnValueRepository
    {
        public AuditTableColumnValueRepository(SHARPContext context) : base(context)
        {
        }

        public async Task<AuditTableColumnValue[]> GetAsync(int auditId)
        {
            return await _entities
                .Include(columnValue => columnValue.Column)
                    .ThenInclude(column => column.Group)
                .Include(columnValue => columnValue.HighAlertAuditValue)
                    .ThenInclude(highAlert => highAlert.HighAlertStatusHistory)
                        .ThenInclude(highAlert => highAlert.HighAlertStatus)

                .Include(columnValue => columnValue.HighAlertAuditValue)
                    .ThenInclude(highAlert => highAlert.HighAlertCategory)

                .Where(columnValue => columnValue.AuditId == auditId)
                .OrderBy(columnValue => columnValue.Column.Sequence)
                .ToArrayAsync();
        }

        public async Task<AuditTableColumnValue> GetById(int auditValueId)
        {
            return await _entities
                .Include(columnValue => columnValue.Column)
                    .ThenInclude(column => column.Group)
                .Include(columnValue => columnValue.HighAlertAuditValue)
                    .ThenInclude(highAlert => highAlert.HighAlertStatusHistory)
                        .ThenInclude(highAlert => highAlert.HighAlertStatus)

                .Include(columnValue => columnValue.HighAlertAuditValue)
                    .ThenInclude(highAlert => highAlert.HighAlertCategory)

                .FirstOrDefaultAsync(columnValue => columnValue.Id == auditValueId);
        }

        public async Task<IReadOnlyCollection<AuditTableColumnValue>> GetTrakerGroupValues(int auditId, string answersGroupId)
        {
            return await _entities
                .Include(columnValue => columnValue.Column)
                    .ThenInclude(column => column.TrackerOption)
                .Where(columnValue => columnValue.AuditId == auditId && columnValue.GroupId == answersGroupId)
                .OrderBy(columnValue => columnValue.Column.Sequence)
                .ToArrayAsync();
        }
    }
}
