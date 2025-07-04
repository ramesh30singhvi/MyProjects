using Microsoft.EntityFrameworkCore;
using SHARP.Common.Enums;
using SHARP.Common.Extensions;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Extensions;
using SHARP.DAL.Helpers;
using SHARP.DAL.Models;
using SHARP.DAL.Models.QueryModels;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SHARP.DAL.Repositories
{
    public class ReportRequestRepository : GenericRepository<ReportRequest>, IReportRequestRepository
    {
        public ReportRequestRepository(SHARPContext context) : base(context)
        {
        }

        public async Task<IReadOnlyCollection<ReportRequest>> GetListAsync(ReportRequestFilter filter, Expression<Func<ReportRequest, object>> orderBySelector)
        {
            var query = GetReportRequestQuery()
                .Where(BuildUserCriteria(filter.UserId))
                .Where(BuildFiltersCriteria(filter));

            if (orderBySelector != null)
            { 
                query = query.QueryOrderBy(orderBySelector, filter.SortOrder);
            }
            else
            {
                query = query.OrderByDescending(request => request.RequestedTime);
            }

            return await query                
                .GetPagedAsync(filter.SkipCount, filter.TakeCount);
        }

        public async Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctColumnAsync(
            ReportRequestFilterColumnSource<ReportRequestFilterColumn> columnData,
            Expression<Func<ReportRequest, FilterOptionQueryModel>> columnSelector)
        {
            var query = GetReportRequestQuery()
                    .Where(BuildUserCriteria(columnData.UserId))
                    .Where(BuildFiltersCriteria(columnData.ReportRequestFilter, columnData.Column))
                    .Select(columnSelector)
                .Distinct();
            /*.OrderBy(i => i.Value);

            if (columnSelector.Body.Type == typeof(FilterOptionQueryModel))
            {
                query = query.ThenBy(i => i.Id);
            }*/

            return await query.ToArrayAsync();
        }

        private IQueryable<ReportRequest> GetReportRequestQuery()
        {
            return _context.ReportRequests
                .Include(request => request.User)
                .Include(request => request.Organization)
                .Include(request => request.Facility)
                .Include(request => request.Form);
        }

        private Expression<Func<ReportRequest, bool>> BuildUserCriteria(int? userId)
        {
            if (userId.HasValue)
            {
                return i => i.UserId == userId;
            }

            return i => true;
        }

        private Expression<Func<ReportRequest, bool>> BuildFiltersCriteria(ReportRequestFilter filter, ReportRequestFilterColumn? column = null)
        {
            Expression<Func<ReportRequest, bool>> reportRequestExpr = PredicateBuilder.True<ReportRequest>();

            if (filter == null)
            {
                return reportRequestExpr;
            }

            reportRequestExpr = reportRequestExpr
                .AndIf(GetOrganizationExpression(filter), column != ReportRequestFilterColumn.OrganizationName && filter.Organization != null && filter.Organization.Any())
                .AndIf(GetFacilityExpression(filter), column != ReportRequestFilterColumn.FacilityName && filter.Facility != null && filter.Facility.Any())
                .AndIf(GetFormExpression(filter), column != ReportRequestFilterColumn.FormName && filter.Form != null && filter.Form.Any())
                .AndIf(GetAuditTypeExpression(filter), column != ReportRequestFilterColumn.AuditType && filter.AuditType != null && filter.AuditType.Any())
                .AndIf(GetUserExpression(filter), column != ReportRequestFilterColumn.UserFullName && filter.User != null && filter.User.Any())
                .AndIf(GetStatusExpression(filter), column != ReportRequestFilterColumn.Status && filter.Status != null && filter.Status.Any());

            if (filter.FromDate != null)
            {
                reportRequestExpr = reportRequestExpr.And(GetFromDateExpression(filter));
            }

            if (filter.ToDate != null)
            {
                reportRequestExpr = reportRequestExpr.And(GetToDateExpression(filter));
            }

            if (filter.RequestedTime != null)
            {
                reportRequestExpr = reportRequestExpr.And(GetRequestedTimeExpression(filter));
            }

            if (filter.GeneratedTime != null)
            {
                reportRequestExpr = reportRequestExpr.And(GetGeneratedTimeExpression(filter));
            }

            return reportRequestExpr;
        }

        private Expression<Func<ReportRequest, bool>> GetOrganizationExpression(ReportRequestFilter filter)
        {
            return i => filter.Organization.Select(option => option.Id).Contains(i.Organization.Id);
        }

        private Expression<Func<ReportRequest, bool>> GetFacilityExpression(ReportRequestFilter filter)
        {
            return i => filter.Facility.Select(option => option.Id).Contains(i.Facility.Id);
        }

        private Expression<Func<ReportRequest, bool>> GetFormExpression(ReportRequestFilter filter)
        {
            return i => filter.Form.Select(option => option.Id).Contains(i.Form.Id);
        }

        private Expression<Func<ReportRequest, bool>> GetAuditTypeExpression(ReportRequestFilter filter)
        {
            return i => filter.AuditType.Select(option => option.Value).Contains(i.AuditType);
        }

        private Expression<Func<ReportRequest, bool>> GetUserExpression(ReportRequestFilter filter)
        {
            return i => filter.User.Select(option => option.Id).Contains(i.User.Id);
        }

        private Expression<Func<ReportRequest, bool>> GetStatusExpression(ReportRequestFilter filter)
        {
            return i => filter.Status.Select(option => (ReportRequestStatus)option.Id).Contains(i.Status);
        }

        private Expression<Func<ReportRequest, bool>> GetFromDateExpression(ReportRequestFilter filter)
        {
            return FilterHelper.GetFilterExpression<ReportRequest, DateTime>(
                nameof(ReportRequest.FromDate),
                filter.FromDate.FirstCondition,
                filter.FromDate.SecondCondition,
                filter.FromDate.Operator);
        }

        private Expression<Func<ReportRequest, bool>> GetToDateExpression(ReportRequestFilter filter)
        {
            return FilterHelper.GetFilterExpression<ReportRequest, DateTime>(
                nameof(ReportRequest.ToDate),
                filter.ToDate.FirstCondition,
                filter.ToDate.SecondCondition,
                filter.ToDate.Operator);
        }

        private Expression<Func<ReportRequest, bool>> GetRequestedTimeExpression(ReportRequestFilter filter)
        {
            return FilterHelper.GetFilterExpression<ReportRequest, DateTime>(
                nameof(ReportRequest.RequestedTime),
                filter.RequestedTime.FirstCondition,
                filter.RequestedTime.SecondCondition,
                filter.RequestedTime.Operator);
        }

        private Expression<Func<ReportRequest, bool>> GetGeneratedTimeExpression(ReportRequestFilter filter)
        {
            return FilterHelper.GetFilterExpression<ReportRequest, DateTime>(
                nameof(ReportRequest.GeneratedTime),
                filter.GeneratedTime.FirstCondition,
                filter.GeneratedTime.SecondCondition,
                filter.GeneratedTime.Operator);
        }

        public Task<ReportRequest> GetReportRequestAsync(int id)
        {
            return  _context.ReportRequests
                .Include(request => request.User)
                .Include(request => request.Organization)
                .Include(request => request.Facility)
                .Include(request => request.Form)
                .FirstOrDefaultAsync(request => request.Id == id);
        }
    }
}
