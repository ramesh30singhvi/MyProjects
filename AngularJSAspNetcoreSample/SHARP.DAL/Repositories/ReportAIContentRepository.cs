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

namespace SHARP.DAL.Repositories
{
    public class ReportAIContentRepository : GenericRepository<ReportAIContent>, IReportAIContentRepository
    {
        public ReportAIContentRepository(SHARPContext context) : base(context)
        {
        }

        public async Task<ReportAIContent[]> GetReportAsync(AuditAIReportFilter filter, Expression<Func<ReportAIContent, object>> orderBySelector)
        {
            var reportAIContent = GetReportAIContentQuery();

            return await reportAIContent
                .Where(a => a.State == filter.State)
                .Where(BuildFiltersCriteria(filter))
                .QueryOrderBy(orderBySelector, filter.SortOrder)
                .GetPagedAsync(filter.SkipCount, filter.TakeCount);
        }

        private IQueryable<ReportAIContent> GetReportAIContentQuery()
        {
            return _context.ReportAIContent
                .Include(organization => organization.Organization)
                .Include(facilities => facilities.Facility)
                .Select(r => new ReportAIContent
                {
                    Id = r.Id,
                    OrganizationId = r.OrganizationId,
                    FacilityId = r.FacilityId,
                    PdfFileName = r.PdfFileName,
                    ContainerName = r.ContainerName,
                    AuditorName = r.AuditorName,
                    AuditTime = r.AuditTime,
                    AuditDate = r.AuditDate,
                    FilteredDate = r.FilteredDate,
                    CreatedAt = r.CreatedAt,
                    Status = r.Status,
                    SubmittedDate = r.SubmittedDate,
                    SentForApprovalDate = r.SentForApprovalDate,
                    State = r.State,
                    Organization = r.Organization,
                    Facility = r.Facility
                })
                .AsNoTracking();
        }

        public async Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctColumnAsync(AuditAIReportFilterColumnSource<AuditAIReportFilterColumn> columnFilter, ColumnOptionQueryRule<ReportAIContent, FilterOptionQueryModel> queryRule)
        {
            var reportAIContents = GetAuditAIReportFilterQuery(columnFilter.AuditAIReportFilter, columnFilter.Column)
                                    .Where(r => r.State == columnFilter.AuditAIReportFilter.State);

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

        private IQueryable<ReportAIContent> GetAuditAIReportFilterQuery(AuditAIReportFilter filter, AuditAIReportFilterColumn? column = null)
        {
            var reportAIContent = GetReportAIContentQuery();

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

        private Expression<Func<ReportAIContent, bool>> BuildFiltersCriteria(AuditAIReportFilter filter, AuditAIReportFilterColumn? column = null)
        {
            Expression<Func<ReportAIContent, bool>> reportAIContentExpr = PredicateBuilder.True<ReportAIContent>();

            if (filter == null)
            {
                return reportAIContentExpr;
            }

            var expression = PredicateBuilder
            .True<ReportAIContent>()
                .AndIf(GetOrganizationExpression(filter), column != AuditAIReportFilterColumn.OrganizationName && filter.OrganizationName != null && filter.OrganizationName.Any())
                .AndIf(GetFacilityExpression(filter), column != AuditAIReportFilterColumn.FacilityName && filter.FacilityName != null && filter.FacilityName.Any())
                .AndIf(GetSummaryAIExpression(filter), column != AuditAIReportFilterColumn.SummaryAI && filter.SummaryAI != null && filter.SummaryAI.Any())
                .AndIf(GetKeywordsExpression(filter), column != AuditAIReportFilterColumn.Keywords && filter.Keywords != null && filter.Keywords.Any())
                .AndIf(GetPdfFileNameExpression(filter), column != AuditAIReportFilterColumn.PdfFileName && filter.PdfFileName != null && filter.PdfFileName.Any())
                .AndIf(GetContainerNameExpression(filter), column != AuditAIReportFilterColumn.ContainerName && filter.ContainerName != null && filter.ContainerName.Any())
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

        private Expression<Func<ReportAIContent, bool>> GetOrganizationExpression(AuditAIReportFilter filter)
        {
            return i => filter.OrganizationName.Select(option => option.Id).Contains(i.Organization.Id);
        }

        private Expression<Func<ReportAIContent, bool>> GetFacilityExpression(AuditAIReportFilter filter)
        {
            return i => filter.FacilityName.Select(option => option.Id).Contains(i.Facility.Id);
        }

        private Expression<Func<ReportAIContent, bool>> GetSummaryAIExpression(AuditAIReportFilter filter)
        {
            return i => filter.SummaryAI.Select(option => option.Value).Contains(i.SummaryAI);
        }

        private Expression<Func<ReportAIContent, bool>> GetKeywordsExpression(AuditAIReportFilter filter)
        {
            return i => filter.Keywords.Select(option => option.Value).Contains(i.Keywords);
        }

        private Expression<Func<ReportAIContent, bool>> GetPdfFileNameExpression(AuditAIReportFilter filter)
        {
            return i => filter.PdfFileName.Select(option => option.Value).Contains(i.PdfFileName);
        }

        private Expression<Func<ReportAIContent, bool>> GetContainerNameExpression(AuditAIReportFilter filter)
        {
            return i => filter.ContainerName.Select(option => option.Value).Contains(i.ContainerName);
        }

        private Expression<Func<ReportAIContent, bool>> GetAuditorNameExpression(AuditAIReportFilter filter)
        {
            return i => filter.AuditorName.Select(option => option.Value).Contains(i.AuditorName);
        }

        private Expression<Func<ReportAIContent, bool>> GetAuditTimeExpression(AuditAIReportFilter filter)
        {
            return i => filter.AuditTime.Select(option => option.Value).Contains(i.AuditTime);
        }

        private Expression<Func<ReportAIContent, bool>> GetAuditDateExpression(AuditAIReportFilter filter)
        {
            return FilterHelper.GetFilterExpression<ReportAIContent, DateTime>(
                nameof(ReportAIContent.AuditDate),
                filter.AuditDate.FirstCondition,
                filter.AuditDate.SecondCondition,
                filter.AuditDate.Operator);
        }

        private Expression<Func<ReportAIContent, bool>> GetFilteredDateExpression(AuditAIReportFilter filter)
        {
            return i => filter.FilteredDate.Select(option => option.Value).Contains(i.FilteredDate);
        }

        private Expression<Func<ReportAIContent, bool>> GetStatusExpression(AuditAIReportFilter filter)
        {
            return i => filter.Status.Select(option => (ReportAIStatus)option.Id).Contains(i.Status);
        }

        private Expression<Func<ReportAIContent, bool>> GetCreatedDateExpression(AuditAIReportFilter filter)
        {
            return FilterHelper.GetFilterExpression<ReportAIContent, DateTime>(
                nameof(ReportAIContent.CreatedAt),
                filter.CreatedAt.FirstCondition,
                filter.CreatedAt.SecondCondition,
                filter.CreatedAt.Operator);
        }

        private Expression<Func<ReportAIContent, bool>> GetSubmittedDateExpression(AuditAIReportFilter filter)
        {
            return FilterHelper.GetFilterExpression<ReportAIContent, DateTime>(
                nameof(ReportAIContent.SubmittedDate),
                filter.SubmittedDate.FirstCondition,
                filter.SubmittedDate.SecondCondition,
                filter.SubmittedDate.Operator);
        }

        public async Task<ReportAIContent> GetReportAIContentAsync(int id)
        {
            var query = _context.ReportAIContent
                        .Include(organization => organization.Organization)
                        .Include(facilities => facilities.Facility)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(reportAIContent => reportAIContent.Id == id);

            return query.Result;
        }

        public async Task<ReportAIContent> GetReportAIContentSelectedColumnsAsync(int id)
        {
            var reportAIContent = _context.ReportAIContent
                        .Include(organization => organization.Organization)
                        .Include(facilities => facilities.Facility)
                        .Select(r => new ReportAIContent
                        {
                            Id = r.Id,
                            OrganizationId = r.OrganizationId,
                            FacilityId = r.FacilityId,
                            PdfFileName = r.PdfFileName,
                            ContainerName = r.ContainerName,
                            AuditorName = r.AuditorName,
                            AuditTime = r.AuditTime,
                            AuditDate = r.AuditDate,
                            FilteredDate = r.FilteredDate,
                            CreatedAt = r.CreatedAt,
                            Status = r.Status,
                            SubmittedDate = r.SubmittedDate,
                            SentForApprovalDate = r.SentForApprovalDate,
                            State = r.State,
                            Organization = r.Organization,
                            Facility = r.Facility
                        })
                        .Single(reportAIContent => reportAIContent.Id == id);

            return reportAIContent;
        }
    }
}
