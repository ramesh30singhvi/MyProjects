using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Models;
using SHARP.DAL.Models.QueryModels;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IFormVersionRepository : IRepository<FormVersion>
    {
        Task<IReadOnlyCollection<FormVersion>> GetFormVersionsByOrganizationAsync(int facilityId, string auditType, CancellationToken ct = default);

        Task<FormVersion[]> GetAsync(FormVersionFilter filter, Expression<Func<FormVersion, object>> orderBySelector, List<int> maxFormVersionIds = null);

        Task<FormVersionQueryModel[]> GetFormVersionIdsAsync(FormVersionFilter filter);

        Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctColumnAsync(
            FormManagementFilterColumnSource<FormVersionColumn> columnData,
            ColumnOptionQueryRule<FormVersion, FilterOptionQueryModel> columnQueryRule);

        Task<FormVersion> GetFormVersionKeywordsAsync(int formVersionId);

        Task<FormVersion> GetFormVersionAsync(int formVersionId);

        Task<FormVersion> GetLastActiveFormVersionAsync(int formId);
    }
}
