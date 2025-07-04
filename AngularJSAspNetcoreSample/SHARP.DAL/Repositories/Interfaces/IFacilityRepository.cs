using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Models;
using SHARP.DAL.Models.QueryModels;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IFacilityRepository : IRepository<Facility>
    {
        Task<Facility[]> GetAsync(FacilityFilter filter, Expression<Func<Facility, object>> orderBySelector);

        Task<Facility> GetFacilityAsync(int id);

        Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctColumnAsync(
            FacilityFilterColumnSource<FacilityFilterColumn> columnData,
            Expression<Func<Facility, FilterOptionQueryModel>> columnSelector);

        Task<IReadOnlyCollection<Facility>> GetFacilityOptionsAsync(FacilityOptionFilter filter);
        Task<Facility> GetFacilityByNameAsync(string name);

        Task<IReadOnlyCollection<Facility>> GetFacilitiesByNameAsync(string name);

        Task<List<AuditSummary>> GetAuditSummaryAsync(int facilityId, string fromDate, string toDate);

    }
}
