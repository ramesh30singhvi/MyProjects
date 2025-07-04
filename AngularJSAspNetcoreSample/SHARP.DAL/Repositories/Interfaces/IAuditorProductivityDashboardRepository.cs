using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Models.QueryModels;
using SHARP.DAL.Models.Views;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IAuditorProductivityDashboardRepository : IRepository<AuditorProductivityInputView>
    {
        Task<AuditorProductivityInputView[]> GetAuditorProductivityInputAsync(AuditorProductivityInputFilter filter, Expression<Func<AuditorProductivityInputView, object>> orderBySelector);
        Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctInputColumnAsync(AuditorProductivityInputFilterColumnSource<AuditorProductivityInputFilterColumn> columnFilter, ColumnOptionQueryRule<AuditorProductivityInputView, FilterOptionQueryModel> queryRule);

        Task<AuditorProductivityAHTPerAuditTypeView[]> GetAuditorProductivityAHTPerAuditTypeAsync(AuditorProductivityAHTPerAuditTypeFilter filter, Expression<Func<AuditorProductivityAHTPerAuditTypeView, object>> orderBySelector);
        Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctAHTPerAuditTypeColumnAsync(AuditorProductivityAHTPerAuditTypeFilterColumnSource<AuditorProductivityAHTPerAuditTypeFilterColumn> columnFilter, ColumnOptionQueryRule<AuditorProductivityAHTPerAuditTypeView, FilterOptionQueryModel> queryRule);

        Task<AuditorProductivitySummaryPerAuditorView[]> GetAuditorProductivitySummaryPerAuditorAsync(AuditorProductivitySummaryPerAuditorFilter filter, Expression<Func<AuditorProductivitySummaryPerAuditorView, object>> orderBySelector);
        Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctSummaryPerAuditorColumnAsync(AuditorProductivitySummaryPerAuditorFilterColumnSource<AuditorProductivitySummaryPerAuditorFilterColumn> columnFilter, ColumnOptionQueryRule<AuditorProductivitySummaryPerAuditorView, FilterOptionQueryModel> queryRule);
    }
}
