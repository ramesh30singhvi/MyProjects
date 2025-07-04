using Microsoft.EntityFrameworkCore;
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

namespace SHARP.DAL.Repositories
{
    public class FacilityRepository : GenericRepository<Facility>, IFacilityRepository
    {
        public FacilityRepository(SHARPContext context) : base(context)
        {
        }

        public async Task<Facility[]> GetAsync(FacilityFilter filter, Expression<Func<Facility, object>> orderBySelector)
        {
            var facilities = GetFacilityQuery()
                .Where(BuildFiltersCriteria(filter));

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                facilities = facilities.Where(facility =>
                    facility.Name.Contains(filter.Search));
            }

            return await facilities
                .QueryOrderBy(orderBySelector, filter.SortOrder)
                .GetPagedAsync(filter.SkipCount, filter.TakeCount);
        }

        public async Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctColumnAsync(
            FacilityFilterColumnSource<FacilityFilterColumn> columnData, 
            Expression<Func<Facility, FilterOptionQueryModel>> columnSelector)
        {
            return await GetFacilityQuery()
                .Where(BuildFiltersCriteria(columnData.FacilityFilter, columnData.Column))
                .Select(columnSelector)
                .Distinct()
                .ToArrayAsync();
        }

        public Task<Facility> GetFacilityAsync(int id)
        {
            return GetFacilityQuery()
                .FirstOrDefaultAsync(facility => facility.Id == id);
        }

        public async Task<IReadOnlyCollection<Facility>> GetFacilityOptionsAsync(FacilityOptionFilter filter)
        {
            var facilities = _context.Facility
                .AsQueryable();

            if(!string.IsNullOrWhiteSpace(filter.Search))
            {
                facilities = facilities.Where(facility =>
                    facility.Name.Contains(filter.Search));
            }

            if(filter.OrganizationIds.Any())
            {
                facilities = facilities.Where(facility => filter.OrganizationIds.Contains(facility.OrganizationId));
            }
            if (filter.FacilityIds!=null)
            {
                if (filter.FacilityIds.Any())
                {
                    facilities = facilities.Where(facility => filter.FacilityIds.Contains(facility.Id));
                }
            }
            

            return await facilities
                .OrderBy(facility => facility.Name)
                .GetPagedAsync(filter.SkipCount, filter.TakeCount);
        }

        private Expression<Func<Facility, bool>> BuildFiltersCriteria(FacilityFilter filter, FacilityFilterColumn? column = null)
        {
            Expression<Func<Facility, bool>> facilityExpr = PredicateBuilder.True<Facility>();

            if (filter == null)
            {
                return facilityExpr;
            }

            return PredicateBuilder
                .True<Facility>()
                .And(GetOrganizationExpression(filter))
                .AndIf(GetNameExpression(filter), column != FacilityFilterColumn.Name && filter.Name != null && filter.Name.Any())                
                .AndIf(GetTimeZoneExpression(filter), column != FacilityFilterColumn.TimeZoneName && filter.TimeZone != null && filter.TimeZone.Any())
                .AndIf(GetStatusExpression(filter), column != FacilityFilterColumn.Active && filter.Active != null && filter.Active.Any());
        }

        private Expression<Func<Facility, bool>> GetNameExpression(FacilityFilter filter)
        {
            return i => filter.Name.Select(option => option.Value).Contains(i.Name);
        }

        private Expression<Func<Facility, bool>> GetTimeZoneExpression(FacilityFilter filter)
        {
            return i => filter.TimeZone.Select(option => option.Id).Contains(i.TimeZone.Id);
        }

        private Expression<Func<Facility, bool>> GetStatusExpression(FacilityFilter filter)
        {
            return i => filter.Active.Select(option => option.Id == 1).Any(status => status == i.Active);
        }

        private Expression<Func<Facility, bool>> GetOrganizationExpression(FacilityFilter filter)
        {
            return i => filter.OrganizationId == i.OrganizationId;
        }

        private IQueryable<Facility> GetFacilityQuery()
        {
            return _context.Facility
                .Include(facility => facility.Organization)
                .Include(facility => facility.TimeZone)
                .Include(facility => facility.Recipients);
        }
        private IQueryable<Facility> GetFacilityWithPortalFeaturesQuery()
        {
            return _context.Facility
                .Include(facility => facility.Organization)
                    .ThenInclude(org => org.PortalFeatures)
                        .ThenInclude(portF => portF.PortalFeature)
                .Include(facility => facility.TimeZone)
                .Include(facility => facility.Recipients);
        }
        public async  Task<Facility> GetFacilityByNameAsync(string name)
        {
            return await GetFacilityWithPortalFeaturesQuery()
            .FirstOrDefaultAsync(facility => facility.Name.ToLower() == name.ToLower());
        }

        public async Task<IReadOnlyCollection<Facility>> GetFacilitiesByNameAsync(string name)
        {
            var facilities =  await GetFacilityWithPortalFeaturesQuery().Where(x  => x.Name.ToLower() == name.ToLower()).ToArrayAsync();

            return facilities;
        }

        public async Task<List<AuditSummary>> GetAuditSummaryAsync(int facilityId, string fromDate, string toDate)
        {
            List<AuditSummary> auditSummary = await _context.AuditSummary.FromSqlInterpolated($"GetAuditSummary {facilityId}, {fromDate}, {toDate}").ToListAsync();

            return auditSummary;
        }
    }
}