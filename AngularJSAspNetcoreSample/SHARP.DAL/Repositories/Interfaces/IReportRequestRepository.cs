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
    public interface IReportRequestRepository : IRepository<ReportRequest>
    {
        Task<IReadOnlyCollection<ReportRequest>> GetListAsync(
            ReportRequestFilter reportRequestFilter, 
            Expression<Func<ReportRequest, object>> orderBySelector);

        Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctColumnAsync(
            ReportRequestFilterColumnSource<ReportRequestFilterColumn> columnData, 
            Expression<Func<ReportRequest, FilterOptionQueryModel>> columnSelector);

        Task<ReportRequest> GetReportRequestAsync(int id);
    }
}
