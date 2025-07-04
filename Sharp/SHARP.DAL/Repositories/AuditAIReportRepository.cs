using Microsoft.EntityFrameworkCore;
using SHARP.Common.Filtration;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;
using SHARP.DAL.Extensions;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Models.QueryModels;
using SHARP.Common.Extensions;
using SHARP.DAL.Helpers;
using SHARP.Common.Enums;
using System.ComponentModel;
using System.IO.Compression;
using System.IO;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Data.Common;

namespace SHARP.DAL.Repositories
{
    public class AuditAIReportRepository : GenericRepository<AuditAIReport>, IAuditAIReportRepository
    {
        public AuditAIReportRepository(SHARPContext context) : base(context)
        {
        }

        public async Task<AuditAIReport[]> GetReportAsync(AuditAIReportFilter filter, Expression<Func<AuditAIReport, object>> orderBySelector)
        {
            var reportAIContent = GetAuditAIReportQuery();

            return await reportAIContent
                .Where(a => a.State == filter.State)
                .Where(BuildFiltersCriteria(filter))
                .QueryOrderBy(orderBySelector, filter.SortOrder)
                .GetPagedAsync(filter.SkipCount, filter.TakeCount);
        }

        private IQueryable<AuditAIReport> GetAuditAIReportQuery()
        {
            return _context.AuditAIReport
                .Include(organization => organization.Organization)
                .Include(facilities => facilities.Facility)
                .Select(r => new AuditAIReport
                {
                    Id = r.Id,
                    Keywords = "",
                 
                    OrganizationId = r.OrganizationId,
                    FacilityId = r.FacilityId,
                    PdfFileName = r.PdfFileName,
                    AuditorName = r.AuditorName,
                    AuditTime = r.AuditTime,
                    AuditDate = r.AuditDate,
                    FilteredDate = r.FilteredDate,
                    CreatedAt = r.CreatedAt,
                    Status = r.Status,
                    SubmittedDate = r.SubmittedDate,
                    SentForApprovalDate = r.SentForApprovalDate,
                    State = r.State,
                    Organization = r.Organization,
                    Facility = r.Facility
                })
                .AsNoTracking();
        }

        public async Task<IReadOnlyCollection<FilterOptionQueryModel>> GetDistinctColumnAsync(AuditAIReportFilterColumnSource<AuditAIReportFilterColumn> columnFilter, ColumnOptionQueryRule<AuditAIReport, FilterOptionQueryModel> queryRule)
        {
            var reportAIContents = GetAuditAIReportFilterQuery(columnFilter.AuditAIReportFilter, columnFilter.Column);
               
            if(columnFilter.AuditAIReportFilter != null)
            {
                reportAIContents = reportAIContents.Where(r => r.State == columnFilter.AuditAIReportFilter.State);
            }

            IQueryable<FilterOptionQueryModel> columnValues;
            if (queryRule.SingleSelector != null)
            {
                columnValues = reportAIContents.Select(queryRule.SingleSelector);
            }
            else
            {
                columnValues = reportAIContents.SelectMany(queryRule.ManySelector);
            }

            return await columnValues
                .Distinct()
                .ToArrayAsync();
        }

        private IQueryable<AuditAIReport> GetAuditAIReportFilterQuery(AuditAIReportFilter filter, AuditAIReportFilterColumn? column = null)
        {
            var reportAIContent = GetAuditAIReportQuery();

            if (filter != null && !string.IsNullOrWhiteSpace(filter.Search) && reportAIContent.Any())
            {

                reportAIContent = reportAIContent.Where(report =>
                         report.AuditorName.Contains(filter.Search)
                         || report.Organization.Name.Contains(filter.Search)
                         || report.Facility.Name.Contains(filter.Search));
            }
            else
            {
                reportAIContent = reportAIContent.Where(BuildFiltersCriteria(filter, column));
            }

            return reportAIContent;
        }

        private Expression<Func<AuditAIReport, bool>> BuildFiltersCriteria(AuditAIReportFilter filter, AuditAIReportFilterColumn? column = null)
        {
            Expression<Func<AuditAIReport, bool>> reportAIContentExpr = PredicateBuilder.True<AuditAIReport>();

            if (filter == null)
            {
                return reportAIContentExpr;
            }

            var expression = PredicateBuilder
            .True<AuditAIReport>()
                .AndIf(GetOrganizationExpression(filter), column != AuditAIReportFilterColumn.OrganizationName && filter.OrganizationName != null && filter.OrganizationName.Any())
                .AndIf(GetFacilityExpression(filter), column != AuditAIReportFilterColumn.FacilityName && filter.FacilityName != null && filter.FacilityName.Any())
                .AndIf(GetAuditorNameExpression(filter), column != AuditAIReportFilterColumn.AuditorName && filter.AuditorName != null && filter.AuditorName.Any())
                .AndIf(GetAuditTimeExpression(filter), column != AuditAIReportFilterColumn.AuditTime && filter.AuditTime != null && filter.AuditTime.Any())
                .AndIf(GetFilteredDateExpression(filter), column != AuditAIReportFilterColumn.FilteredDate && filter.FilteredDate != null && filter.FilteredDate.Any())
                .AndIf(GetStatusExpression(filter), column != AuditAIReportFilterColumn.Status && filter.Status != null && filter.Status.Any());

            if (filter.CreatedAt != null)
            {
                expression = expression.And(GetCreatedDateExpression(filter));
            }
            if (filter.AuditDate != null)
            {
                expression = expression.And(GetAuditDateExpression(filter));
            }
            if (filter.SubmittedDate != null)
            {
                expression = expression.And(GetSubmittedDateExpression(filter));
            }

            return expression;
        }

        private Expression<Func<AuditAIReport, bool>> GetOrganizationExpression(AuditAIReportFilter filter)
        {
            return i => filter.OrganizationName.Select(option => option.Id).Contains(i.Organization.Id);
        }

        private Expression<Func<AuditAIReport, bool>> GetFacilityExpression(AuditAIReportFilter filter)
        {
            return i => filter.FacilityName.Select(option => option.Id).Contains(i.Facility.Id);
        }


        private Expression<Func<AuditAIReport, bool>> GetAuditorNameExpression(AuditAIReportFilter filter)
        {
            return i => filter.AuditorName.Select(option => option.Value).Contains(i.AuditorName);
        }

        private Expression<Func<AuditAIReport, bool>> GetAuditTimeExpression(AuditAIReportFilter filter)
        {
            return i => filter.AuditTime.Select(option => option.Value).Contains(i.AuditTime);
        }

        private Expression<Func<AuditAIReport, bool>> GetAuditDateExpression(AuditAIReportFilter filter)
        {
            return FilterHelper.GetFilterExpression<AuditAIReport, DateTime>(
                nameof(AuditAIReport.AuditDate),
                filter.AuditDate.FirstCondition,
                filter.AuditDate.SecondCondition,
                filter.AuditDate.Operator);
        }

        private Expression<Func<AuditAIReport, bool>> GetFilteredDateExpression(AuditAIReportFilter filter)
        {
            return i => filter.FilteredDate.Select(option => option.Value).Contains(i.FilteredDate);
        }

        private Expression<Func<AuditAIReport, bool>> GetStatusExpression(AuditAIReportFilter filter)
        {
            return i => filter.Status.Select(option => (ReportAIStatus)option.Id).Contains(i.Status);
        }

        private Expression<Func<AuditAIReport, bool>> GetCreatedDateExpression(AuditAIReportFilter filter)
        {
            return FilterHelper.GetFilterExpression<AuditAIReport, DateTime>(
                nameof(AuditAIReport.CreatedAt),
                filter.CreatedAt.FirstCondition,
                filter.CreatedAt.SecondCondition,
                filter.CreatedAt.Operator);
        }

        private Expression<Func<AuditAIReport, bool>> GetSubmittedDateExpression(AuditAIReportFilter filter)
        {
            return FilterHelper.GetFilterExpression<AuditAIReport, DateTime>(
                nameof(AuditAIReport.SubmittedDate),
                filter.SubmittedDate.FirstCondition,
                filter.SubmittedDate.SecondCondition,
                filter.SubmittedDate.Operator);
        }

        public async Task<AuditAIReport> GetAuditAIReportAsync(int id)
        {
            return await _context.AuditAIReport
                        .Include(organization => organization.Organization)
                        .Include(facilities => facilities.Facility)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(reportAIContent => reportAIContent.Id == id);

            
        }

        public async Task UpdateAuditAIReportAsync(
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
                        int? state = null)
        {
            var parameters = new[]
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@OrganizationId", (object?)organizationId ?? DBNull.Value),
                new SqlParameter("@FacilityId", (object?)facilityId ?? DBNull.Value),
                new SqlParameter("@SummaryAI", (object?)summaryAI ?? DBNull.Value),
                new SqlParameter("@Keywords", (object?)keywords ?? DBNull.Value),
                new SqlParameter("@PdfFileName", (object?)pdfFileName ?? DBNull.Value),
                new SqlParameter("@AuditorName", (object?)auditorName ?? DBNull.Value),
                new SqlParameter("@AuditTime", (object?)auditTime ?? DBNull.Value),
                new SqlParameter("@AuditDate", (object?)auditDate ?? DBNull.Value),
                new SqlParameter("@FilteredDate", (object?)filteredDate ?? DBNull.Value),
                new SqlParameter("@CreatedAt", (object?)createdAt ?? DBNull.Value),
                new SqlParameter("@Status", (object?)status ?? DBNull.Value),
                new SqlParameter("@SubmittedDate", (object?)submittedDate ?? DBNull.Value),
                new SqlParameter("@SentForApprovalDate", (object?)sentForApprovalDate ?? DBNull.Value),
                new SqlParameter("@State", (object?)state ?? DBNull.Value)
            };

            await _context.Database.ExecuteSqlRawAsync("EXEC UpdateAuditAIReport @Id, @OrganizationId, @FacilityId, @SummaryAI, @Keywords, @PdfFileName, @AuditorName, @AuditTime, @AuditDate, @FilteredDate, @CreatedAt, @Status, @SubmittedDate, @SentForApprovalDate, @State", parameters);
        }
        public async Task<int> InsertAuditAIReportAsync(AuditAIReport report)
        {
            var insertedIdParam = new SqlParameter
            {
                ParameterName = "@InsertedId",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output // Mark as OUTPUT parameter
            };

            try
            {
                // Build the SQL parameters for the stored procedure
                var parameters = new[]
                {
                    new SqlParameter("@OrganizationId", report.OrganizationId),
                    new SqlParameter("@FacilityId", (object)report.FacilityId ?? DBNull.Value), // Handle nullable values
                    new SqlParameter("@SummaryAI", (object)report.SummaryAI ?? DBNull.Value), // Ensure NULL handling
                    new SqlParameter("@Keywords", (object)report.Keywords ?? DBNull.Value),
                    new SqlParameter("@PdfFileName", (object)report.PdfFileName ?? DBNull.Value),
                    new SqlParameter("@AuditorName", (object)report.AuditorName ?? DBNull.Value),
                    new SqlParameter("@AuditTime", (object)report.AuditTime ?? DBNull.Value),
                    new SqlParameter("@AuditDate", (object)report.AuditDate ?? DBNull.Value),
                    new SqlParameter("@FilteredDate", (object)report.FilteredDate ?? DBNull.Value),
                    new SqlParameter("@CreatedAt", report.CreatedAt),
                    new SqlParameter("@Status", (int)report.Status),
                    new SqlParameter("@SubmittedDate", (object)report.SubmittedDate ?? DBNull.Value),
                    new SqlParameter("@SentForApprovalDate", (object)report.SentForApprovalDate ?? DBNull.Value),
                    new SqlParameter("@State", (int)report.State),
                    insertedIdParam // Ensure OUTPUT parameter is included
                };

                // Execute the stored procedure
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC InsertAuditAIReport @OrganizationId, @FacilityId, @SummaryAI, @Keywords, @PdfFileName, " +
                    "@AuditorName, @AuditTime, @AuditDate, @FilteredDate, @CreatedAt, @Status, @SubmittedDate, " +
                    "@SentForApprovalDate, @State, @InsertedId OUTPUT",
                    parameters
                );
            }
            catch (SqlException s)
            {
                // Log the exception if needed
                var err = s.HResult;
                return 0; // Return 0 on failure
            }

            // Retrieve the inserted ID from the output parameter
            return (int)insertedIdParam.Value;
        }

        public async Task<AuditAIReport> GetAuditAIReportSelectedColumnsAsync(int id)
        {
            var reportAIContent = _context.AuditAIReport
                        .Include(organization => organization.Organization)
                        .Include(facilities => facilities.Facility)
                        .Select(r => new AuditAIReport
                        {
                            Id = r.Id,
                            OrganizationId = r.OrganizationId,
                            FacilityId = r.FacilityId,
                            PdfFileName = r.PdfFileName,
                           
                            AuditorName = r.AuditorName,
                            AuditTime = r.AuditTime,
                            AuditDate = r.AuditDate,
                            FilteredDate = r.FilteredDate,
                            CreatedAt = r.CreatedAt,
                            Status = r.Status,
                            SubmittedDate = r.SubmittedDate,
                            SentForApprovalDate = r.SentForApprovalDate,
                            State = r.State,
                            Organization = r.Organization,
                            Facility = r.Facility
                        })
                        .Single(reportAIContent => reportAIContent.Id == id);

            return reportAIContent;
        }

 
        // This is a placeholder for the decompression logic, assuming it's a GZIP compression.


        public string DecompressGzip(byte[] compressedData)
        {

            using (var msi = new MemoryStream(compressedData))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
            return null;
        }

        public async Task MigrateToSQLCompress()
        {
           
            var reports = await _context.AuditAIReport.ToListAsync();

            foreach (var report in reports)
            {
                // Step 1: Decompress existing GZIP data
                string serializedResult = DecompressGzip(report.SummaryAI);
                var decompressedData = Encoding.UTF8.GetBytes(serializedResult);
                // Step 2: Recompress using SQL Server's COMPRESS() (via stored procedure)
                await _context.Database.ExecuteSqlRawAsync(@"
            UPDATE AuditAIReport 
            SET SummaryAI = COMPRESS(@p0) 
            WHERE Id = @p1",
                    decompressedData, report.Id);
            }
        }
        private IQueryable<AuditAIReport> GetAuditAIReportQuery(int organizationId)
        {
            return _context.AuditAIReport
                .Include(facilities => facilities.Facility).Where(x => x.OrganizationId == organizationId);

        }
        public async Task<AuditAIReport[]> GetSubmittedAuditsPerDateAndOrganization(AuditorProductivitySummaryPerFacilityFilter filter)
        {
                var query = GetAuditAIReportQuery(filter.Organization.Id.GetValueOrDefault());

                query = query.Where(BuildSummaryPerFacilityFilterException(filter)).AsNoTracking();

                var audits = await query.ToListAsync();
                return audits.ToArray();

        }
        private Expression<Func<AuditAIReport, bool>> GetAuditDateExpression(DateFilterModel AuditDateFilter)
        {
                return FilterHelper.GetFilterExpression<AuditAIReport, DateTime>(
                    nameof(AuditAIReport.SubmittedDate),
                    AuditDateFilter.FirstCondition,
                    AuditDateFilter.SecondCondition,
                    AuditDateFilter.Operator);
        }

       private Expression<Func<AuditAIReport, bool>> GetFacilityExpression(IReadOnlyCollection<FilterOption> filterFacilities)
       {
                return i => filterFacilities.Select(option => option.Id).Contains(i.Facility.Id);
       }
       private Expression<Func<AuditAIReport, bool>> BuildSummaryPerFacilityFilterException(AuditorProductivitySummaryPerFacilityFilter filter)
       {
                Expression<Func<AuditAIReport, bool>> auditExpr = PredicateBuilder.True<AuditAIReport>();

                if (filter == null)
                {
                    return auditExpr;
                }
                auditExpr = auditExpr
                    .AndIf(GetFacilityExpression(filter.Facilities), filter.Facilities != null && filter.Facilities.Any());

                if (filter.DateProcessed != null)
                {
                    auditExpr = auditExpr.And(GetAuditDateExpression(filter.DateProcessed));
                }
                return auditExpr;
       }

    }
}
