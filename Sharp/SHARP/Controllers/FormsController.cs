using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.Common;
using SHARP.BusinessLogic.DTO.Form;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Constants;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.Common.Models;
using SHARP.DAL.Models;
using SHARP.Filters;
using SHARP.ViewModels;
using SHARP.ViewModels.Common;
using SHARP.ViewModels.Form;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SHARP.Controllers
{
    [Route("api/forms/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class FormsAdminController : Controller
    {
        private readonly IFormService _formService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly AppConfig _appConfig;

        public FormsAdminController(
            IFormService formService, 
            IMapper mapper, 
            IUserService userService, 
            AppConfig appConfig)
        {
            _formService = formService;
            _mapper = mapper;
            _userService = userService;
            _appConfig = appConfig;
        }

        [Route("versions/get")]
        [HttpPost]
        public async Task<IActionResult> GetFormVersions([FromBody] FormVersionFilterModel formFiler)
        {
            _appConfig.Application[CommonConstants.USER_TIME_ZONE] = _userService.GetCurrentUserTimeZone();

            var filter = _mapper.Map<FormVersionFilter>(formFiler);

            var forms = await _formService.GetFormVersionsAsync(filter);

            var result = _mapper.Map<IReadOnlyCollection<FormVersionGridItemModel>>(forms);

            return Ok(result);
        }

        [Route("versions/filters")]
        [HttpPost]
        public async Task<IActionResult> GetFilters([FromBody] FormManagementFilterColumnSourceModel columnData)
        {
            var columnFilter = _mapper.Map<FormManagementFilterColumnSource<FormVersionColumn>>(columnData);

            IReadOnlyCollection<FilterOption> optionsDto = await _formService.GetFilterColumnSourceDataAsync(columnFilter);

            var options = _mapper.Map<IReadOnlyCollection<FilterOptionModel>>(optionsDto);

            return Ok(options);
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> AddForm([FromBody] AddFormModel addForm)
        {
            var formAddDto = _mapper.Map<AddFormDto>(addForm);

            FormVersionDto formDto = await _formService.AddFormAsync(formAddDto);

            _appConfig.Application[CommonConstants.USER_TIME_ZONE] = _userService.GetCurrentUserTimeZone();

            FormVersionModel form = _mapper.Map<FormVersionModel>(formDto);

            return Ok(form);
        }

        [Route("form/{formId:int}")]
        [HttpPost]
        public async Task<IActionResult> EditForm(int formId, EditFormModel editForm)
        {
            if (_formService.FormExists(formId, editForm.FormName))
                throw new Exception("Form Name must be unique");

            await _formService.EditFormNameAsync(formId, editForm.FormName);

            return Ok(new { editForm.FormName });
        }

        [Route("form/{formId:int}/duplicate/{organizationId:int}")]
        [HttpPost]
        public async Task<IActionResult> DuplicateForm(int formId, int organizationId)
        {
            
            await _formService.DuplicateFormAsync(formId, organizationId);

            return Ok(new {  });
        }

        [Route("{formVersionId:int}")]
        [HttpPost]
        public async Task<IActionResult> EditFormVersion(int formVersionId)
        {
            FormVersionDto formDto = await _formService.EditFormVersionAsync(formVersionId);

            FormDetailsModel formDetails = MapAuditDetails(formDto);

            return Ok(formDetails);
        }


        [Route("publish/{formVersionId:int}/{allowEmptyComment:int}/{disableCompliance:int}/{useHighAlert:bool}/{ahTime:int}")]
        [HttpPut]
        public async Task<IActionResult> PublishFormVersion(int formVersionId, int allowEmptyComment, int disableCompliance,bool useHighAlert,int ahTime)
        {
            await _formService.PublishFormVersionAsync(formVersionId, allowEmptyComment, disableCompliance, useHighAlert, ahTime);

            return Ok(true);
        }
        [Route("formsTriggered/{organizationId:int}/{keyword}")]
        [HttpGet]
        public async Task<IActionResult> GetFormsTriggered(int organizationId,string keyword)
        {
            var forms = await _formService.GetFormsTriggeredAsync(organizationId, keyword);

            var formsOptions = _mapper.Map<IReadOnlyCollection<OptionModel>>(forms);

            return Ok(formsOptions);
        }
        [Route("{id:int}")]
        [HttpDelete]
        public async Task<IActionResult> DeletetForm(int id)
        {
            FormVersionDto formDto = await _formService.DeleteFormVersionAsync(id);

            FormDetailsModel formDetails = MapAuditDetails(formDto);

            return Ok(formDetails);
        }

        [Route("keyword")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> AddFormKeyword([FromBody] AddFormKeywordModel addKeyword)
        {
            var addKeywordDto = _mapper.Map<AddFormKeywordDto>(addKeyword);

            KeywordOptionDto keywordDto = await _formService.AddFormKeywordAsync(addKeywordDto);

            var keyword = _mapper.Map<KeywordOptionModel>(keywordDto);

            return Ok(keyword);
        }

        [Route("keyword/{id:int}")]
        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> EditFormKeyword(int id, [FromBody] EditFormKeywordModel editKeyword)
        {
            var editKeywordDto = _mapper.Map<EditFormKeywordDto>(editKeyword);
            editKeywordDto.Id = id;

            bool result = await _formService.EditFormKeywordAsync(editKeywordDto);

            return Ok(result);
        }

        [Route("column/{id:int}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteColumn(int id)
        {
            FormVersionDto formDto = await _formService.DeleteQuestionAsync(id);

            FormDetailsModel formDetails = MapAuditDetails(formDto);

            return Ok(formDetails);
        }
        [Route("columnkeyword/{id:int}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteColumnKeyword(int id)
        {
            FormVersionDto formDto = await _formService.DeleteTableColumnAsync(id);

            FormDetailsModel formDetails = MapAuditDetails(formDto);

            return Ok(formDetails);
        }
        [Route("questions/rearrange")]
        [HttpPut]
        public async Task<IActionResult> RearrangeQuestions([FromBody] RearrangeQuestionsModel model)
        {
            var rearrangeQuestionsDto = _mapper.Map<RearrangeQuestionsDto>(model);

            bool result = await _formService.RearrangeQuestionsAsync(rearrangeQuestionsDto);

            return Ok(result);
        }

        [Route("{formVersionId:int}/sections")]
        [HttpGet]
        public async Task<IActionResult> GetSectionOptions(int formVersionId)
        {
            IReadOnlyCollection<OptionDto> sectionsDto = await _formService.GetSectionOptionsAsync(formVersionId);

            var sectionOptions = _mapper.Map<IReadOnlyCollection<OptionModel>>(sectionsDto);

            return Ok(sectionOptions);
        }

        [Route("{formVersionId:int}/mdssections")]
        [HttpGet]
        public async Task<IActionResult> GetMdsSections(int formVersionId)
        {
            IReadOnlyCollection<MdsSectionDto> sectionsDto = await _formService.GetMdsSectionsAsync(formVersionId);

            var sectionOptions = _mapper.Map<IReadOnlyCollection<MdsSectionModel>>(sectionsDto);

            return Ok(sectionOptions);
        }

        [Route("field/types")]
        [HttpGet]
        public async Task<IActionResult> GetFieldTypeOptions()
        {
            IReadOnlyCollection<OptionDto> fieldTypesDto = await _formService.GetFieldTypeOptionsAsync();

            var fieldTypeOptions = _mapper.Map<IReadOnlyCollection<OptionModel>>(fieldTypesDto);

            return Ok(fieldTypeOptions);
        }

        [Route("criteria/question")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> AddQuestion([FromBody] AddCriteriaQuestionModel addQuestion)
        {
            var addQuestionDto = _mapper.Map<AddCriteriaQuestionDto>(addQuestion);

            FormVersionDto formDto = await _formService.AddCriteriaQuestionAsync(addQuestionDto);

            FormDetailsModel formDetails = MapAuditDetails(formDto);

            return Ok(formDetails);
        }

        [Route("mds/section")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> AddSection([FromBody] AddMdsSectionModel addSection)
        {
            var addSectionDto = _mapper.Map<AddMdsSectionDto>(addSection);

            FormVersionDto formDto = await _formService.AddMdsSectionAsync(addSectionDto);

            FormDetailsModel formDetails = MapAuditDetails(formDto);

            return Ok(formDetails);
        }
        
        [Route("mds/section")]
        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> EditSection([FromBody] EditMdsSectionModel editMdsSection)
        {
            var editSectionDto = _mapper.Map<EditMdsSectionDto>(editMdsSection);

            FormVersionDto formDto = await _formService.EditMdsSectionAsync(editSectionDto);

            FormDetailsModel formDetails = MapAuditDetails(formDto);

            return Ok(formDetails);
        }

        [Route("mds/group")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> AddGroup([FromBody] AddMdsGroupModel addGroup)
        {
            var addGroupDto = _mapper.Map<AddMdsGroupDto>(addGroup);

            FormVersionDto formDto = await _formService.AddMdsGroupAsync(addGroupDto);

            FormDetailsModel formDetails = MapAuditDetails(formDto);

            return Ok(formDetails);
        }
        
        [Route("mds/group")]
        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> EditGroup([FromBody] EditMdsGroupModel editMdsGroupModel)
        {
            var editGroupDto = _mapper.Map<EditMdsGroupDto>(editMdsGroupModel);

            FormVersionDto formDto = await _formService.EditMdsGroupAsync(editGroupDto);

            FormDetailsModel formDetails = MapAuditDetails(formDto);

            return Ok(formDetails);
        }

        [Route("criteria/question")]
        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> EditQuestion([FromBody] EditCriteriaQuestionModel editQuestion)
        {
            var editQuestionDto = _mapper.Map<EditCriteriaQuestionDto>(editQuestion);

            FormVersionDto formDto = await _formService.EditCriteriaQuestionAsync(editQuestionDto);

            FormDetailsModel formDetails = MapAuditDetails(formDto);

            return Ok(formDetails);
        }

        [Route("tracker/question")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> AddTrackerQuestion([FromBody] AddTrackerQuestionModel addQuestion)
        {
            var addQuestionDto = _mapper.Map<AddTrackerQuestionDto>(addQuestion);

            FormVersionDto formDto = await _formService.AddTrackerQuestionAsync(addQuestionDto);

            FormDetailsModel formDetails = MapAuditDetails(formDto);

            return Ok(formDetails);
        }

        [Route("mds/question")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> AddMdsQuestion([FromBody] AddTrackerQuestionModel addQuestion)
        {
            var addQuestionDto = _mapper.Map<AddTrackerQuestionDto>(addQuestion);

            FormVersionDto formDto = await _formService.AddMdsQuestionAsync(addQuestionDto);

            FormDetailsModel formDetails = MapAuditDetails(formDto);

            return Ok(formDetails);
        }
        
        [Route("mds/question")]
        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> EditMdsQuestion([FromBody] EditTrackerQuestionModel editTrackerQuestionModel)
        {
            var editQuestionDto = _mapper.Map<EditTrackerQuestionDto>(editTrackerQuestionModel);

            FormVersionDto formDto = await _formService.EditMdsQuestionAsync(editQuestionDto);

            FormDetailsModel formDetails = MapAuditDetails(formDto);

            return Ok(formDetails);
        }

        [Route("tracker/question")]
        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> EditTrackerQuestion([FromBody] EditTrackerQuestionModel editQuestion)
        {
            var editQuestionDto = _mapper.Map<EditTrackerQuestionDto>(editQuestion);

            FormVersionDto formDto = await _formService.EditTrackerQuestionAsync(editQuestionDto);

            FormDetailsModel formDetails = MapAuditDetails(formDto);

            return Ok(formDetails);
        }

        [Route("fields")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> AddFormField([FromBody] AddFormFieldModel addFormField)
        {
            var addFormFieldDto = _mapper.Map<AddFormFieldDto>(addFormField);

            FormVersionDto formDto = await _formService.AddFormFieldAsync(addFormFieldDto);

            FormDetailsModel formDetails = MapAuditDetails(formDto);

            return Ok(formDetails);
        }

        [Route("fields")]
        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> EditFormField([FromBody] EditFormFieldModel editFormField)
        {
            var editFormFieldDto = _mapper.Map<EditFormFieldDto>(editFormField);

            FormVersionDto formDto = await _formService.EditFormFieldAsync(editFormFieldDto);

            FormDetailsModel formDetails = MapAuditDetails(formDto);

            return Ok(formDetails);
        }

        [Route("{formVersionId:int}/fields/{fieldId:int}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteFormField(int formVersionId, int fieldId)
        {
            FormVersionDto formDto = await _formService.DeleteFormFieldAsync(formVersionId, fieldId);

            FormDetailsModel formDetails = MapAuditDetails(formDto);

            return Ok(formDetails);
        }

        [Route("{formVersionId:int}/fields/rearrange")]
        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> RearrangeFormFields(int formVersionId, [FromBody] RearrangeItemsModel model)
        {
            var rearrangeItemsDto = _mapper.Map<RearrangeItemsDto>(model);

            bool result = await _formService.RearrangeFormFieldsAsync(formVersionId, rearrangeItemsDto);

            return Ok(result);
        }

        [Route("{formVersionId:int}/sections")]
        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> EditFormSections(int formVersionId, [FromBody] EditQuestionGroupsModel model)
        {
            var editQuestionGroupsDto = _mapper.Map<EditQuestionGroupsDto>(model);

            FormVersionDto formDto = await _formService.EditFormSectionsAsync(formVersionId, editQuestionGroupsDto);

            FormDetailsModel formDetails = MapAuditDetails(formDto);

            return Ok(formDetails);
        }

        [Route("{formId:int}/state")]
        [HttpPut]
        public async Task<IActionResult> SetState(int formId, [FromBody] EditFormStateModel editFormStateModel)
        {
            bool result = await _formService.SetFormStateAsync(formId, editFormStateModel.State);

            return Ok(result);
        }

        [Route("{formId:int}/last/version")]
        [HttpGet]
        public async Task<IActionResult> GetLastActiveFormVersionId(int formId)
        {
            int versionId = await _formService.GetLastActiveFormVersionIdAsync(formId);

            return Ok(versionId);
        }

        private FormDetailsModel MapAuditDetails(FormVersionDto formDto)
        {
            if (formDto == null)
            {
                return null;
            }

            _appConfig.Application[CommonConstants.USER_TIME_ZONE] = _userService.GetCurrentUserTimeZone();

            switch (formDto.Form.AuditType.Name)
            {
                case CommonConstants.TWENTY_FOUR_HOUR_KEYWORD:
                    return _mapper.Map<KeywordFormDetailsModel>(formDto);

                case CommonConstants.CRITERIA:
                    return _mapper.Map<CriteriaFormDetailsModel>(formDto);

                case CommonConstants.TRACKER:
                    return _mapper.Map<TrackerFormDetailsModel>(formDto);

                case CommonConstants.MDS:
                    return _mapper.Map<MdsFormDetailsModel>(formDto);

                default:
                    return _mapper.Map<FormDetailsModel>(formDto);
            }
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class FormsController : Controller
    {
        private readonly IFormService _formService;
        private readonly IMapper _mapper;

        public FormsController(IFormService formService, IMapper mapper)
        {
            _formService = formService;
            _mapper = mapper;
        }

        [Route("{id:int}")]
        [HttpGet]
        public async Task<IActionResult> GetForm(int id)
        {
            FormVersionDto formDto = await _formService.GetFormVersionDetailsAsyncForManagement(id);

            FormDetailsModel formDetails = MapAuditDetails(formDto);

            return Ok(formDetails);
        }

        [Route("{id:int}/versions")]
        [HttpGet]
        public async Task<IActionResult> GetFormVersions(int id)
        {
            FormVersionDto formDto = await _formService.GetFormVersionDetailsAsync(id);

            return Ok(await _formService.GetFormVersionsAsyncByFormId(formDto.Form.Id));
        }

        [Route("types")]
        [HttpGet]
        public async Task<IActionResult> GetTypeOptions()
        {
            IReadOnlyCollection<OptionDto> typesDto = await _formService.GetTypeOptionsAsync();

            var typeOptions = _mapper.Map<IReadOnlyCollection<OptionModel>>(typesDto);

            return Ok(typeOptions);
        }

        [Route("organization/{organizationId:int}/versions")]
        [HttpGet]
        public async Task<IActionResult> GetFormVersionOptions(int organizationId, string auditType)
        {
            IReadOnlyCollection<FormOptionDto> formsDto = await _formService.GetFormVersionOptionsAsync(organizationId, auditType);

            var formOptions = _mapper.Map<IReadOnlyCollection<FormOptionModel>>(formsDto);

            return Ok(formOptions);
        }

        [Route("organization/{organizationId:int}")]
        [HttpGet]
        public async Task<IActionResult> GetFormOptions(int organizationId, string auditType)
        {
            IReadOnlyCollection<FormOptionDto> formsDto = await _formService.GetFormOptionsAsync(organizationId, auditType);

            var formOptions = _mapper.Map<IReadOnlyCollection<FormOptionModel>>(formsDto);

            return Ok(formOptions);
        }

        [Route("options")]
        [HttpPost]
        public async Task<IActionResult> GetFormFilteredOptions([FromBody] FormOptionFilterModel formFilter)
        {
            var filter = _mapper.Map<FormOptionFilter>(formFilter);

            IReadOnlyCollection<FormOptionDto> formsDto = await _formService.GetFormOptionsAsync(filter);

            var formOptions = _mapper.Map<IReadOnlyCollection<FormOptionModel>>(formsDto);

            return Ok(formOptions);
        }

        [Route("questions")]
        [HttpPost]
        public async Task<IActionResult> GetQuestions([FromBody] FormQuestionsFilterModel formQuestionsFilter)
        {
            var questions = await _formService.GetQuestions(formQuestionsFilter.FormVersionIds);
            return Ok(questions);
        }

        private FormDetailsModel MapAuditDetails(FormVersionDto formDto)
        {
            if (formDto == null)
            {
                return null;
            }

            switch (formDto.Form.AuditType.Name)
            {
                case CommonConstants.TWENTY_FOUR_HOUR_KEYWORD:
                    return _mapper.Map<KeywordFormDetailsModel>(formDto);

                case CommonConstants.CRITERIA:
                    return _mapper.Map<CriteriaFormDetailsModel>(formDto);

                case CommonConstants.TRACKER:
                    return _mapper.Map<TrackerFormDetailsModel>(formDto);

                case CommonConstants.MDS:
                    return _mapper.Map<MdsFormDetailsModel>(formDto);

                default:
                    return _mapper.Map<FormDetailsModel>(formDto);
            }
        }
    }
}
