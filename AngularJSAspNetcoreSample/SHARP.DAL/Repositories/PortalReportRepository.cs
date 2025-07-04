using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SHARP.DAL.Extensions;
using SHARP.Common.Filtration;
using System.Linq.Expressions;
using SHARP.Common.Extensions;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Helpers;

namespace SHARP.DAL.Repositories
{
    public class PortalReportRepository : GenericRepository<PortalReport>, IPortalReportRepository
    {
        public PortalReportRepository(SHARPContext context) : base(context)
        {
        }
        private Expression<Func<PortalReport, bool>> BuildOrganizationCriteria(int organizationId)
        {
            if (organizationId != 0)
            {
                return i => i.Organization.Id == organizationId;
            }

            return i => true;
        }
        private Expression<Func<PortalReport, bool>> GetFacilityExpression(PortalReportFilter filter)
        {
            if(filter.Facilities == null)
                return i => true;

            if (!filter.Facilities.Any())
                return i => true;

            return i => filter.Facilities.Select(option => option.Id).Contains(i.FacilityId);
        }

        private Expression<Func<PortalReport,bool>> GetReportByRangeId(PortalReportFilter filter)
        {
            if(filter.ReportIds == null)
                return i => true;

            if (!filter.ReportIds.Any())
                return i => true;

            return i => filter.ReportIds.Contains(i.Id);
        }
        private Expression<Func<PortalReport, bool>> GetOrganizationExpression(PortalReportFilter filter)
        {
            if (filter.Organization == null)
                return i => true;

            if (filter.Organization.Id == null)
                return i => true;

            return i => i.OrganizationId == filter.Organization.Id;
        }
        private Expression<Func<PortalReport, bool>> GetReportCategoryExpression(PortalReportFilter filter)
        {
            if(filter.ReportCategory == null)
                return i => true;

            if (filter.ReportCategory.Id == null)
                return i => true;

            return i => i.ReportCategory.Id == filter.ReportCategory.Id;
        }

        private Expression<Func<PortalReport, bool>> GetDateExpression(PortalReportFilter filter)
        {
            return FilterHelper.GetFilterExpression<PortalReport, DateTime>(
                nameof(PortalReport.CreatedAt),
                filter.Date.FirstCondition,
                filter.Date.SecondCondition,
                filter.Date.Operator);
        }
        private Expression<Func<PortalReport, bool>> BuildFiltersCriteria(PortalReportFilter filter)
        {
            Expression<Func<PortalReport, bool>> reportExpr = PredicateBuilder.True<PortalReport>();

            if (filter == null)
            {
                return reportExpr;
            }

            reportExpr = reportExpr
                .AndIf(GetOrganizationExpression(filter), filter.Organization != null && filter.Organization.Id != null)
                .AndIf(GetFacilityExpression(filter), filter.Facilities != null && filter.Facilities.Any())
                
                .AndIf(GetReportCategoryExpression(filter), filter.ReportCategory != null );



            if (filter.Date != null)
            {
                reportExpr = reportExpr.And(GetDateExpression(filter));
            }


            return reportExpr;
        }
        public async Task<Tuple<PortalReport[],int>> GetPortalReportsAsync(PortalReportFilter filter, Expression<Func<PortalReport, object>> orderBySelector)
        {
            try
            {
                var query = GetPortalReportQuery()
                .Where(BuildFiltersCriteria(filter))
                .OrderBy( x => x.CreatedAt);

                int totalCount = query.Count();
              //  var rep = await query.AnyAsync<PortalReport>(); //query.GetPagedAsync(filter.SkipCount, filter.TakeCount);
         
                return new Tuple<PortalReport[], int>(query.ToArray(),totalCount);
            }
            catch (Exception ex)
            {
                string m = ex.Message;
            }
            return new Tuple<PortalReport[], int>(new List<PortalReport>().ToArray(),0);
        }
        public async Task<PortalReport> GetPortalReportAsync(int id)
        {
            return await _context.PortalReport
                .Include(report => report.Facility)
                        .ThenInclude(facility => facility.Organization)
                .Include(report => report.Audit)
                .Include(report => report.ReportCategory)
                .Include(report => report.ReportType)
                .Include(report => report.AuditType)
                .Include(report => report.User)
                .FirstOrDefaultAsync(report => report.Id == id);
        }

        public async Task<Tuple<IReadOnlyCollection<PortalReport>, int>> GetPortalReportsAsync(IReadOnlyCollection<int> selectedReports)
        {
            try
            {
                var query = GetPortalReportQuery()
                .Where(x => selectedReports.Contains(x.Id));

                int totalCount = query.Count();
                //  var rep = await query.AnyAsync<PortalReport>(); //query.GetPagedAsync(filter.SkipCount, filter.TakeCount);

                return new Tuple<IReadOnlyCollection<PortalReport>, int>(query.ToArray(), totalCount);
            }
            catch (Exception ex)
            {
                string m = ex.Message;
            }
            return new Tuple<IReadOnlyCollection<PortalReport>, int>(new List<PortalReport>().ToArray(), 0);
        }

        private IQueryable<PortalReport> GetPortalReportQuery()
        {
            return _context.PortalReport
                .Include(report => report.Facility)
                        .ThenInclude(facility => facility.Organization)
                .Include(report => report.Audit)
                .Include(report => report.ReportCategory)
                .Include(report => report.ReportType)
                .Include(report => report.User)
                .Include(report => report.AuditType);

        }

        public async Task<Tuple<PortalReport[], int>> GetPortalReportsAsyncByPage(PortalReportFilter filter, Expression<Func<PortalReport, object>> orderBySelector)
        {
            try
            {
                var query = GetPortalReportQuery()
                .Where(BuildFiltersCriteria(filter));
                if (!string.IsNullOrEmpty(filter.Search))
                    query = query.Where(x => x.Name.Contains(filter.Search));

                query = query.QueryOrderBy(orderBySelector, filter.SortOrder);


                int totalCount = filter.ReportIds != null && filter.ReportIds.Any() ? filter.ReportIds.Count : query.Count();
                var reports = await query.GetPagedAsync(filter.SkipCount, filter.TakeCount);

                if (filter.ReportIds != null &&  filter.ReportIds.Any())
                    reports = reports.Where(x => filter.ReportIds.Contains(x.Id)).ToArray();

                return new Tuple<PortalReport[], int>(reports.ToArray(), totalCount);
            }
            catch (Exception ex)
            {
                string m = ex.Message;
            }
            return new Tuple<PortalReport[], int>(new List<PortalReport>().ToArray(), 0);
        }
    }
}
