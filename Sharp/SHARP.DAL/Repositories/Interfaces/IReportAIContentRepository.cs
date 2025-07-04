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
    public interface IReportAIContentRepository : IRepository<ReportAIContent>
    {
        Task<ReportAIContent[]> GetReportAsync(AuditAIReportFilter filter, Expression<Func<ReportAIContent, object>> orderBySelector);
        Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctColumnAsync(AuditAIReportFilterColumnSource<AuditAIReportFilterColumn> columnFilter, ColumnOptionQueryRule<ReportAIContent, FilterOptionQueryModel> queryRule);
        Task<ReportAIContent> GetReportAIContentAsync(int id);
        Task<ReportAIContent> GetReportAIContentSelectedColumnsAsync(int id);
    }
}
