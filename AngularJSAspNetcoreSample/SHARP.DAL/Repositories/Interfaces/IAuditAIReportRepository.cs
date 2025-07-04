using SHARP.Common.Enums;
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
    public interface IAuditAIReportRepository : IRepository<AuditAIReport>
    {
        Task<AuditAIReport[]> GetReportAsync(AuditAIReportFilter filter, Expression<Func<AuditAIReport, object>> orderBySelector);
        Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctColumnAsync(AuditAIReportFilterColumnSource<AuditAIReportFilterColumn> columnFilter, ColumnOptionQueryRule<AuditAIReport, FilterOptionQueryModel> queryRule);
        Task<AuditAIReport> GetAuditAIReportAsync(int id);
        Task<AuditAIReport> GetAuditAIReportSelectedColumnsAsync(int id);

        Task MigrateToSQLCompress();
       
        Task<int> InsertAuditAIReportAsync(AuditAIReport reportAIContent);

        Task UpdateAuditAIReportAsync(
                int id,
                int? organizationId = null,
                int? facilityId = null,
                byte[] summaryAI = null,
                string keywords = null,
                string pdfFileName = null,
                string auditorName = null,
                string auditTime = null,
                string auditDate = null,
                string filteredDate = null,
                DateTime? createdAt = null,
                int? status = null,
                DateTime? submittedDate = null,
                DateTime? sentForApprovalDate = null,
                int? state = null);
        Task<AuditAIReport[]> GetSubmittedAuditsPerDateAndOrganization(AuditorProductivitySummaryPerFacilityFilter filter);
    }
}
