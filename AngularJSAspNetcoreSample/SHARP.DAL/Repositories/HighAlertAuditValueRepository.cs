using Microsoft.EntityFrameworkCore;
using SHARP.Common.Enums;
using SHARP.Common.Extensions;
using SHARP.Common.Filtration;
using SHARP.DAL.Extensions;
using SHARP.DAL.Helpers;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories
{
    public class HighAlertAuditValueRepository : GenericRepository<HighAlertAuditValue>, IHighAlertAuditValueRepository
    {
        public HighAlertAuditValueRepository(SHARPContext context) : base(context)
        {
        }
        public async Task<HighAlertAuditValue[]> GetHighAlertAuditValueForAuditAsync(int auditId)
        {
            return await _context.HighAlertAuditValue
                             .Include(highAlert => highAlert.HighAlertStatusHistory)
                                .ThenInclude(highAlert => highAlert.HighAlertStatus)
                             .Include(highAlert => highAlert.HighAlertCategory)
                             .Where(highAlert => highAlert.AuditId == auditId).ToArrayAsync();
        }
        public async Task<HighAlertAuditValue> GetHighAlertAuditValueAsync(int id)
        {
            return await _context.HighAlertAuditValue
                 .Include(highAlert => highAlert.Audit)
                     .ThenInclude(Facility => Facility.Facility)
                        .ThenInclude(facility => facility.Organization)
                 .Include(highAlert => highAlert.HighAlertStatusHistory)
                    .ThenInclude(highAlert => highAlert.HighAlertStatus)
                 .Include(highAlert => highAlert.HighAlertCategory)
                 .FirstOrDefaultAsync(highAlert => highAlert.Id == id);
        }
        private Expression<Func<HighAlertAuditValue, bool>> BuildOrganizationCriteria(int organizationId)
        {
            if (organizationId != 0)
            {
                return i => i.Audit.Facility.OrganizationId == organizationId;
            }

            return i => true;
        }
        private Expression<Func<HighAlertAuditValue, bool>> GetFacilityExpression(HighAlertPortalFilter filter)
        {
            if (filter.Facility == null)
                return i => true;

            return i => i.Audit.Facility.Id == filter.Facility.Id ;
        }
        private Expression<Func<HighAlertAuditValue, bool>> GetOrganizationExpression(HighAlertPortalFilter filter)
        {
            if (filter.Organization == null)
                return i => true;

            return i => i.Audit.Facility.OrganizationId == filter.Organization.Id;
 ;
        }
        private Expression<Func<HighAlertAuditValue, bool>> GetHighAlertCategoryExpression(HighAlertPortalFilter filter)
        {
            if (filter.HighAlertCategory == null)
                return i => true;

            if (filter.HighAlertCategory.Id == null)
                return i => true;

            return i => i.HighAlertCategory.Id == filter.HighAlertCategory.Id;
        }
        private Expression<Func<HighAlertAuditValue, bool>> GetHighAlertStatusExpression(HighAlertPortalFilter filter)
        {
            if (filter.HighAlertStatus == null)
                return i => true;

            if (filter.HighAlertStatus.Id == null)
                return i => true;

           
            return i => i.HighAlertStatusHistory.OrderByDescending(x => x.Id).FirstOrDefault().HighAlertStatusId == filter.HighAlertStatus.Id;
        }
        private Expression<Func<HighAlertAuditValue, bool>> GetDateExpression(HighAlertPortalFilter filter)
        {
            return FilterHelper.GetFilterExpression<HighAlertAuditValue, DateTime>(
                nameof(HighAlertAuditValue.CreatedAt),
                filter.Date.FirstCondition,
                filter.Date.SecondCondition,
                filter.Date.Operator);
        }
        private Expression<Func<HighAlertAuditValue, bool>> BuildFiltersCriteria(HighAlertPortalFilter filter)
        {
            Expression<Func<HighAlertAuditValue, bool>> reportExpr = PredicateBuilder.True<HighAlertAuditValue>();

            if (filter == null)
            {
                return reportExpr;
            }

            reportExpr = reportExpr
                .And(AuditExpression())
                .AndIf(GetOrganizationExpression(filter), filter.Organization != null && filter.Organization.Id != null)
                .AndIf(GetFacilityExpression(filter), filter.Facility != null && filter.Facility.Id != null)
                .AndIf(GetHighAlertCategoryExpression(filter), filter.HighAlertCategory != null && filter.HighAlertCategory.Id != null)
                .AndIf(GetHighAlertPotentialAreasExpression(filter), filter.PotentialAreas != null && filter.PotentialAreas.Any())
                .AndIf(GetHighAlertStatusExpression(filter), filter.HighAlertStatus != null && filter.HighAlertStatus.Id != null);



            if (filter.Date != null)
            {
                reportExpr = reportExpr.And(GetDateExpression(filter));
            }


            return reportExpr;
        }

        private Expression<Func<HighAlertAuditValue, bool>> GetHighAlertPotentialAreasExpression(HighAlertPortalFilter filter)
        {
            var ids = filter.PotentialAreas.Select(x => x.Id).ToList();
            return i => i.HighAlertCategory.HighAlertCategoryToPotentialAreas.Any( x => ids.Contains(x.HighAlertPotentialAreasID));
        }

        private Expression<Func<HighAlertAuditValue, bool>> AuditExpression()
        {
            return i => i.Audit.Status == AuditStatus.Submitted;
        }

        public async Task<Tuple<HighAlertAuditValue[], int>> GetHighAlertsAsyncByPage(HighAlertPortalFilter filter, Expression<Func<HighAlertAuditValue, object>> orderBySelector)
        {
            try
            {
                var query = GetHighAlertQuery()
                .Where(BuildFiltersCriteria(filter));
                 query = query.QueryOrderBy(orderBySelector, filter.SortOrder);

                int totalCount = query.Count();
                var reports = await query.GetPagedAsync(filter.SkipCount, filter.TakeCount);

                return new Tuple<HighAlertAuditValue[], int>(reports.ToArray(), totalCount);
            }
            catch (Exception ex)
            {
                string m = ex.Message;
            }
            return new Tuple<HighAlertAuditValue[], int>(new List<HighAlertAuditValue>().ToArray(), 0);
        }

        private IQueryable<HighAlertAuditValue> GetHighAlertQuery()
        {
            return  _context.HighAlertAuditValue
                 .Include(highAlert => highAlert.Audit)
                     .ThenInclude(Facility => Facility.Facility)
                        .ThenInclude(facility => facility.Organization)
                  .Include(highAlert => highAlert.Audit)
                    .ThenInclude(report => report.PortalReport)
                 .Include(highAlert => highAlert.HighAlertStatusHistory)
                     .ThenInclude(highAlert => highAlert.HighAlertStatus)
                 .Include(highAlert => highAlert.HighAlertCategory)
                    .ThenInclude(highAlertCategory => highAlertCategory.HighAlertCategoryToPotentialAreas)
                        .ThenInclude(h => h.HighAlertPotentialAreas);
        }

        public void RemoveHighAlertByAuditValueId(int auditValueId)
        {
           var highAlert =   _context.HighAlertAuditValue.FirstOrDefault(x => x.AuditTableColumnValueId == auditValueId);
           if (highAlert != null) {
                var statusHistory = _context.HighAlertStatusHistory.Where(x => x.HighAlertAuditValueId == highAlert.Id).ToList();
                if(statusHistory.Any())
                {
                    _context.HighAlertStatusHistory.RemoveRange(statusHistory);             
                }
                _context.HighAlertAuditValue.Remove(highAlert);
           }
        }

        public async Task<Tuple<HighAlertAuditValue[], int>> GetHighAlertsAsync(HighAlertPortalFilter filter)
        {
            try
            {
                var query = GetHighAlertQuery()
                .Where(BuildFiltersCriteria(filter));
               

                int totalCount = query.Count();
                var reports = await query.ToArrayAsync();

                return new Tuple<HighAlertAuditValue[], int>(reports.ToArray(), totalCount);
            }
            catch (Exception ex)
            {
                string m = ex.Message;
            }
            return new Tuple<HighAlertAuditValue[], int>(null, 0);
        }
    }
}
