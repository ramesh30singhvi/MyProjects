using Microsoft.EntityFrameworkCore;
using SHARP.Common.Filtration;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;
using SHARP.DAL.Extensions;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Models.QueryModels;
using SHARP.Common.Extensions;
using SHARP.DAL.Helpers;
using SHARP.Common.Enums;
using Newtonsoft.Json.Linq;
using EFCore.BulkExtensions;

namespace SHARP.DAL.Repositories
{
    public class AuditAIReportV2Repository : GenericRepository<AuditAIReportV2>, IAuditAIReportV2Repository
    {
        public AuditAIReportV2Repository(SHARPContext context) : base(context)
        {
        }

        public async Task<AuditAIReportV2[]> GetReportAsync(AuditAIReportFilter filter, Expression<Func<AuditAIReportV2, object>> orderBySelector)
        {
            var reportAIContent = GetAuditAIReportQuery();

            return await reportAIContent
                .Where(a => a.State == filter.State)
                .Where(BuildFiltersCriteria(filter))
                .QueryOrderBy(orderBySelector, filter.SortOrder)
                .GetPagedAsync(filter.SkipCount, filter.TakeCount);
        }

        private IQueryable<AuditAIReportV2> GetAuditAIReportQuery()
        {
            return _context.AuditAIReportV2
                .Include(organization => organization.Organization)
                .Include(facilities => facilities.Facility)
                .AsNoTracking();
        }
        private IQueryable<AuditAIReportV2> GetAuditAIReportQueryWithValues()
        {
            return _context.AuditAIReportV2
                .Include(audit => audit.Organization)
                .Include(audit => audit.Facility)
                .Include(audit => audit.Values)
                    .ThenInclude(info => info.Summaries)
                .AsNoTracking();
        }
        public async Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctColumnAsync(AuditAIReportFilterColumnSource<AuditAIReportFilterColumn> columnFilter, ColumnOptionQueryRule<AuditAIReportV2, FilterOptionQueryModel> queryRule)
        {
            var reportAIContents = GetAuditAIReportFilterQuery(columnFilter.AuditAIReportFilter, columnFilter.Column);
               
            if(columnFilter.AuditAIReportFilter != null)
            {
                reportAIContents = reportAIContents.Where(r => r.State == columnFilter.AuditAIReportFilter.State);
            }

            IQueryable<FilterOptionQueryModel> columnValues;
            if (queryRule.SingleSelector != null)
            {
                columnValues = reportAIContents.Select(queryRule.SingleSelector);
            }
            else
            {
                columnValues = reportAIContents.SelectMany(queryRule.ManySelector);
            }

            return await columnValues
                .Distinct()
                .ToArrayAsync();
        }

        private IQueryable<AuditAIReportV2> GetAuditAIReportFilterQuery(AuditAIReportFilter filter, AuditAIReportFilterColumn? column = null)
        {
            var reportAIContent = GetAuditAIReportQuery();

            if (filter != null && !string.IsNullOrWhiteSpace(filter.Search) && reportAIContent.Any())
            {

                reportAIContent = reportAIContent.Where(report =>
                         report.AuditorName.Contains(filter.Search)
                         || report.Organization.Name.Contains(filter.Search)
                         || report.Facility.Name.Contains(filter.Search));
            }
            else
            {
                reportAIContent = reportAIContent.Where(BuildFiltersCriteria(filter, column));
            }

            return reportAIContent;
        }

        private Expression<Func<AuditAIReportV2, bool>> BuildFiltersCriteria(AuditAIReportFilter filter, AuditAIReportFilterColumn? column = null)
        {
            Expression<Func<AuditAIReportV2, bool>> reportAIContentExpr = PredicateBuilder.True<AuditAIReportV2>();

            if (filter == null)
            {
                return reportAIContentExpr;
            }

            var expression = PredicateBuilder
            .True<AuditAIReportV2>()
                .AndIf(GetOrganizationExpression(filter), column != AuditAIReportFilterColumn.OrganizationName && filter.OrganizationName != null && filter.OrganizationName.Any())
                .AndIf(GetFacilityExpression(filter), column != AuditAIReportFilterColumn.FacilityName && filter.FacilityName != null && filter.FacilityName.Any())
                .AndIf(GetAuditorNameExpression(filter), column != AuditAIReportFilterColumn.AuditorName && filter.AuditorName != null && filter.AuditorName.Any())
                .AndIf(GetAuditTimeExpression(filter), column != AuditAIReportFilterColumn.AuditTime && filter.AuditTime != null && filter.AuditTime.Any())
                .AndIf(GetFilteredDateExpression(filter), column != AuditAIReportFilterColumn.FilteredDate && filter.FilteredDate != null && filter.FilteredDate.Any())
                .AndIf(GetStatusExpression(filter), column != AuditAIReportFilterColumn.Status && filter.Status != null && filter.Status.Any());

            if (filter.CreatedAt != null)
            {
                expression = expression.And(GetCreatedDateExpression(filter));
            }
            if (filter.AuditDate != null)
            {
                expression = expression.And(GetAuditDateExpression(filter));
            }
            if (filter.SubmittedDate != null)
            {
                expression = expression.And(GetSubmittedDateExpression(filter));
            }

            return expression;
        }

        private Expression<Func<AuditAIReportV2, bool>> GetOrganizationExpression(AuditAIReportFilter filter)
        {
            return i => filter.OrganizationName.Select(option => option.Id).Contains(i.Organization.Id);
        }

        private Expression<Func<AuditAIReportV2, bool>> GetFacilityExpression(AuditAIReportFilter filter)
        {
            return i => filter.FacilityName.Select(option => option.Id).Contains(i.Facility.Id);
        }


        private Expression<Func<AuditAIReportV2, bool>> GetAuditorNameExpression(AuditAIReportFilter filter)
        {
            return i => filter.AuditorName.Select(option => option.Value).Contains(i.AuditorName);
        }

        private Expression<Func<AuditAIReportV2, bool>> GetAuditTimeExpression(AuditAIReportFilter filter)
        {
            return i => filter.AuditTime.Select(option => option.Value).Contains(i.AuditTime);
        }

        private Expression<Func<AuditAIReportV2, bool>> GetAuditDateExpression(AuditAIReportFilter filter)
        {
            return FilterHelper.GetFilterExpression<AuditAIReportV2, DateTime>(
                nameof(AuditAIReport.AuditDate),
                filter.AuditDate.FirstCondition,
                filter.AuditDate.SecondCondition,
                filter.AuditDate.Operator);
        }

        private Expression<Func<AuditAIReportV2, bool>> GetFilteredDateExpression(AuditAIReportFilter filter)
        {
            return i => filter.FilteredDate.Select(option => option.Value).Contains(i.FilteredDate);
        }

        private Expression<Func<AuditAIReportV2, bool>> GetStatusExpression(AuditAIReportFilter filter)
        {
            return i => filter.Status.Select(option => (ReportAIStatus)option.Id).Contains(i.Status);
        }

        private Expression<Func<AuditAIReportV2, bool>> GetCreatedDateExpression(AuditAIReportFilter filter)
        {
            return FilterHelper.GetFilterExpression<AuditAIReportV2, DateTime>(
                nameof(AuditAIReport.CreatedAt),
                filter.CreatedAt.FirstCondition,
                filter.CreatedAt.SecondCondition,
                filter.CreatedAt.Operator);
        }

        private Expression<Func<AuditAIReportV2, bool>> GetSubmittedDateExpression(AuditAIReportFilter filter)
        {
            return FilterHelper.GetFilterExpression<AuditAIReportV2, DateTime>(
                nameof(AuditAIReport.SubmittedDate),
                filter.SubmittedDate.FirstCondition,
                filter.SubmittedDate.SecondCondition,
                filter.SubmittedDate.Operator);
        }

        public async Task<AuditAIReportV2> GetAuditAIReportAsync(int id)
        {
            return await _context.AuditAIReportV2
                        .Include(organization => organization.Organization)
                        .Include(facilities => facilities.Facility)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(reportAIContent => reportAIContent.Id == id);

            
        }

        public async Task<AuditAIReportV2> GetAuditAIReportSelectedColumnsAsync(int id)
        {
            var reportAIContent = _context.AuditAIReportV2
                        .Include(organization => organization.Organization)
                        .Include(facilities => facilities.Facility)                   
                        .Single(reportAIContent => reportAIContent.Id == id);

            return reportAIContent;
        }

        public async Task<IReadOnlyCollection<AuditAIReportV2>> GetAuditsAsync(AuditAIReportFilter filter, Expression<Func<AuditAIReportV2, object>> orderBySelector)
        {
            var reportAIContent = GetAuditAIReportQuery();

            return await reportAIContent
                .Where(a => a.State == filter.State)
                .Where(BuildFiltersCriteria(filter))
                .QueryOrderBy(orderBySelector, filter.SortOrder)
                .GetPagedAsync(filter.SkipCount, filter.TakeCount);
        }

        public async Task<AuditAIReportV2> GetAIAuditAsync(int id)
        {
            var audit = await GetAuditAIReportQueryWithValues().FirstOrDefaultAsync(x => x.Id == id);
            await _context.BulkReadAsync(audit.Values.ToList());
            return audit;
        }
    }
}
