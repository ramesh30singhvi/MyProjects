using Microsoft.EntityFrameworkCore;
using SHARP.Common.Extensions;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Extensions;
using SHARP.DAL.Helpers;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories
{
    public class ReportRepository : GenericRepository<Report>, IReportRepository
    {
        public ReportRepository(SHARPContext context) : base(context)
        {
        }

        public async Task<Report[]> GetAsync(ReportFilter filter, Expression<Func<Report, object>> orderBySelector)
        {
            var reports = GetReportQuery()
                .Where(BuildFiltersCriteria(filter));

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                reports = reports.Where(report =>report.Name.Contains(filter.Search));
            }

            return await reports
                .QueryOrderBy(orderBySelector, filter.SortOrder)
                .GetPagedAsync(filter.SkipCount, filter.TakeCount);
        }

        private Expression<Func<Report, bool>> BuildFiltersCriteria(ReportFilter filter)
        {
            return PredicateBuilder
                .True<Report>()
                .AndIf(GetNameExpression(filter), filter.Name.Any());
        }

        public async Task<List<string>> GetDistinctColumnAsync(ColumnQueryRule<Report> columnQueryRule)
        {
            var users = GetReportQuery();

            IQueryable<object> columnValues;
            if (columnQueryRule.SingleSelector != null)
            {
                columnValues = users.Select(columnQueryRule.SingleSelector);
            }
            else
            {
                columnValues = users.SelectMany(columnQueryRule.ManySelector);
            }

            return await columnValues
                .Where(i => i != null)
                .Distinct()
                .Select(i => i.ToString())
                .ToListAsync();
        }

        private Expression<Func<Report, bool>> GetNameExpression(ReportFilter filter)
        {
            return i => filter.Name.Contains(i.Name);
        }

        private IQueryable<Report> GetReportQuery()
        {
            return _context.Report;
        }
    }
}