using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SHARP.Common.Extensions;
using SHARP.Common.Filtration;
using SHARP.DAL.Extensions;
using SHARP.DAL.Helpers;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
    public class LoginsTrackingRepository : GenericRepository<LoginsTracking>, ILoginsTrackingRepository
    {
        public LoginsTrackingRepository(SHARPContext context) : base(context)
        {
        }

        public async Task<LoginsTracking> GetLatestLoginsTrackingByUserId(int userId)
        {
            return await _context.LoginsTracking
                .Include(tracking => tracking.User)
                .Where(tracking => tracking.UserId == userId)
                .OrderByDescending(tracking => tracking.Login)
                .FirstOrDefaultAsync();
        }

        public async Task<Tuple<LoginsTracking[], int>> GetPortalLoginsTrackingAsync(PortalLoginsTrackingViewFilter filter, Expression<Func<LoginsTracking, object>> orderBySelector)
        {
            try
            {
                var query = GetLoginsTrackingQuery()
                .Where(BuildFiltersCriteria(filter));
                if (!string.IsNullOrEmpty(filter.Search))
                    query = query.Where(x => x.User.FullName.Contains(filter.Search));

                query = query.QueryOrderBy(orderBySelector, filter.SortOrder);

                int totalCount = query.Count();
                var reports = await query.GetPagedAsync(filter.SkipCount, filter.TakeCount);

                return new Tuple<LoginsTracking[], int>(reports.ToArray(), totalCount);
            }
            catch (Exception ex)
            {
                string m = ex.Message;
            }
            return new Tuple<LoginsTracking[], int>(new List<LoginsTracking>().ToArray(), 0);
        }

        private IQueryable<LoginsTracking> GetLoginsTrackingQuery()
        {
            return _context.LoginsTracking
                .Include(tracking => tracking.User)
                .AsNoTracking();
        }

        private Expression<Func<LoginsTracking, bool>> BuildFiltersCriteria(PortalLoginsTrackingViewFilter filter)
        {
            Expression<Func<LoginsTracking, bool>> loginsTrackingExpr = PredicateBuilder.True<LoginsTracking>();

            if (filter == null)
            {
                return loginsTrackingExpr;
            }

            if (filter.Date != null)
            {
                loginsTrackingExpr = loginsTrackingExpr.And(GetDateExpression(filter));
            }


            return loginsTrackingExpr;
        }

        private Expression<Func<LoginsTracking, bool>> GetDateExpression(PortalLoginsTrackingViewFilter filter)
        {
            return FilterHelper.GetFilterExpression<LoginsTracking, DateTime>(
                nameof(LoginsTracking.Login),
                filter.Date.FirstCondition,
                filter.Date.SecondCondition,
                filter.Date.Operator);
        }
    }
}
