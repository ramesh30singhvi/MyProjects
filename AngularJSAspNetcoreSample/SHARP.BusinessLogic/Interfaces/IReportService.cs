using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.BusinessLogic.DTO.Facility;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.Common.Enums;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Models;

namespace SHARP.BusinessLogic.Interfaces
{
	public interface IReportService
	{
		Task<FallReportDto> GetFallReport(ReportFallFilterModel filter);
		Task<WoundReportDto> GetWoundReport(ReportFallFilterModel filter);
        Task<byte[]> GetDownloadFallReport(ReportFallFilterModel filter);
        Task<byte[]> GetDownloadWoundReport(ReportFallFilterModel filter);
        Task<byte[]> GetDownloadCriteriaReport(ReportCriteriaFilter filter);

        Task<byte[]> Create24KeywordReportByAI(AzureReportProcessResultDto resultAiDto);
        Task<IEnumerable<ReportAIContentDto>> GetReportAIContentAsync(AuditAIReportFilter filter);
        Task<IReadOnlyCollection<FilterOption>> GetFilterColumnSourceDataAsync(AuditAIReportFilterColumnSource<AuditAIReportFilterColumn> columnFilter);
        Task<ReportAIContentDto> GetReportAIContentAsync(int id);
        Task<byte[]> CreateUpdated24KeywordReport(ReportAIContentDto reportDto, int Id);
        Task<AuditAIReportNoSummaryDto> UpdateReportAIDataReportAsync(AzureReportProcessResultDto resultAiDto, int ReportAIId);
        Task<ReportAIStatus> SetAIAuditStatusAsync(int reportAIContentId, ReportAIStatus status);
        Task<ReportAIContentDto> SaveReportAIDataAsync(AzureReportProcessResultDto reportDto);
        Task<bool> UpdateAIAuditState(int reportAIContentId, AIAuditState state);
        Task MigrateGzipToSqlCompressAsync();
        Task<byte[]> Create24KeywordReportByAIV2(int id);

    }
}

