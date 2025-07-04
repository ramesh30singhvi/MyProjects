using SHARP.BusinessLogic.DTO.Report;
using SHARP.BusinessLogic.ReportAI.Models;
using SHARP.Common.Enums;
using SHARP.Common.Filtration;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Interfaces
{
    public  interface IReportAIService
    {
        Task<AzureReportProcessResultDto> SendToAIAzureFunction(PdfReportUploadAzureDto uploadPdfAzureDto, Dictionary<string, string> telemetryProperties = null);
        Task<PdfReportAnalysisDto> ParsePdfAndBuildIndex(PdfReportUploadAzureDto reportUploadAzureDto, Dictionary<string, string> telemetryProperties = null);
        Task<int> ParsePdfByPatients(CreateAIReportDto reportUploadAzureDto);
        Task<Tuple<IReadOnlyCollection<AuditAIKeywordSummaryDto>,string>> SendToAnthropicAIService(PCCNotesDto notesDto);

        Task<IReadOnlyCollection<AuditAIReportV2Dto>> GetAIAuditV2ListAsync(AuditAIReportFilter filter);
        Task<AuditAIReportV2Dto> GetAIAuditAsync(int id);
        Task<AuditAIPatientPdfNotesDto> AddPatientInfoKeySummary(AuditAIPatientPdfNotesDto patientInfoDto);

        Task<AuditAIPatientPdfNotesDto> UpdatePatientInfoKeySummary(AuditAIPatientPdfNotesDto patientInfoDto);

        Task<AuditAIKeywordSummaryDto> AddAuditKeySummary(AuditAIKeywordSummaryDto keySummaryDto);

        Task<AuditAIKeywordSummaryDto> UpdateAuditKeySummary(AuditAIKeywordSummaryDto keySummaryDto);
        Task<ReportAIStatus> UpdateAIAuditV2StatusAsync(int aiAuditId, ReportAIStatus status);
        Task<bool> UpdateAIAuditV2State(int auditId, AIAuditState state);
        Task<AuditAIReportV2Dto> UpdateAuditReportV2(AuditAIReportV2Dto auditDto);
    }
}
