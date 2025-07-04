using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Models;
using SHARP.DAL.Models.QueryModels;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IReportRepository : IRepository<Report>
    {
        Task<Report[]> GetAsync(ReportFilter filter, Expression<Func<Report, object>> orderBySelector);
        Task<List<string>> GetDistinctColumnAsync(ColumnQueryRule<Report> columnQueryRule);
    }
}
