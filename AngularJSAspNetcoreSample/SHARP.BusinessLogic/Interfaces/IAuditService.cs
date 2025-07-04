using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.BusinessLogic.DTO.Dashboard;
using SHARP.BusinessLogic.DTO.Facility;
using SHARP.BusinessLogic.DTO.Form;
using SHARP.BusinessLogic.DTO.ProgressNote;
using SHARP.Common.Enums;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Interfaces
{
    public interface IAuditService
    {
        Task<Tuple<IReadOnlyCollection<AuditDto>, int>> GetAuditsAsync(AuditFilter filter);

        Task<IReadOnlyCollection<FilterOption>> GetFilterColumnSourceDataAsync(AuditFilterColumnSource<AuditFilterColumn> columnData);

        Task<AuditDto> SetAuditStatusAsync(int id, AuditStatus status, string comment, int? reportType);

        Task<AuditDto> GetAuditAsync(int id);

        Task<AuditDto> GetAuditForDownloadAsync(int id);

        Task<List<ResidentDto>> GetResidents(int facilityId);

        Task<IReadOnlyCollection<OptionDto>> GetOrganizationOptionsAsync();

        Task<IReadOnlyCollection<AuditKPIDto>> GetAuditsKPIAsync(DashboardFilter filter);

        Task<IReadOnlyCollection<FormOptionDto>> GetFormOptionsAsync(int organizationId, string auditType);
        
        Task<AuditDto> AddAuditAsync(AuditAddEditDto auditEditDto, AuditStatus auditStatus = AuditStatus.InProgress);
        Task<AuditDto> EditAuditAsync(AuditAddEditDto auditEditDto);
        Task<bool> DeleteAuditAsync(int id);

        Task<IReadOnlyCollection<KeywordOptionDto>> GetKeywordOptionsAsync(int formVersionId);
        
        Task<int?> GetKeywordsTotalCountAsync(AuditProgressNoteFilter filter);

        Task<IReadOnlyCollection<ProgressNoteDto>> GetProgresNotesAsync(AuditProgressNoteFilter filter);

        Task<KeywordDto> AddAuditKeywordAsync(AddKeywordDto auditKeywordDto);

        Task DeleteAuditKeywordAsync(int auditValueId);

        Task<IReadOnlyCollection<KeywordDto>> GetAuditKeywordsAsync(AuditProgressNoteFilter filter);

        Task<IReadOnlyCollection<KeywordDto>> GetAuditKeywordsAsync(int auditId);

        Task<AuditDto> AddAuditTrackerAnswersAsync(int auditId, IReadOnlyCollection<AddEditTrackerAnswerDto> addAnswersDto, OptionDto highAlertCategory, string highAlertDescription, string highAlertNotes);

        Task<AuditDto> EditAuditTrackerAnswersAsync(int auditId, string groupId, IReadOnlyCollection<AddEditTrackerAnswerDto> answersDto,OptionDto highAlertCategory,string highAlertDescription,string highAlertNotes);

        Task DeleteAuditTrackerAnswersAsync(int auditId, string answersGroupId);

        Task<byte[]> GetAuditPdfAsync(int id, Dictionary<string, string> telemetryProperties = null);
        Task<byte[]> GetAuditPdfForHighAlertAsync(int id, Dictionary<string, string> telemetryProperties = null);
        Task<byte[]> GetCriteriaPdfAsync(PdfFilter filter, Dictionary<string, string> telemetryProperties = null);
        Task<byte[]> GetTrackerPdfAsync(PdfFilter filter, Dictionary<string, string> telemetryProperties = null);

        Task<FacilityOptionDto> GetFacilityAsync(int facilityId);

        Task<KeywordDto> EditAuditKeywordAsync(EditKeywordDto auditKeywordDto);

        Task<int> DuplicateAuditAsync(int auditId, DateTime? currentClientDate);

        Task<AuditDto> UpdateDuplicatedAuditAsync(AuditAddEditDto auditEditDto);

        Task<AuditsDueDateCountDto> GetAuditsDueDateCountAsync(DashboardFilter filter);

        bool TestGet();

        Task<AuditDto> SortAuditTrackerAnswersAsync(int auditId, SortModel sortModel);

        Task<bool> ChangeAuditState(int auditId, AuditState state);

        Task<List<Audit>> GetAuditsByUserTimeAndShift(int organizationId, int userId, DateTime startTime, DateTime endTime);
        Task<bool> IsAuditTriggedByKeyword(int id);
        Task<bool> CreateReportForPortal();

        Task<byte[]> GetAuditExcelAsync(int id, Dictionary<string, string> telemetryProperties = null);

    }
}
