using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.BusinessLogic.DTO.Common;
using SHARP.BusinessLogic.DTO.Form;
using SHARP.BusinessLogic.DTO.ProgressNote;
using SHARP.BusinessLogic.DTO.UserActivity;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Constants;
using SHARP.Common.Enums;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.Common.Models;
using SHARP.DAL.Models;
using SHARP.Filters;
using SHARP.Http;
using SHARP.ViewModels;
using SHARP.ViewModels.Audit;
using SHARP.ViewModels.Common;
using SHARP.ViewModels.Form;
using SHARP.ViewModels.ProgressNote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace SHARP.Controllers
{
    [Route("api/audits")]
    [ApiController]
    [Authorize]
    public class AuditsController : BaseController
    {
        private readonly IAuditService _auditService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IFormService _formService;
        private readonly IHighAlertService _highAlertService;
        private readonly AppConfig _appConfig;

        public AuditsController(
            IAuditService auditService,
            IMapper mapper,
            UserManager<ApplicationUser> userManager, 
            IUserInfoService userInfoService,
            IUserService userService,
            IFormService formService, IHighAlertService highAlertService,
            AppConfig appConfig) : base(userManager, userInfoService)
        {
            _auditService = auditService;
            _mapper = mapper;
            _userService = userService;
            _formService = formService;
            _appConfig = appConfig;
            _highAlertService = highAlertService;
        }

        [Route("get")]
        [HttpPost]
        public async Task<IActionResult> GetAudits([FromBody]AuditFilterModel auditFilter)
        {
            _appConfig.Application[CommonConstants.USER_TIME_ZONE] = _userService.GetCurrentUserTimeZone();
            
            var filter = _mapper.Map<AuditFilter>(auditFilter);

            Tuple<IReadOnlyCollection<AuditDto>, int> tuple = await _auditService.GetAuditsAsync(filter);
            IReadOnlyCollection<AuditDto> audits = tuple.Item1;

            IReadOnlyCollection<AuditGridItemModel> result = _mapper.Map<IReadOnlyCollection<AuditGridItemModel>>(audits);

            
            return Ok(new { items = result, totalCount = tuple.Item2 });
        }

        [Route("getHighAlertCategories")]
        [HttpGet]
        public async Task<IActionResult> GetHighAlertCategories()
        {
            var categories = await _highAlertService.GetHighAlertCategories();
            return Ok(categories);
        }

        [Route("filters")]
        [HttpPost]
        public async Task<IActionResult> GetFilters([FromBody]AuditFilterColumnSourceModel columnData)
        {
            var columnFilter = _mapper.Map<AuditFilterColumnSource<AuditFilterColumn>>(columnData);

            IReadOnlyCollection<FilterOption> optionsDto = await _auditService.GetFilterColumnSourceDataAsync(columnFilter);

            var options = _mapper.Map<IReadOnlyCollection<FilterOptionModel>>(optionsDto);

            return Ok(options);
        }

        [Route("{id:int}")]
        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> SetAuditStatus(int id, [FromBody] AuditStatusRequestModel requestModel)
        {
            AuditDto auditDto = await _auditService.SetAuditStatusAsync(id, requestModel.Status, requestModel.Comment, requestModel.ReportType);

            AuditDetailsModel auditDetails = MapAuditDetails(auditDto);

            return Ok(auditDetails);
        }
        [Route("{id:int}/isAuditTriggedByKeyword")]
        [HttpGet]
        public async Task<IActionResult> GetIsAuditTriggedByKeyword(int id)
        {
            var allow = await _auditService.IsAuditTriggedByKeyword(id);

            return Ok(allow);
        }

        [Route("{id:int}")]
        [HttpGet]
        public Task<IActionResult> GetAudit(int id)
        {
            return GetJsonAuditAsync(id);
        }

        [Route("{id:int}/residents")]
        [HttpGet]
        public async Task<IActionResult> GetResidents(int id)
        {
            AuditDto auditDto = await _auditService.GetAuditAsync(id);
            List<ResidentDto> residents = await _auditService.GetResidents(auditDto.Facility.Id);

            return Ok(residents);

        }

        [Route("organizations")]
        [HttpGet]
        public async Task<IActionResult> GetOrganizationOptions()
        {
            IReadOnlyCollection<OptionDto> organizationsDto = await _auditService.GetOrganizationOptionsAsync();

            var organizationOptions = _mapper.Map <IReadOnlyCollection<OptionModel>>(organizationsDto);

            return Ok(organizationOptions);
        }

        [HttpPost]
        [ValidateModel]
        [Authorize(Roles = "Auditor")]
        public async Task<IActionResult> AddAudit([FromBody] AuditAddEditModel audit)
        {
            var auditEditDto = _mapper.Map<AuditAddEditDto>(audit);

            AuditDto auditDto = await _auditService.AddAuditAsync(auditEditDto);

            AuditDetailsModel auditDetails = MapAuditDetails(auditDto);

            return Ok(auditDetails);
        }

        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> EditAudit([FromBody] AuditAddEditModel audit)
        {
            var auditEditDto = _mapper.Map<AuditAddEditDto>(audit);

            AuditDto auditDto = await _auditService.EditAuditAsync(auditEditDto);

            AuditDetailsModel auditDetails = MapAuditDetails(auditDto);

            return Ok(auditDetails);
        }

        [Route("form/{formVersionId:int}/keywords")]
        [HttpGet]
        public async Task<IActionResult> GetKeywordOptions(int formVersionId)
        {
            IReadOnlyCollection<OptionDto> keywordOptionsDto = await _auditService.GetKeywordOptionsAsync(formVersionId);

            var keywordOptions = _mapper.Map<IReadOnlyCollection<OptionModel>>(keywordOptionsDto);

            return Ok(keywordOptions);
        }

        [Route("{auditId:int}/facility/{facilityId:int}/progressNotes")]
        [HttpGet]
        public async Task<IActionResult> GetProgresNotes(int auditId, int facilityId, [FromQuery] AuditProgressNoteFilterModel filterModel)
        {
            var filter = _mapper.Map<AuditProgressNoteFilter>(filterModel);

            filter.AuditId = auditId;
            filter.FacilityId = facilityId;

            var facility = await _auditService.GetFacilityAsync(facilityId);

            IReadOnlyCollection<ProgressNoteDto> progressNotesDto = await _auditService.GetProgresNotesAsync(filter);

            ProgressNoteDetailsModel progressNoteDetails = new ProgressNoteDetailsModel() {
                ProgressNotes = _mapper.Map<IReadOnlyCollection<ProgressNoteModel>>(progressNotesDto),
                TimeZoneOffset = facility.TimeZoneOffset
            };

            return Ok(progressNoteDetails);
        }

        [Route("keyword")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> AddAuditKeyword([FromBody] AddKeywordModel keyword)
        {

            if (keyword.CustomKeyword!=null && keyword.CustomKeyword.Length > 0)
            {
                AddFormKeywordDto addFormKeywordDto = new AddFormKeywordDto()
                {
                    FormVersionId = keyword.FormVersionId,
                    Name = keyword.CustomKeyword,
                    Hidden = 1
                };

                OptionDto keywordDto = await _formService.AddFormKeywordAsync(addFormKeywordDto);
                keyword.KeywordId = keywordDto.Id;
            }

            var auditKeywordDto = _mapper.Map<AddKeywordDto>(keyword);

            KeywordDto auditValueDto = await _auditService.AddAuditKeywordAsync(auditKeywordDto);

            KeywordModel auditValue = _mapper.Map<KeywordModel>(auditValueDto);

            return Ok(auditValue);
        }

        [Route("keyword")]
        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> EditAuditKeyword([FromBody] EditKeywordModel keyword)
        {
            var auditKeywordDto = _mapper.Map<EditKeywordDto>(keyword);

            KeywordDto auditValueDto = await _auditService.EditAuditKeywordAsync(auditKeywordDto);

            KeywordModel auditValue = _mapper.Map<KeywordModel>(auditValueDto);

            return Ok(auditValue);
        }

        [Route("keyword/{keywordValueId:int}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAuditKeyword(int keywordValueId)
        {
            await _auditService.DeleteAuditKeywordAsync(keywordValueId);

            return Ok(true);
        }

        [Route("{auditId:int}/tracker")]
        [HttpPost]
        public async Task<IActionResult> AddAuditTrackerAnswers(int auditId, [FromBody] AddEditTrackerAnswersModel trackerAnswer)
        {
            var addAuditTrackerAnswersDto = _mapper.Map<IReadOnlyCollection<AddEditTrackerAnswerDto>>(trackerAnswer.Answers);
            var highAlertCategory = _mapper.Map<OptionDto>(trackerAnswer.HighAlertCategory);
            AuditDto auditDto = await _auditService.AddAuditTrackerAnswersAsync(auditId, addAuditTrackerAnswersDto,highAlertCategory, trackerAnswer.HighAlertDescription, trackerAnswer.HighAlertNotes);

            return Ok(MapAuditDetails(auditDto));
        }

        [Route("{auditId:int}/tracker/{groupId}")]
        [HttpPut]
        public async Task<IActionResult> EditAuditTrackerAnswers(int auditId, string groupId, [FromBody] AddEditTrackerAnswersModel trackerAnswer)
        {
            var addAuditTrackerAnswersDto = _mapper.Map<IReadOnlyCollection<AddEditTrackerAnswerDto>>(trackerAnswer.Answers);

            var highAlertCategory = _mapper.Map<OptionDto>(trackerAnswer.HighAlertCategory);
            AuditDto auditDto = await _auditService.EditAuditTrackerAnswersAsync(auditId, groupId, addAuditTrackerAnswersDto, highAlertCategory, trackerAnswer.HighAlertDescription, trackerAnswer.HighAlertNotes);

            return Ok(MapAuditDetails(auditDto));
        }

        [Route("{auditId:int}/tracker/{answersGroupId}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAuditTrackerAnswers(int auditId, string answersGroupId)
        {
            await _auditService.DeleteAuditTrackerAnswersAsync(auditId, answersGroupId);

            return Ok(true);
        }

        [Route("{auditId:int}/tracker/sort")]
        [HttpPost]
        public async Task<IActionResult> SortAuditTrackerAnswers(int auditId, [FromBody] SortModel sortModel)
        {
            AuditDto auditDto = await _auditService.SortAuditTrackerAnswersAsync(auditId, sortModel);

             return Ok(MapAuditDetails(auditDto));
        }

        [Route("{id:int}/pdf/get")]
        [HttpGet]
        [Authorize(Roles = "Reviewer, Admin")]
        public async Task<IActionResult> GetSingleAuditPdf(int id)
        {

            var pdf = await _auditService.GetAuditPdfAsync(id);

            Response.Headers.Add(HeaderNames.ContentDisposition, DispositionTypeNames.Attachment);
            return File(pdf, MediaTypeNames.Application.Pdf);
        }

        [Route("{auditId:int}/duplicate")]
        [HttpPost]
        public async Task<IActionResult> DuplicateAudit(int auditId, [FromBody] DuplicateAuditModel model)
        {
            int newAuditId = await _auditService.DuplicateAuditAsync(auditId, model.CurrentClientDate);

            return Ok(newAuditId);
        }

        [Authorize(Roles = "Admin,Reviewer")]
        [Route("{auditId:int}/archive")]
        [HttpPost]
        public async Task<IActionResult> ArchiveAudit(int auditId)
        {
            bool result = await ChangeState(auditId, AuditState.Archived);

            if (result)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("You don't have permissions to perform this action");
            }
        }

        [Authorize(Roles = "Admin,Reviewer,Auditor")]
        [Route("{auditId:int}/delete")]
        [HttpPost]
        public async Task<IActionResult> DeleteAudit(int auditId)
        {
            bool result = await ChangeState(auditId, AuditState.Deleted);

            if (result)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("You don't have permissions to perform this action");
            }
        }
        [Authorize(Roles = "Admin,Reviewer")]
        [Route("deleteAudits")]
        [HttpPost]
        public async Task<IActionResult> DeleteAudits([FromBody] AuditsSelectedModel model)
        {
            IList<int> results = new List<int>();
            foreach ( var auditId in model.AuditIds)
            {
                AddUserActivityDto activeDto = new AddUserActivityDto();
                activeDto.AuditId = auditId;
                activeDto.UserId = _userService.GetLoggedUserId();
                activeDto.ActionType = ActionType.DeleteAudit;
                activeDto.UserAgent = Request.GetUserAgent();
                activeDto.IP = Request.GetIP().ToString();

                await _userService.AddUserActivityAsync(activeDto);

                bool result = await ChangeState(auditId, AuditState.Deleted);
                if(result)
                    results.Add(auditId);
            }
            if (results.Count() == model.AuditIds.Count())
            {
                return Ok(results);
            }
            else
            {
                return BadRequest("You don't have permissions to perform this action or something wrong");
            }

        }


        [Authorize(Roles = "Admin,Reviewer")]
        [Route("archiveAudits")]
        [HttpPost]
        public async Task<IActionResult> ArchiveAudits([FromBody] AuditsSelectedModel model)
        {
            IList<int> results = new List<int>();
            foreach (var auditId in model.AuditIds)
            {
                AddUserActivityDto activeDto = new AddUserActivityDto();
                activeDto.AuditId = auditId;
                activeDto.UserId = _userService.GetLoggedUserId();
                activeDto.ActionType = ActionType.ArchiveAudit;
                activeDto.UserAgent = Request.GetUserAgent();
                activeDto.IP = Request.GetIP().ToString();

                await _userService.AddUserActivityAsync(activeDto);

                bool result = await ChangeState(auditId, AuditState.Archived);
                if (result)
                    results.Add(auditId);
            }
            if (results.Count() == model.AuditIds.Count())
            {
                return Ok(results);
            }
            else
            {
                return BadRequest("You don't have permissions to perform this action or something wrong");
            }

        }
        private async Task<bool> ChangeState(int auditId, AuditState state)
        {
            bool result = false;
            if (await IsUserInRole(UserRoles.Admin))
            {
                result = await _auditService.ChangeAuditState(auditId, state);
            }
            else if (await IsUserInRole(UserRoles.Reviewer))
            {
                var organizationIds = await _userService.GetUserOrganizationIdsAsync();
                AuditDto audit = await _auditService.GetAuditAsync(auditId);

                if (audit != null && organizationIds != null && organizationIds.Contains(audit.Organization.Id))
                {
                    result = await _auditService.ChangeAuditState(auditId, state);
                }
            }else if (state ==  AuditState.Deleted)
            {
               
                var allow = await _auditService.IsAuditTriggedByKeyword(auditId);

                if (!allow)
                    return allow;

                if (allow)
                {
                    result = await _auditService.ChangeAuditState(auditId, state);
                }

            }
            return result;
        }

        [Authorize(Roles = "Admin,Reviewer")]
        [Route("{auditId:int}/unarchive")]
        [HttpPost]
        public async Task<IActionResult> Unarchive(int auditId)
        {
            bool result = await ChangeState(auditId, AuditState.Active);

            if (result)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("You don't have permissions to perform this action");
            }
        }

        [Authorize(Roles = "Admin")]
        [Route("{auditId:int}/undelete")]
        [HttpPost]
        public async Task<IActionResult> Undelete(int auditId)
        {
            bool result = await ChangeState(auditId, AuditState.Active);

            if (result)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("You don't have permissions to perform this action");
            }
        }

        [Route("{auditId:int}/duplicate")]
        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> UpdateDuplicatedAudit([FromBody] AuditAddEditModel audit)
        {
            var auditEditDto = _mapper.Map<AuditAddEditDto>(audit);

            AuditDto auditDto = await _auditService.UpdateDuplicatedAuditAsync(auditEditDto);

            AuditDetailsModel auditDetails = MapAuditDetails(auditDto);

            return Ok(auditDetails);
        }

        private async Task<IActionResult> GetJsonAuditAsync(int id)
        {
            AuditDto auditDto = await _auditService.GetAuditAsync(id);

            AuditDetailsModel auditDetails = MapAuditDetails(auditDto);

            return Ok(auditDetails);
        }

        private AuditDetailsModel MapAuditDetails(AuditDto auditDto)
        {
            if(auditDto == null)
            {
                return null;
            }

            _appConfig.Application[CommonConstants.USER_TIME_ZONE] = _userService.GetCurrentUserTimeZone();

            switch (auditDto.Form.AuditType.Name)
            {
                case CommonConstants.TWENTY_FOUR_HOUR_KEYWORD:
                    return _mapper.Map<HourKeywordAuditDetailsModel>(auditDto);

                case CommonConstants.CRITERIA:
                    return _mapper.Map<CriteriaAuditDetailsModel>(auditDto);

                case CommonConstants.TRACKER:
                    return _mapper.Map<TrackerAuditDetailsModel>(auditDto);
                
                case CommonConstants.MDS:
                    return _mapper.Map<MdsAuditDetailsModel>(auditDto);

                default:
                    return _mapper.Map<AuditDetailsModel>(auditDto);
            }
        }

        [Route("{id:int}/excel/get")]
        [HttpGet]
        [Authorize(Roles = "Reviewer, Admin")]
        public async Task<IActionResult> GetSingleAuditExcel(int id)
        {

            var excel = await _auditService.GetAuditExcelAsync(id);

            Response.Headers.Add(HeaderNames.ContentDisposition, DispositionTypeNames.Attachment);
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
    }
}
