using Microsoft.EntityFrameworkCore;
using SHARP.Common.Extensions;
using SHARP.Common.Filtration;
using SHARP.DAL.Helpers;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;
using SHARP.DAL.Extensions;

namespace SHARP.DAL.Repositories
{
    public class DownloadsTrackingRepository : GenericRepository<DownloadsTracking>, IDownloadsTrackingRepository
    {
        public DownloadsTrackingRepository(SHARPContext context) : base(context)
        {
        }

        public async Task<DownloadsTracking> GetPortalDownloadsTrackingByUserIdPortalReportIdAsync(int userId, int portalReportId)
        {
            return await _context.DownloadsTracking
                .Include(x => x.PortalReport)
                .Where(x => x.UserId == userId && x.PortalReportId == portalReportId)
                .Select(s => new DownloadsTracking
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    DateAndTime = s.DateAndTime,
                    PortalReportId = s.PortalReportId,
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Tuple<DownloadsTracking[], int>> GetPortalDownloadsTrackingAsync(PortalDownloadsTrackingViewFilter filter, Expression<Func<DownloadsTracking, object>> orderBySelector)
        {
            try
            {
                var query = GetDownloadsTrackingQuery()
                .Where(BuildFiltersCriteria(filter));
                if (!string.IsNullOrEmpty(filter.Search))
                    query = query.Where(x => x.PortalReport.Name.Contains(filter.Search));

                query = query.QueryOrderBy(orderBySelector, filter.SortOrder);

                int totalCount = query.Count();

                DownloadsTracking[] downloadsTrackings = null;

                if (filter.TakeCount == -1)
                {
                    downloadsTrackings = query.ToArray();
                }
                else
                {
                    downloadsTrackings = query
                        .Skip(filter.SkipCount)
                        .Take(filter.TakeCount)
                        .ToArray();
                }
                //var reports = await query.GetPagedAsync(filter.SkipCount, filter.TakeCount);

                return new Tuple<DownloadsTracking[], int>(downloadsTrackings.ToArray(), totalCount);
            }
            catch (Exception ex)
            {
                string m = ex.Message;
            }
            return new Tuple<DownloadsTracking[], int>(new List<DownloadsTracking>().ToArray(), 0);
        }

        private IQueryable<DownloadsTracking> GetDownloadsTrackingQuery()
        {
            return _context.DownloadsTracking
                .Include(x => x.PortalReport)
                .Include(x => x.User)
                .AsEnumerable() // Switch to client-side evaluation
                .GroupBy(g => g.PortalReportId)
                .Select(s => new DownloadsTracking
                {
                    PortalReportId = s.Key,
                    NumberOfDownloads = s.Count(),
                    DateAndTime = s.Max(x => x.DateAndTime),
                    PortalReport = s.Select(x => x.PortalReport).FirstOrDefault(),
                    PortalReportCreatedAt = s.Select(x => x.PortalReport.CreatedAt).FirstOrDefault(),
                    DownloadsTrackingDetails = s.Select(x => new DownloadsTrackingDetails
                    {
                        Username = x.User.FullName,
                        DateAndTime = x.DateAndTime
                    }).OrderByDescending(x => x.DateAndTime).ToList()
                })
                .AsQueryable()
                .AsNoTracking();
        }

        private Expression<Func<DownloadsTracking, bool>> BuildFiltersCriteria(PortalDownloadsTrackingViewFilter filter)
        {
            Expression<Func<DownloadsTracking, bool>> downloadsTrackingExpr = PredicateBuilder.True<DownloadsTracking>();

            if (filter == null)
            {
                return downloadsTrackingExpr;
            }

            if (filter.Date != null)
            {
                downloadsTrackingExpr = downloadsTrackingExpr.And(GetDateExpression(filter));
            }


            return downloadsTrackingExpr;
        }

        private Expression<Func<DownloadsTracking, bool>> GetDateExpression(PortalDownloadsTrackingViewFilter filter)
        {
            return FilterHelper.GetFilterExpression<DownloadsTracking, DateTime>(
                nameof(DownloadsTracking.PortalReportCreatedAt),
                filter.Date.FirstCondition,
                filter.Date.SecondCondition,
                filter.Date.Operator);
        }
    }
}
