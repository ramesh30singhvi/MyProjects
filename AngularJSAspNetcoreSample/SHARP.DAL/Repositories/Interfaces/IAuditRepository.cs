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
    public interface IAuditRepository : IRepository<Audit>
    {
        public Task<Tuple<Audit[], int>> GetListAsync(AuditFilter filter, Expression<Func<Audit, object>> orderBySelector);

        Task<Audit> GetAuditAsync(int id);

        Task<Audit> GetAuditWithTypeAsync(int id);

        Task<Audit> GetAuditForDownloadAsync(int id);

        Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctColumnAsync(AuditFilterColumnSource<AuditFilterColumn> columnData, Expression<Func<Audit, FilterOptionQueryModel>> columnSelector);

        Task<Audit> GetAuditForCriteriaPdfAsync(PdfFilter filter);

        Task<Audit> GetAuditForDuplicateAsync(int auditId);

        Task<IReadOnlyCollection<AuditKPIQueryModel>> GetAuditKPIsAsync(DashboardFilter filter);

        Task<int> GetAuditDueDateCountAsync(DashboardFilter filter, Expression<Func<Audit, bool>> dueDatePredicate);

        Task<bool> ExistsSubmittedAuditsAsync(PdfFilter filter);

        Task<IReadOnlyCollection<Audit>> GetSubmittedAuditsAsync(PdfFilter filter);
        Task<List<Audit>> GetAuditsOfFacility(int facilityId);
        Task<List<Audit>> GetSubmitedAuditsAsyncForFallAndWound(List<int> formIds, ReportFallFilterModel reportFallFilterModel);
        Task<List<Audit>> GetAuditsByUserTimeAndShift(int organizationId, int userId, DateTime startTime, DateTime endTime);
        Task<List<Audit>> GetCompliantResidentsForCriteriaFilter(int organizationID, int[] facilityIDs, int[] formVersionIds, string fromDate, string toDate, string fromAuditDate, string toAuditDate, int questionId, int compliantType);

        Task<List<Audit>> GetSubmittedAuditsPerDateAndOrganization(AuditorProductivitySummaryPerFacilityFilter filter);
    }
}
