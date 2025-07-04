using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Presentation;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Splitting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.BusinessLogic.DTO.Dashboard;
using SHARP.BusinessLogic.DTO.Facility;
using SHARP.BusinessLogic.DTO.Form;
using SHARP.BusinessLogic.DTO.ProgressNote;
using SHARP.BusinessLogic.Extensions;
using SHARP.BusinessLogic.Helpers;
using SHARP.BusinessLogic.Interfaces;
using SHARP.BusinessLogic.PDF;
using SHARP.Common.Constants;
using SHARP.Common.Enums;
using SHARP.Common.Extensions;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.Common.Helpers;
using SHARP.Common.Models;
using SHARP.DAL;
using SHARP.DAL.Models;
using SHARP.DAL.Models.QueryModels;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using Text = iText.Layout.Element.Text;

namespace SHARP.BusinessLogic.Services
{
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        private readonly IFormService _formService;
        private readonly AppConfig _appConfig;
        private readonly string HighAlert = " HIGH ALERT ";

        public AuditService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IUserService userService,
            IFormService formService,
            AppConfig appConfig)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _formService = formService;

            _appConfig = appConfig;
        }

        public async Task<Tuple<IReadOnlyCollection<AuditDto>, int>> GetAuditsAsync(AuditFilter filter)
        {
            Expression<Func<Audit, object>> orderBySelector =
                OrderByHelper.GetOrderBySelector<AuditFilterColumn, Expression<Func<Audit, object>>>(filter.OrderBy, GetColumnOrderSelector);

            filter.UserOrganizationIds = await _userService.GetUserOrganizationIdsAsync();

            Tuple<Audit[], int> tuple = await _unitOfWork.AuditRepository.GetListAsync(
                filter,
                orderBySelector);

            return new Tuple<IReadOnlyCollection<AuditDto>, int>(_mapper.Map<IReadOnlyCollection<AuditDto>>(tuple.Item1), tuple.Item2);
        }

        public async Task<IReadOnlyCollection<FilterOption>> GetFilterColumnSourceDataAsync(AuditFilterColumnSource<AuditFilterColumn> columnData)
        {
            if (columnData.Column == AuditFilterColumn.Undefined)
            {
                throw new InvalidOperationException("Column is invalid");
            }

            columnData.UserOrganizationIds = await _userService.GetUserOrganizationIdsAsync();

            var columnSelector = GetColumnSelector(columnData.Column);

            var columnValues = await _unitOfWork.AuditRepository.GetDistinctColumnAsync(columnData, columnSelector);

            return _mapper.Map<IReadOnlyCollection<FilterOption>>(columnValues.OrderBy(i => i.Value));
        }

        public async Task<AuditDto> SetAuditStatusAsync(int id, AuditStatus status, string comment, int? reportType)
        {
            Audit audit = await _unitOfWork.AuditRepository.GetAuditWithTypeAsync(id);

            if(audit is null)
            {
                throw new NotFoundException();
            }

            if(audit.State != AuditState.Active)
            {
                throw new NotActiveAuditException();
            }

            var userId = _userService.GetLoggedUserId();

            CheckRightsForChangingAuditStatus(audit, status, userId);

            if(audit.FormVersion.Form.AuditType.Name == CommonConstants.CRITERIA && audit.IsFilled != true)
            {
                return await GetAuditAsync(audit.Id);
            }

            audit.Status = status;
            audit.Reason = comment;

            
            audit.AuditStatusHistory = new List<AuditStatusHistory>();
            audit.AuditStatusHistory.Add(CreateAuditStatusHistory(status, userId));

            await _unitOfWork.SaveChangesAsync();

            var auditDto =  await GetAuditAsync(audit.Id);
            try
            {
                if (audit.FormVersion.Form.AuditType.Name == CommonConstants.TWENTY_FOUR_HOUR_KEYWORD && auditDto.Status == AuditStatus.WaitingForApproval)
                    await CheckKeywordTriggerAsync(auditDto, userId);
                 
                if(audit.Status == AuditStatus.Submitted)
                {
                    await AddAuditReportToPortal(auditDto, userId, reportType);
                }
            }
            catch(Exception ex)
            {
               
            }


            return auditDto;
        }

        private async Task AddAuditReportToPortal(AuditDto auditDto, int userId, int? reportType)
        {

            var report = _unitOfWork.PortalReportRepository.GetAll().FirstOrDefault(r => r.AuditId == auditDto.Id);
            if (report != null)
            {
                return;
            }
            var portalReport = new PortalReport();
            portalReport.AuditId = auditDto.Id;
            portalReport.Name = $"{auditDto.Form.Name}"; 
            portalReport.ReportCategoryId = 1;
            var trackerAudit = auditDto as TrackerAuditDetailsDto;
            portalReport.ReportTypeId = (reportType ?? 1);
          //  portalReport.ReportRangeId = auditDto.Form.ReportRange == null ? 1 : auditDto.Form.ReportRange.Id;
            portalReport.CreatedAt = DateTime.UtcNow;
            portalReport.CreatedByUserID = userId;
            portalReport.StorageReportName = null;
            portalReport.StorageURL = null;
            portalReport.StorageContainerName = null;
            portalReport.OrganizationId = auditDto.Organization.Id;
            portalReport.FacilityId =  auditDto.Facility.Id;
            portalReport.AuditTypeId = auditDto.Form.AuditType?.Id ;
            await _unitOfWork.PortalReportRepository.AddAsync(portalReport);

            await _unitOfWork.SaveChangesAsync();

            auditDto.ReportTypeId = portalReport.ReportTypeId;

        }

        private async Task CheckKeywordTriggerAsync(AuditDto auditDto,int userId)
        {
            var hourKeywordAudioDto = _mapper.Map<HourKeywordAuditDetailsDto>(auditDto);

            if (hourKeywordAudioDto.MatchedKeywords.All(keyword => keyword.Keyword.Trigger == null || !keyword.Keyword.Trigger.Value))
                return;

            var auditTriggerBy = await _unitOfWork.AuditTriggeredRepository.GetAsync(auditDto.Id);
            foreach (var keyword in hourKeywordAudioDto.MatchedKeywords)
            {
                if (auditTriggerBy.Any(x => x.AuditTableColumnValueId == keyword.Id)) // already made audit for the keyword
                    continue;

                if (!keyword.Keyword.Trigger.Value)
                  continue;

                foreach(var formid in keyword.Keyword.FormsTriggeredByKeyword)
                {
                    //if (formid == 0)
                    //    continue;

                    var formVersions = await _formService.GetFormVersionsAsyncByFormId(formid.Id);
                    if(formVersions == null)
                        continue;

                    var formVersionDto = formVersions.FirstOrDefault(formVersion => formVersion.Status == FormVersionStatus.Published);

                    if(formVersionDto == null)
                        continue;

                    AuditAddEditDto newAudit = new AuditAddEditDto();
                    newAudit.FormVersionId = formVersionDto.Id;
                    newAudit.CurrentClientDate = auditDto.SubmittedDate;
                    newAudit.IncidentDateFrom =auditDto.IncidentDateFrom.Value;
                    if (auditDto.IncidentDateTo != null)
                        newAudit.IncidentDateTo = auditDto.IncidentDateTo.Value;
                    if(auditDto.Facility != null)
                        newAudit.FacilityId = auditDto.Facility.Id;
                    if(auditDto.LastDeletedDate != null)
                        newAudit.LastDeletedDate = auditDto.LastDeletedDate.Value;
                    newAudit.SubHeaderValues = new List<AuditAddEditSubHeaderValueDto>();
                    newAudit.Values = new List<AuditAddEditValueDto>();
                    newAudit.Resident = keyword.Resident;
                   
                    AuditDto auditCritetiaDto = await AddAuditAsync(newAudit, AuditStatus.Triggered);
                    
                    // add to table for log audit trig by audit
                    var auditCreatedBytrigger = new AuditTriggeredByKeyword(auditDto.Id, keyword.Id, auditCritetiaDto.Id);
                    await _unitOfWork.AuditTriggeredRepository.AddAsync(auditCreatedBytrigger);

                    await _unitOfWork.SaveChangesAsync();
                }

            }
  
           
        }
  

        public async Task<AuditDto> GetAuditForDownloadAsync(int id)
        {
            Audit audit = await _unitOfWork.AuditRepository.GetAuditForDownloadAsync(id);

            if (audit == null)
            {
                throw new NotFoundException("Audit is not found");
            }

            audit.FormVersion = await _formService.GetFormVersionAsync(audit.FormVersionId);
            audit.HighAlertAuditValues = await _unitOfWork.HighAlertAuditValueRepository.GetHighAlertAuditValueForAuditAsync(audit.Id);
            return await MapAuditDetailsAsync(audit);
        }
        public async Task<AuditDto> GetAuditAsync(int id)
        {
            Audit audit = await _unitOfWork.AuditRepository.GetAuditAsync(id);

            if (audit == null)
            {
                throw new NotFoundException("Audit is not found");
            }

            audit.FormVersion = await _formService.GetFormVersionAsync(audit.FormVersionId);

            return await MapAuditDetailsAsync(audit);
        }

        public async Task<List<ResidentDto>> GetResidents(int facilityId)
        {
            List<ResidentDto> residents = new List<ResidentDto>();

            var audits = await _unitOfWork.AuditRepository.GetAuditsOfFacility(facilityId);

            foreach(Audit audit in audits)
            {
                residents.Add(new ResidentDto(audit.ResidentName, audit.Room));
            }

            return residents.Distinct().ToList();
        }

        public async Task<IReadOnlyCollection<OptionDto>> GetOrganizationOptionsAsync()
        {
            ICollection<int> userOrganizationIds = await _userService.GetUserOrganizationIdsAsync();

            Expression<Func<Organization, bool>> predicate = null;

            if(userOrganizationIds.Any())
            {
                predicate = organization => userOrganizationIds.Contains(organization.Id);
            }

            IReadOnlyCollection<Organization> organizations = await _unitOfWork.OrganizationRepository.GetListAsync(
                predicate,
                organization => organization.Name,
                asNoTracking: true);

            return _mapper.Map<IReadOnlyCollection<OptionDto>>(organizations);
        }

        public async Task<FacilityOptionDto> GetFacilityAsync(int facilityId)
        {
            Facility facility = await _unitOfWork.FacilityRepository.FirstOrDefaultAsync(
                facility => facility.Id == facilityId,
                facility => facility.TimeZone);

            return _mapper.Map<FacilityOptionDto>(facility);
        }


        public async Task<IReadOnlyCollection<FormOptionDto>> GetFormOptionsAsync(int organizationId, string auditType)
        {
            IReadOnlyCollection<FormVersion> forms = await _unitOfWork.FormVersionRepository.GetFormVersionsByOrganizationAsync(organizationId, auditType);

            return _mapper.Map<IReadOnlyCollection<FormOptionDto>>(forms);
        }

        public async Task<AuditDto> AddAuditAsync(AuditAddEditDto auditEditDto,AuditStatus auditStatus)
        {
            var userId = _userService.GetLoggedUserId();

            Audit audit = _mapper.Map<Audit>(auditEditDto);

            audit.Status = auditStatus;
            audit.State = AuditState.Active;
            audit.SubmittedDate = DateTime.UtcNow;
            audit.SubmittedByUserId = userId;

            audit.DueDate = await CalculateDueDateAsync(audit, auditEditDto.CurrentClientDate);

            audit.AuditStatusHistory = new List<AuditStatusHistory>();
            audit.AuditStatusHistory.Add(CreateAuditStatusHistory(audit.Status, userId));

            await _unitOfWork.AuditRepository.AddAsync(audit);
            await _unitOfWork.SaveChangesAsync();

            return await GetAndVerifyAuditDetails(audit);
        }

        public async Task<AuditDto> EditAuditAsync(AuditAddEditDto auditEditDto)
        {
            Audit audit = await _unitOfWork.AuditRepository.GetAuditAsync(auditEditDto.Id.Value);

            _mapper.Map(auditEditDto, audit);

            await _unitOfWork.SaveChangesAsync();

            await CriteriaAuditHighAlert(audit,auditEditDto);

            return await GetAndVerifyAuditDetails(audit);
        }
        private async Task CriteriaAuditHighAlert(Audit audit, AuditAddEditDto auditEditDto)
        {
            audit.FormVersion = await _formService.GetFormVersionAsync(audit.FormVersionId);
            if (audit.FormVersion.Form.AuditType.Name == CommonConstants.CRITERIA)
            {
                if (auditEditDto?.HighAlertCategory != null && auditEditDto?.HighAlertCategory.Id != null)
                {
                    if (!audit.HighAlertAuditValues.Any())
                        await AddHighAlert(0, audit.Id, audit.FormVersion?.Form?.Name, auditEditDto.HighAlertCategory.Id, auditEditDto.HighAlertDescription, auditEditDto.HighAlertNotes);
                    else
                        await UpdateHighAlert(audit.HighAlertAuditValues.FirstOrDefault().Id, auditEditDto.HighAlertCategory.Id, auditEditDto.HighAlertDescription, auditEditDto.HighAlertNotes);

                    audit = await _unitOfWork.AuditRepository.GetAuditAsync(auditEditDto.Id.Value);
                }
                else
                {
                    if (audit.HighAlertAuditValues.Any())
                        await DeleteHighAlert(audit.HighAlertAuditValues.FirstOrDefault().Id);
                }
            }
        }
        private async Task<HighAlertValueDto> UpdateHighAlert( int highAlertId, int highAlertCategoryID, string highAlertDescription, string highAlertNotes)
        {
            var highAlert = await _unitOfWork.HighAlertAuditValueRepository.GetHighAlertAuditValueAsync(highAlertId);

            if (highAlert == null)
                return null;

            if (highAlert.HighAlertCategoryId == highAlertCategoryID && highAlert.HighAlertDescription == highAlertDescription
                && highAlert.HighAlertNotes == highAlertNotes)
                return _mapper.Map<HighAlertValueDto>(highAlert);

            highAlert.HighAlertCategoryId = highAlertCategoryID;
            highAlert.HighAlertDescription = highAlertDescription;
            highAlert.HighAlertNotes = highAlertNotes;
            _unitOfWork.HighAlertAuditValueRepository.Update(highAlert);
            await _unitOfWork.SaveChangesAsync();

            highAlert = await _unitOfWork.HighAlertAuditValueRepository.GetHighAlertAuditValueAsync(highAlertId);
            return _mapper.Map<HighAlertValueDto>(highAlert);
        }

        //For AutoTests
        public async Task<bool> DeleteAuditAsync(int id)
        {
            Audit audit = await _unitOfWork.AuditRepository.GetAsync(id);

            if(audit == null)
            {
                throw new NotFoundException($"Audit with Id: {id} is not found");
            }

            _unitOfWork.AuditRepository.Remove(audit);

            await _unitOfWork.SaveChangesAsync();         

            return true;
        }

        public async Task<bool> ChangeAuditState(int auditId, AuditState state)
        {
            Audit audit = await _unitOfWork.AuditRepository.GetAsync(auditId);
            if (audit == null)
            {
                throw new NotFoundException($"Audit with Id: {auditId} is not found");
            }
            if (state == AuditState.Active && audit.State == AuditState.Archived)
            {
                audit.LastUnarchivedDate = DateTime.UtcNow;
            }
            if (state == AuditState.Deleted)
            {
                var userId = _userService.GetLoggedUserId();

                audit.LastDeletedDate = DateTime.UtcNow;
                audit.DeletedByUserId = userId;
            }           

            audit.State = state;

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        public async Task<int> DuplicateAuditAsync(int auditId, DateTime? currentClientDate)
        {
            var userId = _userService.GetLoggedUserId();

            Audit audit = await _unitOfWork.AuditRepository.GetAuditForDuplicateAsync(auditId);

            if (audit.State != AuditState.Active)
            {
                throw new NotActiveAuditException();
            }

            Audit newAudit = audit.Clone(userId);

            newAudit.DueDate = await CalculateDueDateAsync(audit, currentClientDate.HasValue ? currentClientDate.Value : DateTime.UtcNow.Date);

            Dictionary<string, string> groups = new Dictionary<string, string>();

            foreach (var value in audit.Values)
            {
                string groupId = null;

                if (!string.IsNullOrEmpty(value.GroupId))
                {
                    string existedGroupId = groups.GetValueOrDefault(value.GroupId);

                    if (!string.IsNullOrEmpty(existedGroupId))
                    {
                        groupId = existedGroupId;
                    }
                    else
                    {
                        string newGroupId = Guid.NewGuid().ToString();

                        groupId = newGroupId;

                        groups.Add(value.GroupId, groupId);
                    }                    
                }                

                AuditTableColumnValue columnValue = value.Clone(newAudit, groupId);

                newAudit.Values.Add(columnValue);
            }

            foreach (var value in audit.AuditFieldValues)
            {
                AuditFieldValue fieldValue = value.Clone(newAudit);

                newAudit.AuditFieldValues.Add(fieldValue);
            }

            foreach (var setting in audit.Settings)
            {
                AuditSetting ausitSetting = setting.Clone(newAudit);

                newAudit.Settings.Add(ausitSetting);
            }

            newAudit.AuditStatusHistory = new List<AuditStatusHistory>();
            newAudit.AuditStatusHistory.Add(CreateAuditStatusHistory(newAudit.Status, userId));

            await _unitOfWork.AuditRepository.AddAsync(newAudit);

            await _unitOfWork.SaveChangesAsync();

            return newAudit.Id;
        }

        public async Task<AuditDto> UpdateDuplicatedAuditAsync(AuditAddEditDto auditEditDto)
        {
            var userId = _userService.GetLoggedUserId();

            Audit audit = await _unitOfWork.AuditRepository.GetAsync(auditEditDto.Id);
            if (audit.State != AuditState.Active)
            {
                throw new NotActiveAuditException();
            }

            _mapper.Map(auditEditDto, audit);

            audit.Status = AuditStatus.InProgress;

            audit.AuditStatusHistory = new List<AuditStatusHistory>();
            audit.AuditStatusHistory.Add(CreateAuditStatusHistory(audit.Status, userId));

            await _unitOfWork.SaveChangesAsync();

            return await GetAndVerifyAuditDetails(audit);
        }

        private AuditStatusHistory CreateAuditStatusHistory(AuditStatus status, int userId)
        {
            AuditStatusHistory statusHistory = new AuditStatusHistory();
            statusHistory.Status = status;
            statusHistory.Date = DateTime.UtcNow;
            statusHistory.UserId = userId;

            return statusHistory;
        }

        public async Task<IReadOnlyCollection<KeywordOptionDto>> GetKeywordOptionsAsync(int formVersionId)
        {
            IReadOnlyCollection<TableColumn> keywords = await _unitOfWork.TableColumnRepository.GetListAsync(
                keyword => keyword.FormVersionId == formVersionId,
                keyword => keyword.Sequence,
                asNoTracking: true,
                include: keyword => keyword.KeywordTrigger);
            keywords = keywords.OrderBy(x => x.Name).ToList();
            return _mapper.Map<IReadOnlyCollection<KeywordOptionDto>>(keywords);
        }

        public async Task<int?> GetKeywordsTotalCountAsync(AuditProgressNoteFilter filter)
        {
            int result = 
                await _unitOfWork.ProgressNoteRepository.GetCountKeywordsMatchingAsync(filter);

            return result;
        }

        public async Task<IReadOnlyCollection<KeywordDto>> GetAuditKeywordsAsync(AuditProgressNoteFilter filter)
        {
            IReadOnlyCollection<AuditTableColumnValue> auditKeywords = await _unitOfWork.AuditTableColumnValueRepository.GetListAsync(
                auditValue => auditValue.AuditId == filter.AuditId && auditValue.TableColumnId == filter.Keyword.Id,
                auditValue => auditValue.Id,
                ascending: false,
                asNoTracking: true);

            return _mapper.Map<IReadOnlyCollection<KeywordDto>>(auditKeywords);
        }

        public async Task<IReadOnlyCollection<KeywordDto>> GetAuditKeywordsAsync(int auditId)
        {
            IReadOnlyCollection<AuditTableColumnValue> auditKeywords = await _unitOfWork.AuditTableColumnValueRepository.GetListAsync(
                auditValue => auditValue.AuditId == auditId,
                auditValue => auditValue.Id,
                ascending: false,
                asNoTracking: true,
                include: auditValue => auditValue.Column);

            return _mapper.Map<IReadOnlyCollection<KeywordDto>>(auditKeywords);
        }

        public async Task<IReadOnlyCollection<ProgressNoteDto>> GetProgresNotesAsync(AuditProgressNoteFilter filter)
        {
            IReadOnlyCollection<ProgressNote> progressNotes = 
                await _unitOfWork.ProgressNoteRepository.GetProgresNotesAsync(filter);

            return _mapper.Map<IReadOnlyCollection<ProgressNoteDto>>(progressNotes);
        }
       
        public async Task<KeywordDto> AddAuditKeywordAsync(AddKeywordDto auditKeywordDto)
        {
            AuditTableColumnValue auditValue = _mapper.Map<AuditTableColumnValue>(auditKeywordDto);


            await _unitOfWork.AuditTableColumnValueRepository.AddAsync(auditValue);
            await _unitOfWork.SaveChangesAsync();


            HighAlertValueDto highAlertDto = null;
            if (auditKeywordDto.HighAlertCategory?.Id != null)
            {
               highAlertDto = await AddHighAlert(auditValue.Id, auditValue.AuditId, auditValue.Audit?.FormVersion?.Form?.Name, auditKeywordDto.HighAlertCategory.Id, auditKeywordDto.HighAlertDescription, auditKeywordDto.HighAlertNotes);
               auditValue.HighAlertID = highAlertDto.Id;

                _unitOfWork.AuditTableColumnValueRepository.Update(auditValue);
                await _unitOfWork.SaveChangesAsync();
            }

            var keywaordDto =  _mapper.Map<KeywordDto>(auditValue);
            if (highAlertDto != null)
                keywaordDto.HighAlertAuditValue = highAlertDto;

            return keywaordDto;
        }

        private async Task<HighAlertValueDto> AddHighAlert(int auditTableColumnValueID,int auditID,string reportName,int highAlertCategory,string highAlertDesc,string highAlertNotes)
        {
            var highAlert = new HighAlertAuditValue();
            highAlert.HighAlertNotes = highAlertNotes;
            highAlert.HighAlertDescription = highAlertDesc;
            highAlert.HighAlertCategoryId = highAlertCategory;
            if (string.IsNullOrEmpty(reportName))
            {
                Audit audit = await _unitOfWork.AuditRepository.GetAuditAsync(auditID);
                highAlert.ReportName = audit?.FormVersion.Form.Name;
            }
            else
            {
                highAlert.ReportName = reportName;
            }
            highAlert.AuditId = auditID;
            highAlert.AuditTableColumnValueId = auditTableColumnValueID;
            var user = await _userService.GetLoggedInUserAsync();
            highAlert.CreatedByUserId = user.Id;
            highAlert.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.HighAlertAuditValueRepository.AddAsync(highAlert);
            await _unitOfWork.SaveChangesAsync();

            var highAlertStatus = new HighAlertStatusHistory();
            highAlertStatus.HighAlertStatusId = 1; // Open
            highAlertStatus.HighAlertAuditValueId = highAlert.Id;
            highAlertStatus.CreatedAt = highAlert.CreatedAt;
            highAlertStatus.ChangedBy = string.Empty;
            highAlertStatus.UserNotes = string.Empty;

            await _unitOfWork.HighAlertStatusHistoryRepository.AddAsync(highAlertStatus);
            await _unitOfWork.SaveChangesAsync();

            var savedAlert = await _unitOfWork.HighAlertAuditValueRepository.GetHighAlertAuditValueAsync(highAlert.Id);

            return _mapper.Map<HighAlertValueDto>(savedAlert);
        }

        public async Task<KeywordDto> EditAuditKeywordAsync(EditKeywordDto auditKeywordDto)
        {
            AuditTableColumnValue auditValue = await _unitOfWork.AuditTableColumnValueRepository.GetAsync<int>(auditKeywordDto.Id);

            if(auditValue is null)
            {
                throw new NotFoundException();
            }

            var updateAuditTriggered = auditKeywordDto.Resident != auditValue.Resident;

            _mapper.Map(auditKeywordDto, auditValue);
            HighAlertValueDto highAlertDto = null;
            if (auditKeywordDto.HighAlertCategory == null)
            {

                await DeleteHighAlert(auditValue.HighAlertID);
                auditValue.HighAlertID = 0;
            }
            else if (auditKeywordDto.HighAlertCategory?.Id > 0 )
            {
                if(auditValue.HighAlertID > 0)
                {
                    highAlertDto = await UpdateHighAlert( auditValue.HighAlertID, auditKeywordDto.HighAlertCategory.Id, auditKeywordDto.HighAlertDescription, auditKeywordDto.HighAlertNotes);
                }
                else
                {
                    highAlertDto = await AddHighAlert(auditValue.Id, auditValue.AuditId,auditValue.Audit?.FormVersion?.Form?.Name, auditKeywordDto.HighAlertCategory.Id, auditKeywordDto.HighAlertDescription, auditKeywordDto.HighAlertNotes);

                    auditValue.HighAlertID = highAlertDto.Id;
                }

               
            }

            _unitOfWork.AuditTableColumnValueRepository.Update(auditValue);
            await _unitOfWork.SaveChangesAsync();

            if (updateAuditTriggered)
                await UpdateAuditTriggeredByKeyword(auditKeywordDto);

            auditValue = await _unitOfWork.AuditTableColumnValueRepository.GetAsync<int>(auditKeywordDto.Id);
            var keywordDto =  _mapper.Map<KeywordDto>(auditValue);

            if (auditValue.HighAlertID > 0)
            {
                var highAlertValue = await _unitOfWork.HighAlertAuditValueRepository.GetHighAlertAuditValueAsync(auditValue.HighAlertID);
                keywordDto.HighAlertAuditValue = _mapper.Map<HighAlertValueDto>(highAlertValue);
            }

            return keywordDto;
        }

        private async Task DeleteHighAlert(int highAlertID)
        {
            var highAlert = await _unitOfWork.HighAlertAuditValueRepository.GetHighAlertAuditValueAsync(highAlertID);
            if (highAlert == null)
                return;
            if (highAlert.HighAlertStatusHistory.Any())
            {
                _unitOfWork.HighAlertStatusHistoryRepository.RemoveRange(highAlert.HighAlertStatusHistory);
                await _unitOfWork.SaveChangesAsync();
            }
            _unitOfWork.HighAlertAuditValueRepository.Remove(highAlertID);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAuditKeywordAsync(int auditValueId)
        {
            await RemoveAuditTriggeredByKeyword(auditValueId);

            _unitOfWork.HighAlertAuditValueRepository.RemoveHighAlertByAuditValueId(auditValueId);

            _unitOfWork.AuditTableColumnValueRepository.Remove(auditValueId);

          

            await _unitOfWork.SaveChangesAsync();

            
        }
        private async Task UpdateAuditTriggeredByKeyword(EditKeywordDto auditKeywordDto)
        {
            try
            {
                var auditTriggerred = await _unitOfWork.AuditTriggeredRepository.GetAsync(auditKeywordDto.AuditId);
                if (auditTriggerred != null && auditTriggerred.Any())
                {
                    foreach (var audit in auditTriggerred)
                    {
                        var createdAudit = await _unitOfWork.AuditRepository.GetAuditAsync(audit.CreatedAuditId);
                        if (createdAudit != null)
                        {
                            createdAudit.ResidentName = auditKeywordDto.Resident;
                        }
                    }
                    await _unitOfWork.SaveChangesAsync();
                }
            }catch(Exception ex)
            {

            }
        }

        private async Task RemoveAuditTriggeredByKeyword(int auditValueId)
        {
            try
            {
                var auditTriggerred = await _unitOfWork.AuditTriggeredRepository.GetByAuditTableColumnValue(auditValueId);
                if (auditTriggerred != null && auditTriggerred.Any())
                {
                    foreach (var audit in auditTriggerred)
                    {
                        Expression<Func<Audit, bool>> predicate = i =>
                                     i.Id == audit.CreatedAuditId;
                        var existed = await _unitOfWork.AuditRepository.ExistsAsync(predicate);
                        if (existed)
                        {
                            _unitOfWork.AuditTriggeredRepository.Remove(audit.Id);
                            await _unitOfWork.SaveChangesAsync();

                            _unitOfWork.AuditRepository.Remove(audit.CreatedAuditId);
                            await _unitOfWork.SaveChangesAsync();
                        }

                    }
                }
            }
            catch(Exception ex)
            {

            }
        }
        public async Task<AuditDto> AddAuditTrackerAnswersAsync(int auditId, IReadOnlyCollection<AddEditTrackerAnswerDto> addAnswersDto, OptionDto highAlertCategory, string highAlertDescription, string highAlertNotes)
        {
            var auditTableColumnValues = _mapper.Map<IReadOnlyCollection<AuditTableColumnValue>>(addAnswersDto);

            _unitOfWork.AuditTableColumnValueRepository.AddRange(auditTableColumnValues);
            await _unitOfWork.SaveChangesAsync();
            if (highAlertCategory != null)
            {
                var auditTableColumnValue = auditTableColumnValues.First();
                var highAlertDto = await AddHighAlert(auditTableColumnValue.Id, auditId, auditTableColumnValue.Audit?.FormVersion?.Form?.Name ,highAlertCategory.Id, highAlertDescription, highAlertNotes);

                auditTableColumnValue.HighAlertID = highAlertDto.Id;
                _unitOfWork.AuditTableColumnValueRepository.Update(auditTableColumnValue);
                await _unitOfWork.SaveChangesAsync();
            }
           

            return await GetAuditAsync(auditId);
        }

        private async Task EditTrackerHighAlert(IReadOnlyCollection<AuditTableColumnValue> values, OptionDto highAlertCategory, string highAlertDescription, string highAlertNotes)
        {
            var auditTableColumnValueWithHighAlert = values.FirstOrDefault(x => x.HighAlertID != 0);
            if (highAlertCategory == null)
            {
                if (auditTableColumnValueWithHighAlert != null)
                {
                    await DeleteHighAlert(auditTableColumnValueWithHighAlert.HighAlertID);

                    auditTableColumnValueWithHighAlert.HighAlertID = 0;
                    _unitOfWork.AuditTableColumnValueRepository.Update(auditTableColumnValueWithHighAlert);
                    await _unitOfWork.SaveChangesAsync();
                }

            }
            else
            {
                if (auditTableColumnValueWithHighAlert == null)
                {
                    var auditTableColumnValue = values.FirstOrDefault();
                    var highAlerDto = await AddHighAlert(auditTableColumnValue.Id, auditTableColumnValue.AuditId, auditTableColumnValue.Audit?.FormVersion?.Form?.Name, highAlertCategory.Id, highAlertDescription, highAlertNotes);
                    auditTableColumnValue.HighAlertID = highAlerDto.Id;
                    _unitOfWork.AuditTableColumnValueRepository.Update(auditTableColumnValue);
                    await _unitOfWork.SaveChangesAsync();
                }
                else
                {
                    await UpdateHighAlert(auditTableColumnValueWithHighAlert.HighAlertID, highAlertCategory.Id, highAlertDescription, highAlertNotes);
                }
            }
        }
        public async Task<AuditDto> EditAuditTrackerAnswersAsync(int auditId, string groupId, IReadOnlyCollection<AddEditTrackerAnswerDto> answersDto, OptionDto highAlertCategory, string highAlertDescription, string highAlertNotes)
        {
            IReadOnlyCollection<AuditTableColumnValue> values = await _unitOfWork.AuditTableColumnValueRepository.GetListAsync(
                value => value.AuditId == auditId && value.GroupId == groupId);

            foreach (var value in values)
            {
                _mapper.Map(answersDto.FirstOrDefault(answer => answer.Id == value.Id), value);
            }

            _unitOfWork.AuditTableColumnValueRepository.UpdateRange(values);
            
            await _unitOfWork.SaveChangesAsync();


            await EditTrackerHighAlert(values, highAlertCategory, highAlertDescription, highAlertNotes);

            return await GetAuditAsync(auditId);
        }

        public async Task DeleteAuditTrackerAnswersAsync(int auditId, string answersGroupId)
        {
            IReadOnlyCollection<AuditTableColumnValue> answwers = await _unitOfWork.AuditTableColumnValueRepository.GetListAsync(
                answer => answer.AuditId == auditId && answer.GroupId == answersGroupId);

            _unitOfWork.AuditTableColumnValueRepository.RemoveRange(answwers);

            var auditTableColumnValueWithHighAlert = answwers.FirstOrDefault(x => x.HighAlertID != 0);
            if(auditTableColumnValueWithHighAlert != null)
                _unitOfWork.HighAlertAuditValueRepository.RemoveHighAlertByAuditValueId(auditTableColumnValueWithHighAlert.Id);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IReadOnlyCollection<AuditKPIDto>> GetAuditsKPIAsync(DashboardFilter filter)
        {
            ICollection<int> userOrganizationIds = await _userService.GetUserOrganizationIdsAsync();

            if(userOrganizationIds.Any() && !filter.Organizations.Any())
            {
                filter.Organizations = userOrganizationIds.ToList();
            }            

            IReadOnlyCollection<AuditKPIQueryModel> auditKPIs = await _unitOfWork.AuditRepository.GetAuditKPIsAsync(filter);

            return _mapper.Map< IReadOnlyCollection<AuditKPIDto>>(auditKPIs);
        }

        public async Task<AuditsDueDateCountDto> GetAuditsDueDateCountAsync(DashboardFilter filter)
        {
            ICollection<int> userOrganizationIds = await _userService.GetUserOrganizationIdsAsync();

            if (userOrganizationIds.Any() && !filter.Organizations.Any())
            {
                filter.Organizations = userOrganizationIds.ToList();
            }

            return new AuditsDueDateCountDto() 
            { 
                Today = await _unitOfWork.AuditRepository.GetAuditDueDateCountAsync(filter, audit => audit.DueDate >= filter.CurrentClientDate && audit.DueDate < filter.CurrentClientDate.AddDays(1)),
                Later = await _unitOfWork.AuditRepository.GetAuditDueDateCountAsync(filter, audit => audit.DueDate >= filter.CurrentClientDate.AddDays(1))
            };
        }

        public async Task<AuditDto> SortAuditTrackerAnswersAsync(int auditId, SortModel sortModel)
        {
            var trackerOrderSetting = await _unitOfWork.AuditSettingRepository.FirstOrDefaultAsync(setting => 
                setting.AuditId == auditId && setting.Type == AuditSettingType.TrackerOrder);

            if (trackerOrderSetting != null)
            {
                trackerOrderSetting.Value = FormatOrderValueSetting(sortModel);
            }
            else
            {
                trackerOrderSetting = new AuditSetting()
                {
                    AuditId = auditId,
                    Type = AuditSettingType.TrackerOrder,
                    Value = FormatOrderValueSetting(sortModel)
                };

                _unitOfWork.AuditSettingRepository.Add(trackerOrderSetting);
            }

            await _unitOfWork.SaveChangesAsync();

            return await GetAuditAsync(auditId);
        }

        public bool TestGet()
        {
            return true;
        }

        private string FormatOrderValueSetting(SortModel sortModel)
        {
            return !string.IsNullOrEmpty(sortModel.OrderBy) && !string.IsNullOrEmpty(sortModel.SortOrder)
                ? $"{sortModel.OrderBy}{CommonConstants.SLASH}{sortModel.SortOrder}"
                : null;
        }

        private async Task<AuditDto> MapAuditDetailsAsync(Audit audit)
        {
            if (audit == null)
            {
                return null;
            }

            switch (audit.FormVersion.Form.AuditType.Name)
            {
                case CommonConstants.TWENTY_FOUR_HOUR_KEYWORD:
                    var hourKeywordAuditDto = _mapper.Map<HourKeywordAuditDetailsDto>(audit);
                    hourKeywordAuditDto.Keywords = await GetKeywordOptionsAsync(audit.FormVersionId);

                    hourKeywordAuditDto.IsReadyForNextStatus = true;

                    return hourKeywordAuditDto;

                case CommonConstants.CRITERIA:
                    var criteriaAuditDto = _mapper.Map<CriteriaAuditDetailsDto>(audit);

                    criteriaAuditDto.IsReadyForNextStatus = MapCriteriaAuditAnswersToQuestionsAndCheckIsFilled(criteriaAuditDto);

                    return criteriaAuditDto;

                case CommonConstants.TRACKER:
                    var trackerAuditDto = _mapper.Map<TrackerAuditDetailsDto>(audit);

                    trackerAuditDto.IsReadyForNextStatus = true;

                    return trackerAuditDto;
                
                case CommonConstants.MDS:
                    var mdsAuditDto = _mapper.Map<MdsAuditDetailsDto>(audit);

                    mdsAuditDto.IsReadyForNextStatus = MapMdsAuditAnswersToQuestionsAndCheckIsFilled(mdsAuditDto);

                    return mdsAuditDto;

                default:

                    return _mapper.Map<AuditDto>(audit);
            }
        }

        private async Task<TrackerAnswerGroupDto> GroupTrackerAnswersAsync(int auditId, string answersGroupId)
        {
            IReadOnlyCollection<AuditTableColumnValue> values = await _unitOfWork.AuditTableColumnValueRepository.GetTrakerGroupValues(auditId, answersGroupId);
            return new TrackerAnswerGroupDto()
            {
                GroupId = answersGroupId,
                Answers = _mapper.Map<IReadOnlyCollection<TrackerAnswerDto>>(values)
            };
        }
        public async Task<byte[]> GetAuditPdfForHighAlertAsync(int id, Dictionary<string, string> telemetryProperties = null)
        {
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            var pdfDocument = new PdfDocument(writer);

            AuditHeaderEventHandler header = null;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            telemetryProperties?.Add("2. Start Get Audit", $"Time: {DateTime.UtcNow}.");

            AuditDto auditDto = await GetAuditForDownloadAsync(id);

            stopWatch.Stop();
            telemetryProperties?.Add("3. End Get Audit", $"Time: {DateTime.UtcNow}. Audit Type: {auditDto.Form.AuditType.Name}. Duration: {stopWatch.Elapsed}.");

            stopWatch.Restart();
            telemetryProperties?.Add("4. Start Generate Pdf", $"Time: {DateTime.UtcNow}.");

            if (auditDto.Form.AuditType.Name == CommonConstants.TWENTY_FOUR_HOUR_KEYWORD)
            {
                using Document document = new Document(pdfDocument, PageSize.A4, true);

                header = new AuditHeaderEventHandler(document, new List<AuditDto>() { auditDto }, "24 Hr Logged Keyword Report");

                AddRepeatedElements(pdfDocument, header);
                SetMargins(document, header);

                ProcessKeywordsDocument(document, (HourKeywordAuditDetailsDto)auditDto);

                document.Close();
            }
            else if (auditDto.Form.AuditType.Name == CommonConstants.CRITERIA)
            {
                using Document document = new Document(pdfDocument, PageSize.A4, true);

                header = new CriteriaAuditHeaderEventHandler(document, new List<AuditDto>() { auditDto }, auditDto.Form.Name);

                AddRepeatedElements(pdfDocument, header);
                //SetMargins(document, header);

                var sideMargin = 20;
                var topMargin = 20 + header.Height;
                var bottomMargin = 60;
                document.SetMargins(topMargin, sideMargin, bottomMargin, sideMargin);

                ProcessCriteriaDocumentAsync(document, (CriteriaAuditDetailsDto)auditDto);

                document.Close();
            }
            else if (auditDto.Form.AuditType.Name == CommonConstants.MDS)
            {
                using Document document = new Document(pdfDocument, PageSize.A4, true);

                header = new CriteriaAuditHeaderEventHandler(document, new List<AuditDto>() { auditDto }, auditDto.Form.Name);

                AddRepeatedElements(pdfDocument, header);

                var sideMargin = 20;
                var topMargin = 20 + header.Height;
                var bottomMargin = 60;
                document.SetMargins(topMargin, sideMargin, bottomMargin, sideMargin);

                ProcessMdsDocumentAsync(document, (MdsAuditDetailsDto)auditDto);

                document.Close();
            }
            else if (auditDto.Form.AuditType.Name == CommonConstants.TRACKER)
            {
                var document = GetTrackerPDFDocument(pdfDocument, new List<TrackerAuditDetailsDto>() { (TrackerAuditDetailsDto)auditDto });

                document.Close();
            }

            stopWatch.Stop();
            telemetryProperties?.Add("5. End Generate Pdf", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}.");

            return stream.ToArray();
        }
        public async Task<byte[]> GetAuditPdfAsync(int id, Dictionary<string, string> telemetryProperties = null)
        {
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            var pdfDocument = new PdfDocument(writer);

            AuditHeaderEventHandler header = null;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            telemetryProperties?.Add("2. Start Get Audit", $"Time: {DateTime.UtcNow}.");

            AuditDto auditDto = await GetAuditAsync(id);

            stopWatch.Stop();
            telemetryProperties?.Add("3. End Get Audit", $"Time: {DateTime.UtcNow}. Audit Type: {auditDto.Form.AuditType.Name}. Duration: {stopWatch.Elapsed}.");

            stopWatch.Restart();
            telemetryProperties?.Add("4. Start Generate Pdf", $"Time: {DateTime.UtcNow}.");

            if (auditDto.Form.AuditType.Name == CommonConstants.TWENTY_FOUR_HOUR_KEYWORD)
            {
                using Document document = new Document(pdfDocument, PageSize.A4, true);

                header = new AuditHeaderEventHandler(document, new List<AuditDto>() { auditDto }, "24 Hr Logged Keyword Report");

                AddRepeatedElements(pdfDocument, header);
                SetMargins(document, header);

                ProcessKeywordsDocument(document, (HourKeywordAuditDetailsDto)auditDto);

                document.Close();
            }
            else if (auditDto.Form.AuditType.Name == CommonConstants.CRITERIA)
            {
                using Document document = new Document(pdfDocument, PageSize.A4, true);

                header = new CriteriaAuditHeaderEventHandler(document, new List<AuditDto>() { auditDto }, auditDto.Form.Name);

                AddRepeatedElements(pdfDocument, header);
                //SetMargins(document, header);
                
                var sideMargin = 20;
                var topMargin = 20 + header.Height;
                var bottomMargin = 60;
                document.SetMargins(topMargin, sideMargin, bottomMargin, sideMargin);

                ProcessCriteriaDocumentAsync(document, (CriteriaAuditDetailsDto)auditDto);

                document.Close();
            }
            else if (auditDto.Form.AuditType.Name == CommonConstants.MDS)
            {
                using Document document = new Document(pdfDocument, PageSize.A4, true);

                header = new CriteriaAuditHeaderEventHandler(document, new List<AuditDto>() { auditDto }, auditDto.Form.Name);

                AddRepeatedElements(pdfDocument, header);

                var sideMargin = 20;
                var topMargin = 20 + header.Height;
                var bottomMargin = 60;
                document.SetMargins(topMargin, sideMargin, bottomMargin, sideMargin);
                
                ProcessMdsDocumentAsync(document, (MdsAuditDetailsDto)auditDto);

                document.Close();
            }
            else if (auditDto.Form.AuditType.Name == CommonConstants.TRACKER)
            {
                var document = GetTrackerPDFDocument(pdfDocument, new List<TrackerAuditDetailsDto>() { (TrackerAuditDetailsDto)auditDto });

                document.Close();
            }        

            stopWatch.Stop();
            telemetryProperties?.Add("5. End Generate Pdf", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}.");

            return stream.ToArray();
        }

        public async Task<byte[]> GetCriteriaPdfAsync(PdfFilter filter, Dictionary<string, string> telemetryProperties = null)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            telemetryProperties?.Add("2. Start Get Audits", $"Time: {DateTime.UtcNow}.");

            IReadOnlyCollection<Audit> audits = await _unitOfWork.AuditRepository.GetSubmittedAuditsAsync(filter);
            
            Console.WriteLine($"Audits Count ${audits.Count}");

            var auditIds = audits.Select(audit => audit.Id).ToArray();
            var formVersionsIds = audits.Select(audit => audit.FormVersionId).Distinct().ToArray();

            var questions = await _unitOfWork.TableColumnRepository.GetFormCriteriaQuestionsAsync(formVersionsIds);
            var fields = await _unitOfWork.FormFieldRepository.GetFormFieldsAsync(formVersionsIds);

            var questionAnswers = await _unitOfWork.AuditTableColumnValueRepository.GetListAsync(
                answer => auditIds.Contains(answer.AuditId));

            var fieldAnswers = await _unitOfWork.AuditFieldValueRepository.GetListAsync(
                answer => auditIds.Contains(answer.AuditId));

            ICollection<CriteriaAuditDetailsDto> criteriaAuditsDto = new List<CriteriaAuditDetailsDto>();

            foreach (var audit in audits)
            {
                audit.FormVersion.Columns = questions.Where(question => question.FormVersionId == audit.FormVersionId).ToList();
                audit.FormVersion.FormFields = fields.Where(field => field.FormVersionId == audit.FormVersionId).ToList();

                audit.Values = questionAnswers.Where(answer => answer.AuditId == audit.Id).ToList();
                audit.AuditFieldValues = fieldAnswers.Where(answer => answer.AuditId == audit.Id).ToList();

                var criteriaAuditDto = await MapAuditDetailsAsync(audit) as CriteriaAuditDetailsDto;
                criteriaAuditsDto.Add(criteriaAuditDto);
            }

            stopWatch.Stop();
            telemetryProperties?.Add("3. End Get Audits", $"Time: {DateTime.UtcNow}. Audits Count: {criteriaAuditsDto.Count()}. Duration: {stopWatch.Elapsed}.");

            return await GetCriteriaPdfAsync(criteriaAuditsDto, filter, telemetryProperties);
        }

        public async Task<byte[]> GetTrackerPdfAsync(PdfFilter filter, Dictionary<string, string> telemetryProperties = null)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            telemetryProperties?.Add("2. Start Get Audits", $"Time: {DateTime.UtcNow}.");

            IReadOnlyCollection<Audit> audits = await _unitOfWork.AuditRepository.GetSubmittedAuditsAsync(filter);

            var auditIds = audits.Select(audit => audit.Id).ToArray();
            var formVersionsIds = audits.Select(audit => audit.FormVersionId).Distinct().ToArray();

            var questions = await _unitOfWork.TableColumnRepository.GetFormTrackerQuestionsAsync(formVersionsIds);

            var questionAnswers = await _unitOfWork.AuditTableColumnValueRepository.GetListAsync(
              answer => auditIds.Contains(answer.AuditId));

            foreach (var audit in audits)
            {
                audit.FormVersion.Columns = questions.Where(question => question.FormVersionId == audit.FormVersionId).ToList();

                audit.Values = questionAnswers.Where(answer => answer.AuditId == audit.Id).ToList();
            }

            var trackerAuditsDto = _mapper.Map<IReadOnlyCollection<TrackerAuditDetailsDto>>(audits);

            stopWatch.Stop();
            telemetryProperties?.Add("3. End Get Audits", $"Time: {DateTime.UtcNow}. Audits Count: {trackerAuditsDto.Count()}. Duration: {stopWatch.Elapsed}.");

            return GetTrackerPdf(trackerAuditsDto, telemetryProperties);
        }

        public async Task<List<Audit>> GetAuditsByUserTimeAndShift(int organizationId, int userId, DateTime startTime, DateTime endTime)
        {
            DateTime userStartDate = DateHelpers.ConvertDateTimeToDateTimeWithUTC(startTime, _userService.GetCurrentUserTimeZone());
            DateTime userEndDate = DateHelpers.ConvertDateTimeToDateTimeWithUTC(endTime, _userService.GetCurrentUserTimeZone());

            return await _unitOfWork.AuditRepository.GetAuditsByUserTimeAndShift(organizationId, userId, userStartDate, userEndDate);
        }

        private async Task<AuditDto> GetAndVerifyAuditDetails(Audit audit)
        {
            var auditDto = await GetAuditAsync(audit.Id);

            if (auditDto.Form.AuditType.Name == "MDS")
            {
                var mdsAuditDto = auditDto as MdsAuditDetailsDto;

                if (mdsAuditDto != null && audit.IsFilled != mdsAuditDto.IsReadyForNextStatus)
                {
                    audit.IsFilled = mdsAuditDto.IsReadyForNextStatus;

                    await _unitOfWork.SaveChangesAsync();
                }

            }
            else
            {
                var criteriaAuditDto = auditDto as CriteriaAuditDetailsDto;

                if (criteriaAuditDto != null && audit.IsFilled != criteriaAuditDto.IsReadyForNextStatus)
                {
                    audit.IsFilled = criteriaAuditDto.IsReadyForNextStatus;

                    await _unitOfWork.SaveChangesAsync();
                }

            }


            return auditDto;
        }

        private byte[] GetTrackerPdf(IReadOnlyCollection<TrackerAuditDetailsDto> auditsDto, Dictionary<string, string> telemetryProperties = null)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            telemetryProperties?.Add("4. Start Generate Pdf", $"Time: {DateTime.UtcNow}.");

            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            var pdfDocument = new PdfDocument(writer);

            Document document = GetTrackerPDFDocument(pdfDocument, auditsDto);

            document.Close();

            stopWatch.Stop();
            telemetryProperties?.Add("5. End Generate Pdf", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}.");

            return stream.ToArray();
        }

        private Document GetTrackerPDFDocument(PdfDocument pdfDocument, IReadOnlyCollection<TrackerAuditDetailsDto> auditsDto)
        {
            using Document document = new Document(pdfDocument, PageSize.A4.Rotate(), true);
            List<AuditDto> baseAudits = auditsDto.Cast<AuditDto>().ToList();

            AuditHeaderEventHandler header = new TrackerAuditHeaderEventHandler(document, baseAudits, $"{auditsDto.First().Form.Name} Tracker");
            AddRepeatedElements(pdfDocument, header);
            SetMargins(document, header,5);

            var auditsGroupped = auditsDto.GroupBy(a => new { FormVersion = a.FormVersion.Version, Facility =  a.Facility}).OrderBy(group => group.Key.FormVersion).ToList();
            foreach (var auditGroup in auditsGroupped)
            {
                var audits = auditGroup.ToList();
                ProcessTrackerDocument(document, audits);
            }

            return document;
        }


        private async Task<byte[]> GetCriteriaPdfAsync(IEnumerable<CriteriaAuditDetailsDto> auditsDto,PdfFilter filter, Dictionary<string, string> telemetryProperties = null)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            telemetryProperties?.Add("4. Start Generate Pdf", $"Time: {DateTime.UtcNow}.");

            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            var pdfDocument = new PdfDocument(writer);

            var pageSize = PageSize.A4;

            using Document document = new Document(pdfDocument, pageSize, true);

            ICollection<HeaderItem> headerItems = new List<HeaderItem>();

            AuditDto auditDto = auditsDto.FirstOrDefault();

            Audit audit = null;

            if (auditDto is null)
            {
                audit = await _unitOfWork.AuditRepository.GetAuditForCriteriaPdfAsync(filter);
            }

            float availableWidth = pageSize.GetWidth() - 40;

            headerItems.Add(new HeaderItem()
            {
                Label = "Organization",
                Value = auditDto?.Organization.Name ?? audit?.Facility.Organization.Name,
                WidthPoints = (float)(availableWidth * 0.3)
            });
            headerItems.Add(new HeaderItem()
            {
                Label = "Facility",
                Value = auditDto?.Facility.Name ?? audit?.Facility.Name,
                WidthPoints = (float)(availableWidth * 0.4)
            });

            var dateFrom = filter.FromDate?.ToString(DateTimeConstants.MMM_DD_COMA_YYYY);
            var dateTo = filter.ToDate?.ToString(DateTimeConstants.MMM_DD_COMA_YYYY);
            var dateText = dateTo == null ? dateFrom : $"{dateFrom} - {dateTo}";

            headerItems.Add(new HeaderItem() { Label = "Dates", Value = dateText, WidthPoints = (float)(availableWidth * 0.3) });

            HeaderEventHandler header = new HeaderEventHandler(document, auditDto?.Form.Name ?? audit?.FormVersion.Form.Name, headerItems, auditsDto.ToArray());

            pdfDocument.AddEventHandler(PdfDocumentEvent.START_PAGE, header);
            //pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, header);
            pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new AuditFooterEventHandler());
            pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new WatermarkingEventHandler());

            var sideMargin = 20;
            var topMargin = 20 + header.Height;
            var bottomMargin = 50;
            document.SetMargins(topMargin, sideMargin, bottomMargin, sideMargin);

            ProcessCriteriaDocumentAsync(document, auditsDto.ToArray());

            document.Close();

            stopWatch.Stop();
            telemetryProperties?.Add("5. End Generate Pdf", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}.");

            return stream.ToArray();
        }

        private Expression<Func<Audit, FilterOptionQueryModel>> GetColumnSelector(AuditFilterColumn columnName)
        {
            var columnSelectorMap = new Dictionary<AuditFilterColumn, Expression<Func<Audit, FilterOptionQueryModel>>>
            {
                { AuditFilterColumn.OrganizationName, i => new FilterOptionQueryModel { Id = i.Facility.OrganizationId, Value = i.Facility.Organization.Name } },
                { AuditFilterColumn.FacilityName, i => new FilterOptionQueryModel { Id = i.FacilityId, Value = i.Facility.Name } },
                { AuditFilterColumn.FormName, i => new FilterOptionQueryModel { Id = i.FormVersion.FormId, Value = i.FormVersion.Form.Name } },
                { AuditFilterColumn.AuditType, i => new FilterOptionQueryModel { Id = i.FormVersion.Form.AuditTypeId, Value = i.FormVersion.Form.AuditType.Name } },
                { AuditFilterColumn.AuditorName, i => new FilterOptionQueryModel { Id = i.SubmittedByUserId, Value = i.SubmittedByUser.FullName } },
                { AuditFilterColumn.Unit, i => new FilterOptionQueryModel { Value = i.Unit } },
                { AuditFilterColumn.Room, i => new FilterOptionQueryModel { Value = i.Room } },
                { AuditFilterColumn.Resident, i => new FilterOptionQueryModel { Value = i.ResidentName } },
                { AuditFilterColumn.Identifier, i => new FilterOptionQueryModel { Value = i.Identifier } },
                { AuditFilterColumn.DeletedByUser, i => new FilterOptionQueryModel { Id = i.DeletedByUserId, Value = i.DeletedByUser.FullName } },
            };

            if (!columnSelectorMap.TryGetValue(columnName, out var columnSelector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return columnSelector;
        }

        private Expression<Func<Audit, object>> GetColumnOrderSelector(AuditFilterColumn columnName)
        {
            var columnSelectorMap = new Dictionary<AuditFilterColumn, Expression<Func<Audit, object>>>
            {
                { AuditFilterColumn.OrganizationName, i => i.Facility.Organization.Name },
                { AuditFilterColumn.FacilityName, i => i.Facility.Name },
                { AuditFilterColumn.FormName, i => i.FormVersion.Form.Name },
                { AuditFilterColumn.AuditType, i => i.FormVersion.Form.AuditType.Name },
                { AuditFilterColumn.AuditDate, i => i.SubmittedDate },
                { AuditFilterColumn.AuditorName, i => i.SubmittedByUser.FullName },
                { AuditFilterColumn.Unit, i => i.Unit },
                { AuditFilterColumn.Room, i => i.Room },
                { AuditFilterColumn.Resident, i => i.ResidentName },
                { AuditFilterColumn.Identifier, i => i.Identifier },
                { AuditFilterColumn.IncidentDateFrom, i => i.IncidentDateFrom },
                { AuditFilterColumn.IncidentDateTo, i => i.IncidentDateTo },
                { AuditFilterColumn.Compliance, i => i.TotalCompliance },
                { AuditFilterColumn.Status, i => i.Status },
                { AuditFilterColumn.LastDeletedDate, i => i.LastDeletedDate },
                { AuditFilterColumn.DeletedByUser, i => i.DeletedByUser.FullName },
            };

            if (!columnSelectorMap.TryGetValue(columnName, out var columnSelector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return columnSelector;
        }

        private Cell CreateKeywordCell(int rowSpan)
        {
            return new Cell(rowSpan, 1)
                .SetBorder(Border.NO_BORDER)
                .SetWidth(200)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFontSize(10)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA));
        }

        private void AddRepeatedElements(PdfDocument pdfDocument, AuditHeaderEventHandler header)
        {
            pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, header);
            pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new AuditFooterEventHandler());
            pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new WatermarkingEventHandler());
        }

        private void SetMargins(Document document, AuditHeaderEventHandler header,int sideMargin = 20)
        {
            var topMargin = 20 + header.Height;
            var bottomMargin = 50;
            document.SetMargins(topMargin, sideMargin, bottomMargin, sideMargin);
        }

        private Document ProcessKeywordsDocument(Document document, HourKeywordAuditDetailsDto audit)
        {
            var keywords = audit.Keywords.ToList()
                .FindAll(keyword => keyword.Hidden==null)
                .FindAll(keyword => keyword.Name.ToLower()!="empty keyword")
                .FindAll(keyword => keyword.Name.Length>0);

            var matchedCustomKeywords = audit.MatchedKeywords.ToList()
                .FindAll(keyword => keyword.Keyword?.Hidden == 1)
                .FindAll(keyword => keyword.Keyword?.Name.ToLower() != "empty keyword")
                .FindAll(keyword => keyword.Keyword?.Name.Length > 0).ConvertAll(keyword => keyword.Keyword).ToList();

            keywords.AddRange(matchedCustomKeywords);


            Rectangle pageSize = document.GetPdfDocument().GetDefaultPageSize();

            var keywordColumnCount = 3;
            var keywordHeightCalcParagraph = new Paragraph("KEYWORD");
            var keywordHeightCalcCell = CreateKeywordCell(1).Add(keywordHeightCalcParagraph);
            var keywordHeight = HeightCalculator.GetHeight(keywordHeightCalcCell, document);
            var keywordsPerPage = (int)Math.Floor(pageSize.GetHeight() / keywordHeight * keywordColumnCount);

            keywordsPerPage = keywordsPerPage % keywordColumnCount < keywordColumnCount
                ? keywordsPerPage - (keywordsPerPage % keywordColumnCount) + keywordColumnCount : keywordsPerPage;
            var pageCount = (int)Math.Ceiling((double)keywords.Count / keywordsPerPage);

            var colorKeywordMatch = new DeviceRgb(180, 0, 0);
            var colorKeywordDefault = new DeviceRgb(38, 50, 55);
            var colorKeywordDescription = new DeviceRgb(80, 80, 80);
            var colorKeywordInfo = new DeviceRgb(0, 0, 0);

            for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
            {
                int keywordsLeft = keywords.Count - keywordsPerPage * pageIndex;
                int keywordPerColumn = Math.Min(keywordsLeft, keywordsPerPage / keywordColumnCount);

                var keywordsForPage = keywords.Skip(pageIndex * keywordsPerPage).Take(keywordsPerPage).ToList();
                var keywordTable = new Table(keywordColumnCount);

                for (int columnIndex = 0; columnIndex < keywordColumnCount; columnIndex++)
                {
                    int startIdx = columnIndex * keywordPerColumn;
                    int count = Math.Min(keywordPerColumn, keywordsForPage.Count - startIdx);
                    var keywordsForColumn = keywordsForPage.Skip(startIdx).Take(count);
                    var keywordCell = CreateKeywordCell(keywordsForColumn.Count());
                    foreach (var keyword in keywordsForColumn)
                    {
                        var match = audit.MatchedKeywords.Any(matchKeyw => matchKeyw.Keyword?.Id == keyword.Id);
                        var color = match ? colorKeywordMatch : colorKeywordDefault;
                        var paragraph = new Paragraph(keyword.Name.ToUpper()).SetFontColor(color);
                        keywordCell = keywordCell.Add(paragraph);

                    }
                    keywordTable.AddCell(keywordCell);
                }
                document.Add(keywordTable);
            }

            var keywordGroups = audit.MatchedKeywords
                .OrderBy(match => match.Keyword?.Name)
                .GroupBy(match => match.Keyword?.Name)
                .Select(group => new {
                    KeywordName = group.Key,
                    Values = group
                });

            if(!keywordGroups.Any())
            {
                return document;
            }

            document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

            var lineColor = new DeviceRgb(150, 150, 150);
            var darkLineColor = new DeviceRgb(100, 100, 100);

            SolidLine sl = new SolidLine();
            sl.SetColor(lineColor);
            LineSeparator ls = new LineSeparator(sl)
                .SetMarginLeft(-20)
                .SetMarginRight(-20);

            SolidLine darkSL = new SolidLine();
            darkSL.SetColor(darkLineColor);
            LineSeparator darkLS = new LineSeparator(darkSL)
                .SetMarginLeft(-20)
                .SetMarginRight(-20);

            foreach (var group in keywordGroups)
            {
                Paragraph keywordName = new Paragraph(group.KeywordName?.ToUpper())
                    .SetFontSize(10)
                    .SetFontColor(colorKeywordMatch);

                document.Add(keywordName);
                document.Add(ls);

                var percentArray = UnitValue.CreatePercentArray(new float[] { 20, 80 });

                foreach (var keyword in group.Values)
                {
                    var keywordTable = new Table(percentArray)
                        .SetMarginBottom(0);
                    var residentCell = CreateKeywordCell(1)
                        .SetPaddingLeft(0)
                        .SetPaddingBottom(0);
                    var timeCell = CreateKeywordCell(1)
                        .SetPaddingBottom(0);

                    Cell highAlertCell = null;
                    if (keyword.HighAlertAuditValue != null)
                    {
                        highAlertCell = CreateKeywordCell(1)
                        .SetPaddingLeft(0)
                        .SetPaddingBottom(0);

                        var highAlert = new Paragraph(HighAlert)
                                   .SetFontSize(12)
                                   .SetFontColor(colorKeywordMatch);
                        highAlertCell = highAlertCell.Add(highAlert);
                    }
                    var resident = new Paragraph(keyword.Resident)
                        .SetFontSize(12)
                        .SetFontColor(colorKeywordInfo);
                    var time = new Paragraph(keyword.ProgressNoteDateTime)
                        .SetFontSize(12)
                        .SetFontColor(colorKeywordInfo);

                    residentCell = residentCell.Add(resident);
                    timeCell = timeCell.Add(time);

                    
                    keywordTable.AddCell(residentCell);
                    if(highAlertCell != null)
                    {
                        keywordTable.AddCell(highAlertCell);
                    }
                    keywordTable.AddCell(timeCell);

                    Paragraph keyworDescription = new Paragraph(keyword.Description)
                    .SetFontSize(12)
                    .SetFontColor(colorKeywordDescription)
                    .SetMarginTop(0);

                    document.Add(keywordTable);
                    document.Add(keyworDescription);
                    document.Add(darkLS);
                }
            }

            return document;
        }

        private void ProcessCriteriaDocumentAsync(Document document, params CriteriaAuditDetailsDto[] audits)
        {
            if(!audits.Any())
            {
                return;
            }

            var isCombined = audits.Count() > 1;

            

            // Moved the summary page to each page header.
            // 
            if (isCombined)
            {
                document.Add(PdfHelper.CreateCriteriaAuditSummaryTable(audits));
            }

            int auditCounter = 0;
            foreach (var audit in audits)
            {
                if (isCombined)
                {
                    document.SetProperty(Property.ID, auditCounter);
                    document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                    
                }
                if (!isCombined)
                {
                    Table detailedHeader = PdfHelper.CreateCriteriaAuditDetailedHeader(audit);
                    detailedHeader.SetDestination(string.Format("{0}-{1}", audit.Resident ?? string.Empty, auditCounter++));
                    document.Add(detailedHeader);
                }
                
                Table subheaderData = PdfHelper.CreateCriteriaAuditSubheaderData(audit);
                subheaderData.SetDestination(string.Format("{0}-{1}", audit.Resident ?? string.Empty, auditCounter));
                document.Add(subheaderData);
                

                Table detailedData = PdfHelper.CreateCriteriaAuditDetailedData(audit);
                document.Add(detailedData);
                auditCounter++;

            }
        }

        private void ProcessMdsDocumentAsync(Document document, params MdsAuditDetailsDto[] audits)
        {
            if (!audits.Any())
            {
                return;
            }

            var isCombined = audits.Count() > 1;



            // Moved the summary page to each page header.
            // 
            if (isCombined)
            {
                document.Add(PdfHelper.CreateMdsAuditSummaryTable(audits));
            }

            int auditCounter = 0;
            foreach (var audit in audits)
            {
                if (isCombined)
                {
                    document.SetProperty(Property.ID, auditCounter);
                    document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                }
                if (!isCombined)
                {
                    Table detailedHeader = PdfHelper.CreateMdsAuditDetailedHeader(audit);
                    detailedHeader.SetDestination(string.Format("{0}-{1}", audit.Resident ?? string.Empty, auditCounter++));
                    document.Add(detailedHeader);
                }

                //Table subheaderData = PdfHelper.CreateMdsAuditSubheaderData(audit);
                //subheaderData.SetDestination(string.Format("{0}-{1}", audit.Resident ?? string.Empty, auditCounter));
                //document.Add(subheaderData);


                Table detailedData = PdfHelper.CreateMdsAuditDetailedData(audit);
                document.Add(detailedData);
                auditCounter++;

            }
        }

        private void AddTrackerCounterCell(Table table, int counter, float width)
        {
            table.AddCell(CreateRegularCell((counter + 1).ToString(), width: width));
        }

        private void AddTrackerAnswerCell(Table table, int questionIndex, List<TrackerQuestionDto> auditTrackerQuestions, Dictionary<string, object> answers, float maxWidrh)
        {
            var trackerAnswer = answers[auditTrackerQuestions[questionIndex].Id.ToString()] as TrackerAnswerDto;

            if (auditTrackerQuestions.Count > questionIndex && trackerAnswer != null && trackerAnswer.FormattedAnswer != null)
            {
                Cell answer = null;
                if (trackerAnswer.HighAlertAuditValue != null)
                    answer = CreateDoubleColorCell(trackerAnswer.FormattedAnswer, HighAlert);
                else
                    answer = trackerAnswer.FieldType == FieldTypes.TextArea
                        ? PdfHelper.CreateTextAreaCell(trackerAnswer.FormattedAnswer)
                        : CreateRegularCell(trackerAnswer.FormattedAnswer);

                answer
                    .SetMaxWidth(maxWidrh)
                    .SetBorderBottom(new SolidBorder(new DeviceRgb(100, 100, 100), 1));

                table.AddCell(answer);
            }
            else
            {
                table.AddCell(CreateRegularCell(string.Empty));
            }
        }

        private Cell CreateDoubleColorCell(string formattedAnswer, string highAlert)
        {

            Text blackText = new Text(formattedAnswer).SetFontColor(new DeviceRgb(38, 50, 55));
            var redColor = new DeviceRgb(180, 0, 0);
            Text greenText = new Text(highAlert).SetFontColor(redColor);
            var paragraph = new Paragraph();
            paragraph.Add(blackText);
            paragraph.Add(greenText);

            return CreateRegularCell(paragraph);
        }

        private bool IsTrackerComplianceQuestionAnswered(int questionIndex, List<TrackerQuestionDto> auditTrackerQuestions, Dictionary<string, object> answers)
        {
            var trackerAnswer = answers[auditTrackerQuestions[questionIndex].Id.ToString()] as TrackerAnswerDto;

            if (auditTrackerQuestions.Find(atq => atq.Id == trackerAnswer.QuestionId).TrackerOption.Compliance)
            {
                if (trackerAnswer!=null && trackerAnswer.FormattedAnswer.ToLower() != "no")
                {
                    return true;
                }
                if (auditTrackerQuestions.Find(atq => atq.Id == trackerAnswer.QuestionId).Question.ToLower().Contains("has the resident been refusing") && trackerAnswer != null && trackerAnswer.FormattedAnswer.ToLower() == "no")
                {
                    return true;
                }
            }

            
            return false;
        }

        private int GetTrackerGroupComplianceCount(Dictionary<string, object> answers, List<TrackerQuestionDto> auditTrackerQuestions)
        {
            int compliance = 0;

            List<TrackerAnswerDto> trackerAnswers = answers.Values.OfType<TrackerAnswerDto>().ToList();

            foreach(var answer in trackerAnswers)
            {
                var question = auditTrackerQuestions.Find(atq => atq.Id == answer.QuestionId);
                if (question != null && question.TrackerOption.Compliance)
                {
                    if (answer.FormattedAnswer.ToLower() != "no")
                    {
                        compliance++;
                    }

                    if (question.Question.ToLower().Contains("has the resident been refusing") && answer.FormattedAnswer.ToLower() == "no")
                    {
                        compliance++;
                    }
                }
            }
            

            return compliance;
        }

        private Document ProcessTrackerDocument(Document document, List<TrackerAuditDetailsDto> audits)
        {
            const float NUM_WIDTH = 10f;

            var pageSize = document.GetPdfDocument().GetDefaultPageSize();
            var availableWidth = pageSize.GetWidth() - NUM_WIDTH; // left/right paddings, number column

            var firstAudit = audits.First();
            List<TrackerQuestionDto> auditTrackerQuestions = firstAudit.FormVersion.Questions.ToList();

            var tableHeaderNumberOfCols = 1;
            int EXTRA_COLUMNS = 0;

            var hasCompliance = auditTrackerQuestions.Any(atq => atq.TrackerOption.Compliance == true);

            if (hasCompliance)
            {
                tableHeaderNumberOfCols = 2;
                EXTRA_COLUMNS = 2;
            }

            var tableHeader = new Table(tableHeaderNumberOfCols, true);


            var redColor = new DeviceRgb(180, 0, 0);
            var greenColor = new DeviceRgb(0, 120, 30);

            int MAX_TABLE_COLUMNS = 9 + EXTRA_COLUMNS;
          
            int tablesCount = (int)Math.Ceiling((double)auditTrackerQuestions.Count() / MAX_TABLE_COLUMNS);

            tableHeader.AddCell(CreateTrackerHeaderCell("", TextAlignment.LEFT));

            float compliancePercentage = 0;
            int answerGroupsCount = 0;

            foreach (var audit in audits)
            {
                var answerGroups = audit.PivotAnswerGroups.ToList();
                answerGroupsCount = answerGroups.Count;

                for (int j = 0; j < answerGroups.Count; j++)
                {
                    float totalComplianceQuestions = auditTrackerQuestions.FindAll(atq => atq.TrackerOption.Compliance).Count;
                    float totalComplianceQuestionsAnswered = 0;

                    var answerGroup = answerGroups[j];

                    var answers = (answerGroup as Dictionary<string, object>);
                    for (int k = 0; k < auditTrackerQuestions.Count; k++)
                    {
                        if (IsTrackerComplianceQuestionAnswered(k, auditTrackerQuestions, answers))
                        {
                            totalComplianceQuestionsAnswered++;
                        }
                    }
                    compliancePercentage += (totalComplianceQuestionsAnswered / totalComplianceQuestions) * 100;
                }
            }


            if (hasCompliance)
            {
                tableHeader.AddCell(CreateTrackerHeaderComplianceCell($"Compliance Summary: {Math.Round(compliancePercentage / answerGroupsCount, 2)}% ", TextAlignment.RIGHT));
            }

            document.Add(tableHeader);

            for (int tableCounter = 0; tableCounter < tablesCount; tableCounter++)
            {
                int startIndex = tableCounter * MAX_TABLE_COLUMNS;
                int endIndex = GetTableEndIndex(tableCounter, MAX_TABLE_COLUMNS, auditTrackerQuestions.Count);

                var table = new Table(endIndex - startIndex + 1 + EXTRA_COLUMNS);

                float maxWidth = (availableWidth / (endIndex - startIndex + EXTRA_COLUMNS) );

                table.AddCell(CreateHeaderCell("#", NUM_WIDTH));
                //var maxLength = auditTrackerQuestions.Max(x => x.Question.Length);
                //if (maxLength > 20)
                //    maxWidth = maxWidth - 60f;

                for (int i = startIndex; i < endIndex; i++)
                {
                    table.AddCell(CreateHeaderCell(auditTrackerQuestions[i].Question, maxWidth));
                }

                if (hasCompliance)
                {
                    table.AddCell(CreateHeaderCompliaceCell("Compliance Questions", maxWidth)).SetTextAlignment(TextAlignment.CENTER);
                    table.AddCell(CreateHeaderCompliaceCell("Total Compliance", maxWidth)).SetTextAlignment(TextAlignment.CENTER);
                }
                

                foreach (var audit in audits)
                {
                    var answerGroups = audit.PivotAnswerGroups.ToList();
                    float totalComplianceQuestionsAnswered = 0;
                    float totalComplianceQuestions = auditTrackerQuestions.FindAll(atq => atq.TrackerOption.Compliance).Count;
                    if (hasCompliance)
                    {
                        for (int j = 0; j < answerGroups.Count; j++)
                        {
                            var answerGroup = answerGroups[j];
                            var answers = (answerGroup as Dictionary<string, object>);

                            for (int k = 0; k < auditTrackerQuestions.Count; k++)
                            {
                                if (IsTrackerComplianceQuestionAnswered(k, auditTrackerQuestions, answers))
                                {
                                    totalComplianceQuestionsAnswered++;
                                }
                            }
                        }
                    }

                    for (int j = 0; j < answerGroups.Count; j++)
                    {
                        AddTrackerCounterCell(table, j, NUM_WIDTH);

                        var answerGroup = answerGroups[j];
                        var answers = (answerGroup as Dictionary<string, object>);
                        
                        for (int k = startIndex; k < endIndex; k++)
                        {
                            AddTrackerAnswerCell(table, k, auditTrackerQuestions, answers, maxWidth);                            
                        }

                        if (hasCompliance)
                        {
                            var complianceCount = GetTrackerGroupComplianceCount(answers, auditTrackerQuestions);
                            table.AddCell(CreateRegularComplianceCell($"{totalComplianceQuestions}", 1f, 60f)).SetTextAlignment(TextAlignment.CENTER);
                            table.AddCell(CreateRegularComplianceCell($"{Math.Round(complianceCount / totalComplianceQuestions * 100, 2)}% ", 1f, 60f)).SetTextAlignment(TextAlignment.CENTER);
                        }
                    }
                    
                }
                
                document.Add(table);
            }

            return document;
        }
        Paragraph MakeHeaderParagraph(string text,bool split = false)
        {
            var texts = text.Split(' ').ToList();
            var paragraph = new Paragraph();
            //if( texts.Count > 2 && split)
            //{
            //    foreach (var word in texts)
            //    {
            //        paragraph.Add(word);
            //        paragraph.Add("\r\n");
            //    }
            //}else
            //{
            //    paragraph.Add(text);
            //}
            paragraph.SetPadding(1);
            paragraph.Add(text);
            return paragraph.SetMultipliedLeading(1.2f);
        }
        Cell CreateHeaderCell(string text, float width = 100f) => new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderTop(new SolidBorder(new DeviceRgb(100, 100, 100), 1.5f))
                .SetBorderBottom(new SolidBorder(new DeviceRgb(100, 100, 100), 1.5f))
                .SetWidth(width)
                //.SetFontColor(new DeviceRgb(38, 50, 55))
                .SetOpacity(0.9f)
                .SetFontSize(9)            
                .SetPaddingTop(5)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetBackgroundColor(new DeviceRgb(247, 247, 247))
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA)).Add(MakeHeaderParagraph(text,true));

        Cell CreateHeaderCompliaceCell(string text, float width = 100f) => new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderTop(new SolidBorder(new DeviceRgb(100, 100, 100), 1.5f))
                .SetBorderBottom(new SolidBorder(new DeviceRgb(100, 100, 100), 1.5f))
                .SetWidth(width)
                //.SetFontColor(new DeviceRgb(38, 50, 55))
                .SetOpacity(0.9f)
                .SetFontSize(9)
                .SetPaddingTop(5)
                .SetBackgroundColor(new DeviceRgb(247, 247, 247))
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA)).Add(MakeHeaderParagraph(text));

        Cell CreateRegularCell(string text, float opacity = 1f, float width = 100f) => new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(new SolidBorder(new DeviceRgb(100, 100, 100), 1))
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFontSize(9)
                .SetWidth(width)
                .SetFontColor(GetTextColor(text))
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                //.SetHeight(31)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                .Add(new Paragraph(text))
                .SetOpacity(opacity);

        Cell CreateRegularCell(Paragraph paragraph, float opacity = 1f, float width = 100f) => new Cell(1, 1)
        .SetBorder(Border.NO_BORDER)
        .SetBorderBottom(new SolidBorder(new DeviceRgb(100, 100, 100), 1))
        .SetTextAlignment(TextAlignment.LEFT)
        .SetFontSize(9)
        .SetWidth(width)
        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
        //.SetHeight(31)
        .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
        .Add(paragraph)
        .SetOpacity(opacity);

        Cell CreateRegularComplianceCell(string text, float opacity = 1f, float width = 100f) => new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(new SolidBorder(new DeviceRgb(100, 100, 100), 1))
                .SetFontSize(9)
                .SetWidth(width)
                .SetFontColor(GetTextColor(text))
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                //.SetHeight(31)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                .Add(new Paragraph(text))
                .SetOpacity(opacity);

        Cell CreateTrackerHeaderCell(string text, TextAlignment textAlignment = TextAlignment.LEFT, float width = 100f) => new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(new SolidBorder(new DeviceRgb(100, 100, 100), 1))
                .SetTextAlignment(textAlignment)
                .SetFontSize(9)
                .SetOpacity(0.9f)
                .SetWidth(width)
                .SetFontColor(GetTextColor(text))
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetMarginTop(0)
                .SetMarginBottom(0)
                .SetPadding(2)
                .SetPaddingBottom(4)
                //.SetHeight(31)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                .SetBackgroundColor(new DeviceRgb(238, 236, 236))
                .Add(MakeHeaderParagraph(text));

        Cell CreateTrackerHeaderComplianceCell(string text, TextAlignment textAlignment = TextAlignment.LEFT, float width = 100f) => new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(new SolidBorder(new DeviceRgb(100, 100, 100), 1))
                .SetTextAlignment(textAlignment)
                .SetFontSize(9)
                .SetOpacity(0.9f)
                .SetWidth(width)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetMarginTop(0)
                .SetMarginBottom(0)
                .SetPadding(2)
                .SetPaddingBottom(4)
                //.SetHeight(31)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                .SetBackgroundColor(new DeviceRgb(238, 236, 236))
                .Add(new Paragraph(text));

        private int GetTableEndIndex(int tableCounter, int maxTableColumns, int totalRecordsCount)
        {
            int numOfColumnsToTake = (tableCounter * maxTableColumns) + maxTableColumns;
            if (numOfColumnsToTake > totalRecordsCount)
            {
                numOfColumnsToTake = totalRecordsCount;
            }
            return numOfColumnsToTake;
        }

        private DeviceRgb GetTextColor(string option)
        {
            DeviceRgb color = null;
            if (!string.IsNullOrEmpty(option) && option.ToLower() == "no")
            {
                color = new DeviceRgb(245, 0, 0);
            }
            else
            {
                color = new DeviceRgb(38, 50, 55);
            }

            return color;
        }

        private bool MapCriteriaAuditAnswersToQuestionsAndCheckIsFilled(CriteriaAuditDetailsDto criteriaAuditDto)
        {
            List<bool> readyForNextStatusList = new List<bool>();

            foreach (var group in criteriaAuditDto.FormVersion.QuestionGroups)
            {
                if (group?.Questions == null)
                {
                    continue;
                }

                foreach (var question in group?.Questions)
                {
                    question.Answer = criteriaAuditDto.Answers?.FirstOrDefault(answer => answer.TableColumnId == question.Id);
                    if (criteriaAuditDto.Form.AllowEmptyComment==1)
                    {
                        readyForNextStatusList.Add(true);
                    } else
                    {
                        readyForNextStatusList.Add(IsAnswerComplete(question.Answer));
                    }
                    

                    if (question?.SubQuestions == null)
                    {
                        continue;
                    }

                    foreach (var subQuestion in question?.SubQuestions)
                    {
                        subQuestion.Answer = criteriaAuditDto.Answers.FirstOrDefault(answer => answer.TableColumnId == subQuestion.Id);
                        if (criteriaAuditDto.Form.AllowEmptyComment == 1)
                        {
                            readyForNextStatusList.Add(true);
                        }
                        else
                        {
                            if (question.Answer != null && question.Answer.Value == CommonConstants.YES.ToLower())
                            {
                                readyForNextStatusList.Add(IsAnswerComplete(subQuestion.Answer));
                            }
                        }
                        
                    }
                }
            }

            foreach (var field in criteriaAuditDto.FormVersion.FormFields)
            {
                field.Value = criteriaAuditDto.SubHeaderValues?.FirstOrDefault(value => value.FormFieldId == field.Id);
            }

            return !string.IsNullOrEmpty(criteriaAuditDto.Resident) &&
                !string.IsNullOrEmpty(criteriaAuditDto.Room) &&
                readyForNextStatusList.All(readyStatus => readyStatus) &&
                criteriaAuditDto.FormVersion.FormFields.Where(field => field.IsRequired).All(field => field.Value != null && !string.IsNullOrEmpty(field.Value.Value));
        }

        private bool MapMdsAuditAnswersToQuestionsAndCheckIsFilled(MdsAuditDetailsDto mdsAuditDto)
        {
            List<bool> readyForNextStatusList = new List<bool>();
            
            foreach (var field in mdsAuditDto.FormVersion.FormFields)
            {
                field.Value = mdsAuditDto.SubHeaderValues?.FirstOrDefault(value => value.FormFieldId == field.Id);
            }

            return !string.IsNullOrEmpty(mdsAuditDto.Resident) &&
                !string.IsNullOrEmpty(mdsAuditDto.Room) &&
                readyForNextStatusList.All(readyStatus => readyStatus) &&
                mdsAuditDto.FormVersion.FormFields.Where(field => field.IsRequired).All(field => field.Value != null && !string.IsNullOrEmpty(field.Value.Value));
        }

        private bool IsAnswerComplete(CriteriaAnswerDto answer)
        {
            return 
                answer != null && 
                answer.Value != null && 
                !string.IsNullOrWhiteSpace(answer.AuditorComment) && 
                !string.IsNullOrWhiteSpace(Regex.Replace(answer.AuditorComment, "<p>\\s*</p>", string.Empty, RegexOptions.Multiline | RegexOptions.IgnoreCase));
        }

        private async Task<DateTime?> CalculateDueDateAsync(Audit audit, DateTime currentDate)
        {
            FormOrganization formOrganization = await _unitOfWork.FormOrganizationRepository
                .FirstOrDefaultAsync(formOrg =>
                formOrg.Organization.Facilities.Any(facility => facility.Id == audit.FacilityId &&
                formOrg.Form.Versions.Any(version => version.Id == audit.FormVersionId)),
                formOrg => formOrg.ScheduleSetting
                );

            //Triggered
            if (formOrganization.ScheduleSetting == null)
            {
                return currentDate;
            }

            int[] days = JsonConvert.DeserializeObject<int[]>(formOrganization.ScheduleSetting.Days);

            if (!days.Any())
            {
                throw new Exception("Wrong form schedule settings");
            }

            switch (formOrganization.ScheduleSetting.ScheduleType)
            {
                case ScheduleType.Weekly:
                    return GetNextWeekday(currentDate, (DayOfWeek)days.First());

                case ScheduleType.Monthly:                    
                    int scheduledDay = days.First();

                    return GetNextMonthDay(currentDate, scheduledDay);

                default:
                    return currentDate;
            }
        }

        private DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }

        private DateTime GetNextMonthDay(DateTime currentDate, int scheduledDay)
        {
            DateTime startDate = currentDate;
                
            if (startDate.Day > scheduledDay)
            {
                startDate = startDate.AddMonths(1);
            }

            int lastDayOfMonth = startDate.GetLastDayOfMonth();

            if (scheduledDay > lastDayOfMonth)
            {
                scheduledDay = lastDayOfMonth;
            }

            return new DateTime(startDate.Year, startDate.Month, scheduledDay);
        }

        private void CheckRightsForChangingAuditStatus(Audit audit, AuditStatus newAuditstatus, int currentUserId)
        {
            string[] roles = _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role)?.Select(claim => claim.Value).ToArray();

            bool result = false;

            switch (newAuditstatus)
            {
                case AuditStatus.InProgress:
                case AuditStatus.WaitingForApproval:
                    result = roles.Contains(UserRoles.Auditor.ToString()) && audit.SubmittedByUserId == currentUserId;
                    break;
                case AuditStatus.Approved:
                case AuditStatus.Disapproved:
                case AuditStatus.Reopen:
                case AuditStatus.Submitted:
                    result = roles.Contains(UserRoles.Reviewer.ToString());
                    break;
                default:
                    break;
            }

            if (!result)
            {
                throw new Exception($"You don't have rights to change the status {newAuditstatus}");
            }
        }

        public async Task<bool> IsAuditTriggedByKeyword(int id)
        {
            bool allow =  await _unitOfWork.AuditTriggeredRepository.IsAuditCreatedByKeywordTrigger(id);
            var user = await _userService.GetLoggedInUserAsync();
            AuditDto audit = await GetAuditAsync(id);

            if(user?.Id == audit?.SubmittedByUser?.Id)
            {
                return allow;
            }
            return false;
        }

        public async Task<bool> CreateReportForPortal()
        {
            AuditFilter filter = new AuditFilter();
            filter.AuditDate = new DateFilterModel();
            filter.AuditDate.FirstCondition = new Condition<DateTime>();
            filter.AuditDate.FirstCondition.From = DateTime.Now.AddMonths(-3);
            filter.AuditDate.FirstCondition.To = DateTime.Now;
            filter.AuditDate.FirstCondition.Type = CompareType.Equals;
            var statusList = new List<FilterOption>();
            var status = new FilterOption();
            status.Id = 6;
            status.Value = "Submitted";
            statusList.Add(status);

            filter.Status = statusList;
            filter.OrderBy = "auditDate";
            filter.State = AuditState.Active;
            filter.SortOrder = "desc";
            filter.AuditType = new List<FilterOption>();
            filter.Auditor = new List<FilterOption>();
            filter.Unit = new List<FilterOption>();
            filter.Resident = new List<FilterOption>();
            filter.Room = new List<FilterOption>();
            filter.Form = new List<FilterOption>();
            var organizaFilter = new FilterOption();
            organizaFilter.Id = 52;
            organizaFilter.Value = "Southern Healthcare Management";

            var orgfilters = new List<FilterOption>();
            orgfilters.Add(organizaFilter);
            filter.Organization = orgfilters;
            filter.UserOrganizationIds = await _userService.GetUserOrganizationIdsAsync();
            Expression<Func<Audit, object>> orderBySelector =
                    OrderByHelper.GetOrderBySelector<AuditFilterColumn, Expression<Func<Audit, object>>>(filter.OrderBy, GetColumnOrderSelector);

            filter.TakeCount = 100000;
            Tuple<Audit[], int> tuple = await _unitOfWork.AuditRepository.GetListAsync(filter, orderBySelector);

            var userId = _userService.GetLoggedUserId();
            var reportCat = _unitOfWork.ReportCategoryRepository.GetAll().FirstOrDefault(cat => cat.ReportCategoryName == "Clinical");
            var reportType = _unitOfWork.ReportTypeRepository.GetAll().FirstOrDefault(type => type.TypeName == "PDF");
 
            foreach (Audit audit in tuple.Item1)
            {

                var portal = new PortalReport();
                portal.Name = $"{audit.FormVersion.Form.Name}";
                portal.ReportCategoryId = reportCat.Id;
                portal.ReportTypeId = reportType.Id;
              //  portal.ReportRangeId = reportRange.Id;
                portal.AuditId = audit.Id;
                portal.CreatedAt = audit.SubmittedDate;
                portal.CreatedByUserID = audit.SubmittedByUserId?? userId;
                portal.OrganizationId = audit.FormVersion.Form.Organization.Id;
                portal.FacilityId = audit.FacilityId;
                portal.AuditTypeId = audit.FormVersion.Form.AuditTypeId;
                var report = _unitOfWork.PortalReportRepository.GetAll().FirstOrDefault( r => r.AuditId == audit.Id );
                if(report != null )
                {
                    continue;
                }
                await _unitOfWork.PortalReportRepository.AddAsync(portal);
               
               
            }
            await _unitOfWork.SaveChangesAsync();
            return false;
        }


        public async Task<byte[]> GetAuditExcelAsync(int id, Dictionary<string, string> telemetryProperties = null)
        {
            var workbook = new XLWorkbook();

            AuditDto auditDto = await GetAuditAsync(id);

            if (auditDto.Form.AuditType.Name == CommonConstants.TRACKER)
            {
                ProcessTrackerExcelAsync(workbook, new List<TrackerAuditDetailsDto>() { (TrackerAuditDetailsDto)auditDto });
            }
            else if (auditDto.Form.AuditType.Name == CommonConstants.CRITERIA)
            {
                ProcessCriteriaExcelAsync(workbook, (CriteriaAuditDetailsDto)auditDto);
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private void ProcessTrackerExcelAsync(XLWorkbook workbook, IReadOnlyCollection<TrackerAuditDetailsDto> auditsDto)
        {
            List<AuditDto> baseAudits = auditsDto.Cast<AuditDto>().ToList();

            var auditsGroupped = auditsDto.GroupBy(a => new { FormVersion = a.FormVersion.Version, Facility = a.Facility }).OrderBy(group => group.Key.FormVersion).ToList();

            foreach (var auditGroup in auditsGroupped)
            {
                int tblHeaderLine = 0;
                var audits = auditGroup.ToList();
                var firstAudit = audits.First();
                List<TrackerQuestionDto> auditTrackerQuestions = firstAudit.FormVersion.Questions.ToList();

                var ws = workbook.Worksheets.Add("Audit");
                int currentLine = 1;

                int columnCount = auditTrackerQuestions.Count();
                var hasCompliance = auditTrackerQuestions.Any(atq => atq.TrackerOption.Compliance == true);
                columnCount = columnCount + (hasCompliance ? 2 : 0);
                columnCount = columnCount < 9 ? 9 : columnCount + 1;

                var range = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, columnCount));
                range.Merge();
                range.Value = firstAudit.Form.Name;
                range.Style.Font.Bold = true;
                range.Style.Font.FontSize = 22;
                range.Style.Fill.BackgroundColor = XLColor.FromHtml("#145196");
                range.Style.Font.FontColor = XLColor.White;
                range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                currentLine++;

                var rangeOrganization = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 2));
                rangeOrganization.Merge();
                rangeOrganization.Value = "ORGANIZATION: ";

                var rangeOrganizationVal = ws.Range(ws.Cell(currentLine, 3), ws.Cell(currentLine, columnCount));
                rangeOrganizationVal.Merge();
                rangeOrganizationVal.Value = firstAudit.Organization.Name;

                currentLine++;

                var rangeFacility = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 2));
                rangeFacility.Merge();
                rangeFacility.Value = "FACILITY: ";

                var rangeFacilityVal = ws.Range(ws.Cell(currentLine, 3), ws.Cell(currentLine, columnCount));
                rangeFacilityVal.Merge();
                rangeFacilityVal.Value = firstAudit.Facility.Name;

                currentLine++;

                var rangeDates = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 2));
                rangeDates.Merge();
                rangeDates.Value = "DATES: ";

                var rangeDatesVal = ws.Range(ws.Cell(currentLine, 3), ws.Cell(currentLine, columnCount));
                rangeDatesVal.Merge();
                var dateFrom = firstAudit.IncidentDateFrom?.ToString("MMM dd, yyyy");
                var dateTo = firstAudit.IncidentDateTo?.ToString("MMM dd, yyyy");
                var dateText = dateTo == null ? dateFrom : $"{dateFrom} - {dateTo}";
                rangeDatesVal.Value = dateText;

                currentLine++;

                var rangeAuditDate = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 2));
                rangeAuditDate.Merge();
                rangeAuditDate.Value = "AUDIT DATE: ";

                var rangeAuditDateVal = ws.Range(ws.Cell(currentLine, 3), ws.Cell(currentLine, columnCount));
                rangeAuditDateVal.Merge();
                rangeAuditDateVal.Value = $"{firstAudit.SubmittedDate.AddHours(firstAudit.Facility.TimeZoneOffset).ToString(DateTimeConstants.MM_DD_YYYY_hh_mm_tt_SLASH)} ({firstAudit.Facility.TimeZoneShortName})";

                currentLine++;

                var rangeRecords = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 2));
                rangeRecords.Merge();
                rangeRecords.Value = "RECORDS: ";

                var rangeRecordsVal = ws.Range(ws.Cell(currentLine, 3), ws.Cell(currentLine, columnCount));
                rangeRecordsVal.Merge();
                rangeRecordsVal.Value = firstAudit.PivotAnswerGroups.Count.ToString();

                var rangeSubHeader = ws.Range(ws.Cell(2, 1), ws.Cell(currentLine, 2));
                rangeSubHeader.Style.Font.Bold = true;
                rangeSubHeader.Style.Font.FontSize = 14;
                rangeSubHeader.Style.Fill.BackgroundColor = XLColor.FromHtml("#8EA9DB");
                rangeSubHeader.Style.Font.FontColor = XLColor.Black;

                var rangeSubHeaderVal = ws.Range(ws.Cell(2, 3), ws.Cell(currentLine, columnCount));
                rangeSubHeaderVal.Style.Font.Bold = true;
                rangeSubHeaderVal.Style.Font.FontSize = 14;
                rangeSubHeaderVal.Style.Fill.BackgroundColor = XLColor.FromHtml("#8EA9DB");
                rangeSubHeaderVal.Style.Font.FontColor = XLColor.Black;


                float compliancePercentage = 0;
                int answerGroupsCount = 0;

                foreach (var audit in audits)
                {
                    var answerGroups = audit.PivotAnswerGroups.ToList();
                    answerGroupsCount = answerGroups.Count;

                    for (int j = 0; j < answerGroups.Count; j++)
                    {
                        float totalComplianceQuestions = auditTrackerQuestions.FindAll(atq => atq.TrackerOption.Compliance).Count;
                        float totalComplianceQuestionsAnswered = 0;

                        var answerGroup = answerGroups[j];

                        var answers = (answerGroup as Dictionary<string, object>);
                        for (int k = 0; k < auditTrackerQuestions.Count; k++)
                        {
                            if (IsTrackerComplianceQuestionAnswered(k, auditTrackerQuestions, answers))
                            {
                                totalComplianceQuestionsAnswered++;
                            }
                        }
                        compliancePercentage += (totalComplianceQuestionsAnswered / totalComplianceQuestions) * 100;
                    }
                }

                if (hasCompliance)
                {
                    currentLine++;

                    var rangeCompSummary = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, columnCount));
                    rangeCompSummary.Merge();
                    rangeCompSummary.Value = "Compliance Summary: " + $"{Math.Round(compliancePercentage / answerGroupsCount, 2)}% ";
                    rangeCompSummary.Style.Font.Bold = true;
                    rangeCompSummary.Style.Font.FontSize = 12;
                    rangeCompSummary.Style.Fill.BackgroundColor = XLColor.FromHtml("#8EA9DB");
                    rangeCompSummary.Style.Font.FontColor = XLColor.Black;
                    rangeCompSummary.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                }

                int tablesCount = (int)Math.Ceiling((double)auditTrackerQuestions.Count() / columnCount);

                for (int tableCounter = 0; tableCounter < tablesCount; tableCounter++)
                {
                    int startIndex = tableCounter * columnCount;
                    int endIndex = GetTableEndIndex(tableCounter, columnCount, auditTrackerQuestions.Count);

                    currentLine++;
                    tblHeaderLine = currentLine;

                    var rangeTableHeader = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, columnCount));
                    rangeTableHeader.Style.Font.Bold = true;
                    rangeTableHeader.Style.Font.FontSize = 12;
                    rangeTableHeader.Style.Fill.BackgroundColor = XLColor.FromHtml("#C9F2FF");
                    rangeTableHeader.Style.Font.FontColor = XLColor.Black;
                    rangeTableHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    rangeTableHeader.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    rangeTableHeader.Style.Alignment.SetWrapText(true);

                    ws.Cell(currentLine, 1).Value = "#";

                    if (hasCompliance)
                    {
                        var cellTotalCompliance = ws.Cell(currentLine, 2);
                        cellTotalCompliance.Value = "Total Compliance";
                    }

                    int modifyHeaderCell = hasCompliance ? 3 : 2;
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        var cell = ws.Cell(currentLine, modifyHeaderCell);
                        cell.Value = auditTrackerQuestions[i].Question;
                        modifyHeaderCell++;
                    }

                    if (hasCompliance)
                    {
                        var cellCompQuestions = ws.Cell(currentLine, columnCount);
                        cellCompQuestions.Value = "Compliance Questions";
                    }


                    foreach (var audit in audits)
                    {
                        var answerGroups = audit.PivotAnswerGroups.ToList();
                        float totalComplianceQuestionsAnswered = 0;
                        float totalComplianceQuestions = auditTrackerQuestions.FindAll(atq => atq.TrackerOption.Compliance).Count;
                        if (hasCompliance)
                        {
                            for (int j = 0; j < answerGroups.Count; j++)
                            {
                                var answerGroup = answerGroups[j];
                                var answers = (answerGroup as Dictionary<string, object>);

                                for (int k = 0; k < auditTrackerQuestions.Count; k++)
                                {
                                    if (IsTrackerComplianceQuestionAnswered(k, auditTrackerQuestions, answers))
                                    {
                                        totalComplianceQuestionsAnswered++;
                                    }
                                }
                            }
                        }


                        for (int j = 0; j < answerGroups.Count; j++)
                        {
                            currentLine++;

                            ws.Cell(currentLine, 1).Value = (j + 1).ToString();

                            var answerGroup = answerGroups[j];
                            var answers = (answerGroup as Dictionary<string, object>);

                            if (hasCompliance)
                            {
                                var complianceCount = GetTrackerGroupComplianceCount(answers, auditTrackerQuestions);

                                var tbodyCell_TotalCompliance = ws.Cell(currentLine, 2);
                                tbodyCell_TotalCompliance.Value = $"{Math.Round(complianceCount / totalComplianceQuestions * 100, 2)}% ";
                                tbodyCell_TotalCompliance.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            }

                            int modifyBodyCell = hasCompliance ? 3 : 2;
                            for (int k = startIndex; k < endIndex; k++)
                            {
                                var trackerAnswer = answers[auditTrackerQuestions[k].Id.ToString()] as TrackerAnswerDto;

                                var tbodyCell = ws.Cell(currentLine, modifyBodyCell);

                                if (trackerAnswer != null && trackerAnswer.FormattedAnswer != null)
                                {
                                    string textColor = trackerAnswer.FormattedAnswer.ToLower() == "no" ? "red" : "black";
                                    tbodyCell.Value = trackerAnswer.FormattedAnswer;
                                    tbodyCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    if (textColor == "red")
                                    {
                                        tbodyCell.Style.Font.FontColor = XLColor.Red;
                                    }
                                    modifyBodyCell++;
                                }
                                else
                                {
                                    tbodyCell.Value = "";
                                    modifyBodyCell++;
                                }
                            }

                            if (hasCompliance)
                            {
                                var tbodyCellComplianceQuestions = ws.Cell(currentLine, columnCount);
                                tbodyCellComplianceQuestions.Value = $"{totalComplianceQuestions}";
                                tbodyCellComplianceQuestions.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            }
                        }

                    }

                }

                var rangeAllContent = ws.Range(ws.Cell(1, 1), ws.Cell(currentLine, columnCount));
                rangeAllContent.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                rangeAllContent.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                rangeAllContent.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                rangeAllContent.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                rangeAllContent.Style.Alignment.SetWrapText(true);

                ws.Columns().AdjustToContents();

                if (tblHeaderLine > 0)
                {
                    for (int i = 1; i <= columnCount; i++)
                    {
                        string cellVal = ws.Cell(tblHeaderLine, i).Value.ToString();
                        if (cellVal.Length > 50)
                        {
                            ws.Column(i).Width = 50;
                        }
                    }
                }
                currentLine++;
            }
        }

        private void ProcessCriteriaExcelAsync(XLWorkbook workbook, params CriteriaAuditDetailsDto[] audits)
        {
            if (!audits.Any())
            {
                return;
            }

            int currentLine = 1;
            int tblHeaderLine = 0;

            foreach (var audit in audits)
            {
                var ws = workbook.Worksheets.Add("Audit");
                int columnCount = 5;

                var range = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, columnCount));
                range.Merge();
                range.Value = audit.Form.Name;
                range.Style.Font.Bold = true;
                range.Style.Font.FontSize = 22;
                range.Style.Fill.BackgroundColor = XLColor.FromHtml("#145196");
                range.Style.Font.FontColor = XLColor.White;
                range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                currentLine++;

                var rangeOrganization = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 2));
                rangeOrganization.Merge();
                rangeOrganization.Value = "ORGANIZATION: ";

                var rangeOrganizationVal = ws.Range(ws.Cell(currentLine, 3), ws.Cell(currentLine, columnCount));
                rangeOrganizationVal.Merge();
                rangeOrganizationVal.Value = audit.Organization.Name;

                currentLine++;

                var rangeFacility = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 2));
                rangeFacility.Merge();
                rangeFacility.Value = "FACILITY: ";

                var rangeFacilityVal = ws.Range(ws.Cell(currentLine, 3), ws.Cell(currentLine, columnCount));
                rangeFacilityVal.Merge();
                rangeFacilityVal.Value = audit.Facility.Name;

                currentLine++;

                var rangeDates = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 2));
                rangeDates.Merge();
                rangeDates.Value = "DATES: ";

                var rangeDatesVal = ws.Range(ws.Cell(currentLine, 3), ws.Cell(currentLine, columnCount));
                rangeDatesVal.Merge();
                var dateFrom = audit.IncidentDateFrom?.ToString("MMM dd, yyyy");
                var dateTo = audit.IncidentDateTo?.ToString("MMM dd, yyyy");
                var dateText = dateTo == null ? dateFrom : $"{dateFrom} - {dateTo}";
                rangeDatesVal.Value = dateText;

                currentLine++;

                var rangeBuilding = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 2));
                rangeBuilding.Merge();
                rangeBuilding.Value = "BUILDING: ";

                var rangeBuildingVal = ws.Range(ws.Cell(currentLine, 3), ws.Cell(currentLine, columnCount));
                rangeBuildingVal.Merge();
                rangeBuildingVal.Value = audit.Room;

                currentLine++;

                var rangeResident = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 2));
                rangeResident.Merge();
                rangeResident.Value = "RESIDENT: ";

                var rangeResidentVal = ws.Range(ws.Cell(currentLine, 3), ws.Cell(currentLine, columnCount));
                rangeResidentVal.Merge();
                rangeResidentVal.Value = audit.Resident;

                currentLine++;

                var rangeAuditedBy = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 2));
                rangeAuditedBy.Merge();
                rangeAuditedBy.Value = "AUDITED BY: ";

                var rangeAuditedByVal = ws.Range(ws.Cell(currentLine, 3), ws.Cell(currentLine, columnCount));
                rangeAuditedByVal.Merge();
                rangeAuditedByVal.Value = audit.SubmittedByUser.FullName;

                currentLine++;

                var rangeAuditDate = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 2));
                rangeAuditDate.Merge();
                rangeAuditDate.Value = "AUDIT DATE: ";

                var rangeAuditDateVal = ws.Range(ws.Cell(currentLine, 3), ws.Cell(currentLine, columnCount));
                rangeAuditDateVal.Merge();
                rangeAuditDateVal.Value = $"{audit.SubmittedDate.AddHours(audit.Facility.TimeZoneOffset).ToString(DateTimeConstants.MM_DD_YYYY_hh_mm_tt_SLASH)} ({audit.Facility.TimeZoneShortName})";


                //Subheader additional form fields
                foreach (var subheader in audit.FormVersion.FormFields)
                {
                    if (!string.IsNullOrWhiteSpace(subheader.LabelName))
                    {
                        currentLine++;

                        var rangeFormFields = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 2));
                        rangeFormFields.Merge();
                        rangeFormFields.Value = subheader.LabelName?.ToUpper() + ": ";

                        var rangeFormFieldsVal = ws.Range(ws.Cell(currentLine, 3), ws.Cell(currentLine, columnCount));
                        rangeFormFieldsVal.Merge();
                        rangeFormFieldsVal.Value = subheader.Value != null ? subheader.Value.FormattedValue : "-";
                    }
                }

                var rangeSubHeader = ws.Range(ws.Cell(2, 1), ws.Cell(currentLine, 2));
                rangeSubHeader.Style.Font.Bold = true;
                rangeSubHeader.Style.Font.FontSize = 14;
                rangeSubHeader.Style.Fill.BackgroundColor = XLColor.FromHtml("#8EA9DB");
                rangeSubHeader.Style.Font.FontColor = XLColor.Black;

                var rangeSubHeaderVal = ws.Range(ws.Cell(2, 3), ws.Cell(currentLine, columnCount));
                rangeSubHeaderVal.Style.Font.Bold = true;
                rangeSubHeaderVal.Style.Font.FontSize = 14;
                rangeSubHeaderVal.Style.Fill.BackgroundColor = XLColor.FromHtml("#8EA9DB");
                rangeSubHeaderVal.Style.Font.FontColor = XLColor.Black;

                currentLine++;

                var totalComplianceValue = audit.TotalCompliance.HasValue ? Math.Round(audit.TotalCompliance.Value, 2) : 0;

                var rangeCompSummary = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, columnCount));
                rangeCompSummary.Merge();
                rangeCompSummary.Value = "Compliance Summary: " + $"{totalComplianceValue}%";
                rangeCompSummary.Style.Font.Bold = true;
                rangeCompSummary.Style.Font.FontSize = 12;
                rangeCompSummary.Style.Fill.BackgroundColor = XLColor.FromHtml("#8EA9DB");
                rangeCompSummary.Style.Font.FontColor = totalComplianceValue == 100 ? XLColor.Green : XLColor.Red;
                rangeCompSummary.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                foreach (var questionGroup in audit.FormVersion.QuestionGroups)
                {
                    if (questionGroup.Questions == null || !questionGroup.Questions.Any())
                    {
                        continue;
                    }

                    if (questionGroup.Name != null)
                    {
                        currentLine++;

                        var rangeQuestionGroupName = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, columnCount));
                        rangeQuestionGroupName.Merge();
                        rangeQuestionGroupName.Value = questionGroup.Name != null ? questionGroup.Name : string.Empty;
                        rangeQuestionGroupName.Style.Font.Bold = true;
                        rangeQuestionGroupName.Style.Font.FontSize = 12;
                        rangeQuestionGroupName.Style.Fill.BackgroundColor = XLColor.FromArgb(238, 236, 236);
                        rangeQuestionGroupName.Style.Font.FontColor = XLColor.FromArgb(38, 50, 55);
                        rangeQuestionGroupName.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    }

                    currentLine++;
                    tblHeaderLine = currentLine;

                    var rangeTableHeader = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, columnCount));
                    rangeTableHeader.Style.Font.Bold = true;
                    rangeTableHeader.Style.Font.FontSize = 12;
                    rangeTableHeader.Style.Fill.BackgroundColor = XLColor.FromHtml("#C9F2FF");
                    rangeTableHeader.Style.Font.FontColor = XLColor.Black;
                    rangeTableHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    rangeTableHeader.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    rangeTableHeader.Style.Alignment.SetWrapText(true);

                    ws.Cell(currentLine, 1).Value = "#";
                    ws.Range(ws.Cell(currentLine, 2), ws.Cell(currentLine, 3)).Merge().Value = "CRITERIA";
                    ws.Cell(currentLine, 4).Value = "ANSWER";
                    ws.Cell(currentLine, 5).Value = "AUDITOR COMMENT";

                    int index = 0;

                    foreach (var question in questionGroup.Questions)
                    {
                        var questionValueText = "";

                        if (question.Answer != null)
                        {
                            questionValueText = question.Answer.Value == CommonConstants.NA ? question.Answer.Value?.Insert(1, "/").ToUpper() : question.Answer.Value?.ToUpper();
                        }

                        string questionNumber = (++index).ToString();

                        currentLine++;

                        //#
                        var tbodyRowNumber = ws.Cell(currentLine, 1);
                        tbodyRowNumber.Value = questionNumber;
                        tbodyRowNumber.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        tbodyRowNumber.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        tbodyRowNumber.Style.Font.FontSize = 12;
                        tbodyRowNumber.Style.Font.FontColor = questionValueText == CommonConstants.NA_SLASH || !question.CriteriaOption.Compliance ? XLColor.FromArgb(80, 80, 80) : XLColor.FromArgb(0, 0, 0);

                        //CRITERIA
                        var tbodyCriteria = ws.Range(ws.Cell(currentLine, 2), ws.Cell(currentLine, 3)).Merge();
                        tbodyCriteria.Value = question.Value is null ? string.Empty : question.Value;
                        tbodyCriteria.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        tbodyCriteria.Style.Font.FontSize = 12;
                        tbodyCriteria.Style.Font.FontColor = questionValueText == CommonConstants.NA_SLASH || !question.CriteriaOption.Compliance ? XLColor.FromArgb(80, 80, 80) : XLColor.FromArgb(0, 0, 0);

                        //ANSWER
                        var tbodyAnswer = ws.Cell(currentLine, 4);
                        tbodyAnswer.Value = questionValueText is null ? string.Empty : questionValueText;
                        tbodyAnswer.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        tbodyAnswer.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        tbodyAnswer.Style.Font.FontSize = 12;
                        if (question.CriteriaOption.Compliance)
                        {
                            tbodyAnswer.Style.Font.FontColor = XLColor.FromArgb(80, 80, 80);
                        }

                        switch (questionValueText)
                        {
                            case CommonConstants.YES:
                                tbodyAnswer.Style.Font.FontColor = XLColor.FromArgb(0, 120, 30);
                                break;
                            case CommonConstants.NO:
                                tbodyAnswer.Style.Font.FontColor = XLColor.FromArgb(180, 80, 80);
                                break;
                            default:
                                tbodyAnswer.Style.Font.FontColor = XLColor.FromArgb(80, 80, 80);
                                break;
                        }

                        //COMMENT
                        if (question.Answer != null && question.Answer.AuditorComment != null)
                        {
                            var tbodyComment = ws.Cell(currentLine, 5);
                            tbodyComment.Value = Regex.Replace(question.Answer.AuditorComment, "<[^>]*>", string.Empty);
                            tbodyComment.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            tbodyComment.Style.Font.FontSize = 12;
                            tbodyComment.Style.Font.FontColor = questionValueText == CommonConstants.NA_SLASH || !question.CriteriaOption.Compliance ? XLColor.FromArgb(80, 80, 80) : XLColor.FromArgb(0, 0, 0);
                        }
                        else
                        {
                            var tbodyComment = ws.Cell(currentLine, 5);
                            tbodyComment.Value = "";
                            tbodyComment.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            tbodyComment.Style.Font.FontSize = 12;
                            tbodyComment.Style.Font.FontColor = questionValueText == CommonConstants.NA_SLASH || !question.CriteriaOption.Compliance ? XLColor.FromArgb(80, 80, 80) : XLColor.FromArgb(0, 0, 0);
                        }

                        //hidden column for criteria column height adjustment
                        var tbodyhidden = ws.Cell(currentLine, 6);
                        tbodyhidden.Value = question.Value is null ? string.Empty : question.Value;
                        tbodyhidden.Style.Font.FontColor = XLColor.White;


                        /**** SubQuestions ****/

                        int subQuestionIndex = 0;

                        foreach (var subQuestion in question.SubQuestions)
                        {
                            if (subQuestion.Answer == null)
                            {
                                continue;
                            }
                            var subQuestionValueText = "";
                            if (subQuestion.Answer != null)
                            {
                                subQuestionValueText = subQuestion.Answer.Value == CommonConstants.NA ? subQuestion.Answer.Value?.Insert(1, "/").ToUpper() : subQuestion.Answer.Value?.ToUpper();
                            }

                            string subQuestionNumber = $"{questionNumber}.{++subQuestionIndex}";

                            currentLine++;

                            //#
                            var tbodyRowNumberSub = ws.Cell(currentLine, 1);
                            tbodyRowNumberSub.Value = subQuestionNumber;
                            tbodyRowNumberSub.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            tbodyRowNumberSub.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            tbodyRowNumberSub.Style.Font.FontSize = 12;
                            tbodyRowNumberSub.Style.Font.FontColor = subQuestionValueText == CommonConstants.NA_SLASH || !subQuestion.CriteriaOption.Compliance ? XLColor.FromArgb(80, 80, 80) : XLColor.FromArgb(0, 0, 0);

                            //CRITERIA
                            var tbodyCriteriaSub = ws.Range(ws.Cell(currentLine, 2), ws.Cell(currentLine, 3)).Merge();
                            tbodyCriteriaSub.Value = subQuestion.Value is null ? string.Empty : subQuestion.Value;
                            tbodyCriteriaSub.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            tbodyCriteriaSub.Style.Font.FontSize = 12;
                            tbodyCriteriaSub.Style.Font.FontColor = subQuestionValueText == CommonConstants.NA_SLASH || !subQuestion.CriteriaOption.Compliance ? XLColor.FromArgb(80, 80, 80) : XLColor.FromArgb(0, 0, 0);

                            //ANSWER
                            var tbodyAnswerSub = ws.Cell(currentLine, 4);
                            tbodyAnswerSub.Value = subQuestionValueText is null ? string.Empty : subQuestionValueText;
                            tbodyAnswerSub.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            tbodyAnswerSub.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            tbodyAnswerSub.Style.Font.FontSize = 12;
                            if (subQuestion.CriteriaOption.Compliance)
                            {
                                tbodyAnswerSub.Style.Font.FontColor = XLColor.FromArgb(80, 80, 80);
                            }

                            switch (subQuestionValueText)
                            {
                                case CommonConstants.YES:
                                    tbodyAnswerSub.Style.Font.FontColor = XLColor.FromArgb(0, 120, 30);
                                    break;
                                case CommonConstants.NO:
                                    tbodyAnswerSub.Style.Font.FontColor = XLColor.FromArgb(180, 80, 80);
                                    break;
                                default:
                                    tbodyAnswerSub.Style.Font.FontColor = XLColor.FromArgb(80, 80, 80);
                                    break;
                            }

                            //COMMENT
                            if (subQuestion.Answer != null && subQuestion.Answer.AuditorComment != null)
                            {
                                var tbodyCommentSub = ws.Cell(currentLine, 5);
                                tbodyCommentSub.Value = Regex.Replace(subQuestion.Answer.AuditorComment, "<[^>]*>", string.Empty);
                                tbodyCommentSub.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                tbodyCommentSub.Style.Font.FontSize = 12;
                                tbodyCommentSub.Style.Font.FontColor = subQuestionValueText == CommonConstants.NA_SLASH || !subQuestion.CriteriaOption.Compliance ? XLColor.FromArgb(80, 80, 80) : XLColor.FromArgb(0, 0, 0);
                            }
                            else
                            {
                                var tbodyCommentSub = ws.Cell(currentLine, 5);
                                tbodyCommentSub.Value = "";
                                tbodyCommentSub.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                tbodyCommentSub.Style.Font.FontSize = 12;
                                tbodyCommentSub.Style.Font.FontColor = subQuestionValueText == CommonConstants.NA_SLASH || !subQuestion.CriteriaOption.Compliance ? XLColor.FromArgb(80, 80, 80) : XLColor.FromArgb(0, 0, 0);
                            }

                            //hidden column for criteria column height adjustment
                            var tbodyhidden2 = ws.Cell(currentLine, 6);
                            tbodyhidden2.Value = subQuestion.Value is null ? string.Empty : subQuestion.Value;
                            tbodyhidden2.Style.Font.FontColor = XLColor.White;
                        }

                    }

                }

                var rangeAllContent = ws.Range(ws.Cell(1, 1), ws.Cell(currentLine, columnCount + 1));
                rangeAllContent.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                rangeAllContent.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                rangeAllContent.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                rangeAllContent.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                rangeAllContent.Style.Alignment.SetWrapText(true);

                ws.Columns().AdjustToContents();

                ws.Column(1).Width = 8;
                ws.Column(2).Width = 30;
                ws.Column(3).Width = 50;
                ws.Column(4).Width = 15;
                ws.Column(5).Width = 60;

                ws.Column(6).Width = 80;
                ws.Column(6).Hide(); //hides dummy column

                currentLine += 2;
            }

        }

    }
}
