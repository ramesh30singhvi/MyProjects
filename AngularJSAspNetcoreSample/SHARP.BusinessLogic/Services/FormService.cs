using AutoMapper;
using Microsoft.AspNetCore.Http;
using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.BusinessLogic.DTO.Common;
using SHARP.BusinessLogic.DTO.Form;
using SHARP.BusinessLogic.Extensions;
using SHARP.BusinessLogic.Helpers;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Constants;
using SHARP.Common.Enums;
using SHARP.Common.Extensions;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL;
using SHARP.DAL.Extensions;
using SHARP.DAL.Models;
using SHARP.DAL.Models.QueryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using SHARP.Common.Models;

namespace SHARP.BusinessLogic.Services
{
    public class FormService : IFormService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;

        private readonly AppConfig _appConfig;

        public FormService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IUserService userService,
            AppConfig appConfig)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;

            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            this._appConfig = appConfig;
        }

        public async Task<IEnumerable<FormVersionDto>> GetFormVersionsAsync(FormVersionFilter filter)
        {
            var orderBySelector = OrderByHelper.GetOrderBySelector<FormVersionColumn, Expression<Func<FormVersion, object>>>(
                    filter.OrderBy,
                    GetOrderBySelector);

            FormVersionQueryModel[] formVersions = await _unitOfWork.FormVersionRepository.GetFormVersionIdsAsync(filter);

            List<int> maxFormVersionIds = formVersions
            .GroupBy(form => form.FormId)
            .SelectMany(group => group.Where(form => form.Version == group.Max(form => form.Version)).Select(version => version.Id))
            .ToList();

            var sharpForms = await _unitOfWork.FormVersionRepository.GetAsync(filter, orderBySelector, maxFormVersionIds);

            var dtoForms = _mapper.Map<IEnumerable<FormVersionDto>>(sharpForms);
            return dtoForms;
        }

        public async Task<IEnumerable<FormVersionDto>> GetFormVersionsAsyncByFormId(int formId)
        {
            var sharpForms = _unitOfWork.FormVersionRepository.Find(formVersion => formVersion.FormId == formId).ToArray();
            var dtoForms = _mapper.Map<IEnumerable<FormVersionDto>>(sharpForms);
            return dtoForms;
        }

        public bool FormExists(int id, string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            name = name.Trim().ToLower();
            return _unitOfWork.FormRepository.Any(form => form.Name.ToLower() == name && form.Id != id);
        }

        public async Task<IReadOnlyCollection<OptionDto>> GetTypeOptionsAsync()
        {
            IReadOnlyCollection<AuditType> types = await _unitOfWork.AuditTypeRepository.GetListAsync(
                orderBySelector: type => type.Name,
                asNoTracking: true);

            return _mapper.Map<IReadOnlyCollection<OptionDto>>(types);
        }

        public async Task<IReadOnlyCollection<OptionDto>> GetFieldTypeOptionsAsync()
        {
            IReadOnlyCollection<FieldType> types = await _unitOfWork.FieldTypeRepository.GetListAsync(
                orderBySelector: type => type.Name,
                asNoTracking: true);

            return _mapper.Map<IReadOnlyCollection<OptionDto>>(types);
        }

        public async Task<FormVersion> GetFormVersionAsync(int formVersionId)
        {
            FormVersion formVersion = await _unitOfWork.FormVersionRepository.GetFormVersionAsync(formVersionId);

            if (formVersion is null)
            {
                throw new NotFoundException();
            }

            switch (formVersion.Form.AuditType.Name)
            {
                case CommonConstants.TWENTY_FOUR_HOUR_KEYWORD:
                    formVersion.Columns = await _unitOfWork.TableColumnRepository.GetFormKeywordsAsync(formVersion.Id);
                    break;

                case CommonConstants.CRITERIA:
                    formVersion.Columns = await _unitOfWork.TableColumnRepository.GetFormCriteriaQuestionsAsync(formVersion.Id);
                    formVersion.FormFields = await _unitOfWork.FormFieldRepository.GetFormFieldsAsync(formVersion.Id);
                    break;

                case CommonConstants.TRACKER:
                    formVersion.Columns = await _unitOfWork.TableColumnRepository.GetFormTrackerQuestionsAsync(formVersion.Id);
                    break;

                case CommonConstants.MDS:
                    formVersion.Sections = await _unitOfWork.FormSectionRepository.GetSectionsAsync(formVersion.Id);
                    formVersion.FormFields = await _unitOfWork.FormFieldRepository.GetFormFieldsAsync(formVersion.Id);
                    break;

                default:
                    break;
            }

            return formVersion;
        }

        public async Task<FormVersion> GetFormVersionAsyncForManagement(int formVersionId)
        {
            FormVersion formVersion = await _unitOfWork.FormVersionRepository.GetFormVersionAsync(formVersionId);

            if (formVersion is null)
            {
                throw new NotFoundException();
            }

            switch (formVersion.Form.AuditType.Name)
            {
                case CommonConstants.TWENTY_FOUR_HOUR_KEYWORD:
                    formVersion.Columns = await _unitOfWork.TableColumnRepository.GetFormKeywordsAsyncForManagement(formVersion.Id);
                    break;

                case CommonConstants.CRITERIA:
                    formVersion.Columns = await _unitOfWork.TableColumnRepository.GetFormCriteriaQuestionsAsync(formVersion.Id);
                    formVersion.FormFields = await _unitOfWork.FormFieldRepository.GetFormFieldsAsync(formVersion.Id);
                    break;

                case CommonConstants.TRACKER:
                    formVersion.Columns = await _unitOfWork.TableColumnRepository.GetFormTrackerQuestionsAsync(formVersion.Id);
                    break;

                case CommonConstants.MDS:
                    formVersion.Sections = await _unitOfWork.FormSectionRepository.GetSectionsAsync(formVersion.Id);
                    formVersion.FormFields = await _unitOfWork.FormFieldRepository.GetFormFieldsAsync(formVersion.Id);
                    break;

                default:
                    break;
            }

            return formVersion;
        }

        public async Task<FormVersionDto> GetFormVersionDetailsAsync(int formVersionId)
        {
            FormVersion form = await GetFormVersionAsync(formVersionId);

            return MapFormDetails(form);
        }

        public async Task<FormVersionDto> GetFormVersionDetailsAsyncForManagement(int formVersionId)
        {
            FormVersion form = await GetFormVersionAsyncForManagement(formVersionId);

            return MapFormDetails(form);
        }

        public async Task<FormVersionDto> AddFormAsync(AddFormDto formAddDto)
        {
            string userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = _userService.GetUser(userId);

            var form = _mapper.Map<Form>(formAddDto);
            form.FormOrganizations = new List<FormOrganization>();
            form.IsActive = true;

            form.FormOrganizations.Add(new FormOrganization()
            {
                OrganizationId = formAddDto.OrganizationId
            });
            FormVersion formVersion = new FormVersion() {
                Status = FormVersionStatus.Draft,
                CreatedDate = DateTime.UtcNow,
                CreatedByUserId = user.Id,
                Version = 1,
                Form = form
            };

            await _unitOfWork.FormVersionRepository.AddAsync(formVersion);
            await _unitOfWork.SaveChangesAsync();

            return await GetFormVersionDetailsAsync(formVersion.Id);
        }

        public async Task EditFormNameAsync(int formId, string formName)
        {
            var form = _unitOfWork.FormRepository.Find(form => form.Id == formId).SingleOrDefault();
            if (form == null)
                return;

            form.Name = formName;
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DuplicateFormAsync(int formId, int organizationId)
        {
            String userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = _userService.GetUser(userId);

            var form = _unitOfWork.FormRepository.Find(form => form.Id == formId).SingleOrDefault();
            if (form == null)
                return;

            var lightFormVersion = await _unitOfWork.FormVersionRepository.GetLastActiveFormVersionAsync(form.Id);

            var formVersion = await GetFormVersionAsync(lightFormVersion.Id);

            Form newForm = new Form()
            {
                Name = form.Name,
                AuditTypeId = form.AuditTypeId,
                DisableCompliance = form.DisableCompliance,
                AllowEmptyComment = form.AllowEmptyComment,
                IsActive = true
            };

            newForm.FormOrganizations = new List<FormOrganization>();
            newForm.IsActive = true;
            newForm.FormOrganizations.Add(new FormOrganization()
            {
                OrganizationId = organizationId
            });

            //await _unitOfWork.FormRepository.AddAsync(newForm);
            //await _unitOfWork.SaveChangesAsync();

            FormVersion newFormVersion = new FormVersion()
            {
                Status = FormVersionStatus.Draft,
                CreatedDate = DateTime.UtcNow,
                CreatedByUserId = user.Id,
                Version = 1,
                Form = newForm
            };

            await _unitOfWork.FormVersionRepository.AddAsync(newFormVersion);
            await _unitOfWork.SaveChangesAsync();

            foreach (FormSection section in formVersion.Sections)
            {
                FormSection newFormSection = new FormSection()
                {
                    Name = section.Name,
                    Sequence = section.Sequence,
                    FormVersionId = newFormVersion.Id
                };

                await _unitOfWork.FormSectionRepository.AddAsync(newFormSection);
                await _unitOfWork.SaveChangesAsync();

                foreach (FormGroup group in section.Groups)
                {
                    FormGroup newFormGroup = new FormGroup()
                    {
                        Name = group.Name,
                        Sequence = group.Sequence,
                        FormSectionId = newFormSection.Id
                    };

                    await _unitOfWork.FormGroupRepository.AddAsync(newFormGroup);
                    await _unitOfWork.SaveChangesAsync();

                    foreach (FormField formField in group.FormFields)
                    {
                        FormField newFormField = new FormField()
                        {
                            FieldTypeId = formField.FieldTypeId,
                            Sequence = formField.Sequence,
                            FieldName = formField.FieldName,
                            LabelName = formField.LabelName,
                            FormVersionId = newFormVersion.Id,
                            IsRequired = formField.IsRequired,
                            FormGroupId = newFormGroup.Id
                        };

                        await _unitOfWork.FormFieldRepository.AddAsync(newFormField);
                        await _unitOfWork.SaveChangesAsync();

                        foreach (FormFieldItem formFieldItem in formField.Items)
                        {
                            FormFieldItem newFormFieldItem = new FormFieldItem()
                            {
                                FormFieldId = newFormField.Id,
                                Value = formFieldItem.Value,
                                Code = formFieldItem.Code,
                                Sequence = formFieldItem.Sequence
                            };

                            await _unitOfWork.FormFieldItemRepository.AddAsync(newFormFieldItem);
                            await _unitOfWork.SaveChangesAsync();
                        }

                    }

                }

            }

        }

        public async Task<FormVersionDto> EditFormVersionAsync(int formVersionId)
        {
            string userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = _userService.GetUser(userId);

            FormVersion form = await GetFormVersionAsync(formVersionId);

            var draftForm = form.Clone();
            draftForm.CreatedByUserId = user.Id;
            draftForm.CreatedBy = user;

            if (form.Groups != null && form.Groups.Any())
            {
                form.Groups.ToList().ForEach(group =>
                {
                    var formGroup = group.Clone(draftForm);

                    draftForm.Groups.Add(formGroup);
                });
            }

            if (form.Columns != null && form.Columns.Any())
            {
                var oldGroups = form.Groups.ToList();

                form.Columns.Where(column => !column.ParentId.HasValue).ToList().ForEach(column =>
                {
                    int groupIndex = oldGroups.FindIndex(group => group.Id == column.GroupId);

                    var columnGroup = groupIndex >= 0 ? draftForm.Groups.ElementAt(groupIndex) : null;

                    var formColumn = column.Clone(draftForm, columnGroup);

                    if (column.SubQuestions != null && column.SubQuestions.Any())
                    {
                        column.SubQuestions.ToList().ForEach(subColumn => {
                            var formSubColumn = subColumn.Clone(draftForm);

                            formColumn.SubQuestions.Add(formSubColumn);
                        });
                    }
                    if (column.KeywordTrigger != null && column.KeywordTrigger.Any())
                    {
                        column.KeywordTrigger.ToList().ForEach(keywordTrigger =>
                        {
                            var formKeywordTrigger = keywordTrigger.Clone();

                            formColumn.KeywordTrigger.Add(formKeywordTrigger);
                        });
                    }

                    draftForm.Columns.Add(formColumn);
                });
            }

            if (form.FormFields != null && form.FormFields.Any())
            {
                form.FormFields.ToList().ForEach(field =>
                {
                    var formField = field.Clone(draftForm);

                    draftForm.FormFields.Add(formField);
                });
            }

            if (form.Sections != null && form.Sections.Any())
            {
                form.Sections.OrderBy(section => section.Id).ToList().ForEach(section =>
                {
                    var newSection = section.Clone(draftForm);

                    draftForm.Sections.Add(newSection);
                });
            }

            await _unitOfWork.FormVersionRepository.AddAsync(draftForm);

            await _unitOfWork.SaveChangesAsync();

            return MapFormDetails(draftForm);
        }

        public async Task PublishFormVersionAsync(int formVersionId, int allowEmptyComment, int disableCompliance, bool useHighAlert, int ahTime)
        {
            Form form = await _unitOfWork.FormRepository.FirstOrDefaultAsync(
                form => form.Versions.Any(version => version.Id == formVersionId),
                form => form.Versions);

            form.DisableCompliance = disableCompliance;
            form.AllowEmptyComment = allowEmptyComment;
            form.UseHighAlert = useHighAlert;
            form.AHTime = ahTime;

            var activeForm = form.Versions.SingleOrDefault(version => version.Status == FormVersionStatus.Published);

            if (activeForm != null)
            {
                activeForm.Status = FormVersionStatus.Archived;
            }

            var draftForm = form.Versions.SingleOrDefault(version => version.Id == formVersionId);
            draftForm.Status = FormVersionStatus.Published;
            draftForm.ActivationDate = DateTime.UtcNow;


            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<FormVersionDto> DeleteFormVersionAsync(int formVersionId)
        {
            FormVersion form = await _unitOfWork.FormVersionRepository.GetAsync(formVersionId);

            if (form.Status != FormVersionStatus.Draft)
            {
                throw new Exception("You can only delete the draft form");
            }

            _unitOfWork.FormVersionRepository.Remove(form);

            await _unitOfWork.SaveChangesAsync();

            FormVersion activeForm = await _unitOfWork.FormVersionRepository
                .FirstOrDefaultAsync(formVersion => formVersion.FormId == form.FormId && formVersion.Status == FormVersionStatus.Published);

            if (activeForm is null)
            {
                return null;
            }

            return await GetFormVersionDetailsAsync(activeForm.Id);
        }

        //For AutoTests
        public async Task<bool> DeleteFormAsync(int id)
        {
            var form = await _unitOfWork.FormRepository.GetAsync(id);

            if (form == null)
            {
                throw new NotFoundException($"Form with Id: {id} is not found");
            }

            _unitOfWork.FormRepository.Remove(form);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<KeywordOptionDto> AddFormKeywordAsync(AddFormKeywordDto addKeyword)
        {
            FormVersion formVersion = await _unitOfWork.FormVersionRepository.GetFormVersionKeywordsAsync(addKeyword.FormVersionId);

            if (formVersion is null)
            {
                throw new NotFoundException();
            }

            ValidateKeywords(default(int?), addKeyword.Name, formVersion.Columns);

            TableColumn keyword = _mapper.Map<TableColumn>(addKeyword);

            keyword.Sequence = formVersion.Columns.Any() ? formVersion.Columns.Max(keyword => keyword.Sequence) + 1 : 1;

            formVersion.Columns.Add(keyword);

            await _unitOfWork.SaveChangesAsync();
            if (addKeyword.Trigger != null && addKeyword.Trigger.Value)
            {
                keyword.KeywordTrigger = new List<KeywordTrigger>();
                foreach (var form in addKeyword.FormsTriggeredByKeyword)
                    keyword.KeywordTrigger.Add(new KeywordTrigger() { KeywordId = keyword.Id, KeywordFormId = keyword.FormVersion.FormId, FormTriggerId = form.Id });
            }
            await _unitOfWork.SaveChangesAsync();

            var column = await _unitOfWork.TableColumnRepository.GetKeywordAsync(keyword.Id);
            return _mapper.Map<KeywordOptionDto>(column);
        }

        public async Task<OptionDto> AddFormCustomKeywordAsync(AddFormKeywordDto addKeyword)
        {
            FormVersion formVersion = await _unitOfWork.FormVersionRepository.GetFormVersionKeywordsAsync(addKeyword.FormVersionId);

            if (formVersion is null)
            {
                throw new NotFoundException();
            }

            ValidateKeywords(default(int?), addKeyword.Name, formVersion.Columns);

            TableColumn keyword = _mapper.Map<TableColumn>(addKeyword);

            keyword.Sequence = formVersion.Columns.Any() ? formVersion.Columns.Max(keyword => keyword.Sequence) + 1 : 1;

            _unitOfWork.TableColumnRepository.Add(keyword);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<OptionDto>(keyword);
        }

        public async Task<bool> EditFormKeywordAsync(EditFormKeywordDto editKeyword)
        {
            FormVersion formVersion = await _unitOfWork.FormVersionRepository.GetFormVersionKeywordsAsync(editKeyword.FormVersionId);

            if (formVersion is null)
            {
                throw new NotFoundException();
            }

            ValidateKeywords(editKeyword.Id, editKeyword.Name, formVersion.Columns);

            var currentFormKeyword = formVersion.Columns.FirstOrDefault(column => column.Id == editKeyword.Id);

            _mapper.Map(editKeyword, currentFormKeyword);

            await EditKeywordTrigger(currentFormKeyword, editKeyword);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private async Task EditKeywordTrigger(TableColumn currentFormKeyword,EditFormKeywordDto editKeyword)
        {
            if (editKeyword.Trigger != null && !editKeyword.Trigger.Value)
            {
                if (currentFormKeyword.KeywordTrigger != null)
                {
                    _unitOfWork.KeywordTriggerRepository.RemoveRange(currentFormKeyword.KeywordTrigger);
                    await _unitOfWork.SaveChangesAsync();
                    return;
                }
            }


            if (editKeyword.Trigger != null && editKeyword.Trigger.Value)
            {
                if (currentFormKeyword.KeywordTrigger == null)
                {
                    currentFormKeyword.KeywordTrigger = new List<KeywordTrigger>();
                }

                if (!currentFormKeyword.KeywordTrigger.Any(key => key.KeywordId == currentFormKeyword.Id ))
                {
                    foreach (var form in editKeyword.FormsTriggeredByKeyword)
                        currentFormKeyword.KeywordTrigger.Add(new KeywordTrigger() { KeywordId = currentFormKeyword.Id, KeywordFormId = currentFormKeyword.FormVersion.FormId, FormTriggerId = form.Id });
                }
                else
                {
                    var updatedKeywordTriggerList = new List<KeywordTrigger>();
                    foreach(var keywordTrigger in currentFormKeyword.KeywordTrigger)
                    {
                        if (editKeyword.FormsTriggeredByKeyword.Any(form => form.Id == keywordTrigger.FormTriggerId))
                            updatedKeywordTriggerList.Add(keywordTrigger);
                    }
                    foreach (var form in editKeyword.FormsTriggeredByKeyword)
                    {
                        if (!currentFormKeyword.KeywordTrigger.Any(keywordTrigger => keywordTrigger.FormTriggerId == form.Id))
                            updatedKeywordTriggerList.Add(new KeywordTrigger() { KeywordId = currentFormKeyword.Id, KeywordFormId = currentFormKeyword.FormVersion.FormId, FormTriggerId = form.Id });
                    }

                    currentFormKeyword.KeywordTrigger = updatedKeywordTriggerList;
                }
             
            }
        }
        public async Task<FormVersionDto> DeleteTableColumnAsync(int id)
        {
            TableColumn tableColumn = await _unitOfWork.TableColumnRepository
                .FirstOrDefaultAsync(column => column.Id == id,
                column => column.KeywordTrigger);

            if (tableColumn != null && tableColumn.KeywordTrigger.Any())
            {
                _unitOfWork.KeywordTriggerRepository.RemoveRange(tableColumn.KeywordTrigger);
            }
            _unitOfWork.TableColumnRepository.Remove(tableColumn);

            await _unitOfWork.SaveChangesAsync();

            return await GetFormVersionDetailsAsync(tableColumn.FormVersionId);
        }

        public async Task<FormVersionDto> DeleteQuestionAsync(int id)
        {
            TableColumn question = await _unitOfWork.TableColumnRepository
                .FirstOrDefaultAsync(
                column => column.Id == id,
                column => column.SubQuestions);

            if (question != null && question.SubQuestions.Any())
            {
                _unitOfWork.TableColumnRepository.RemoveRange(question.SubQuestions);
            }

            _unitOfWork.TableColumnRepository.Remove(question);

            RearrangeQuestions(column =>
                   column.FormVersionId == question.FormVersionId &&
                   column.GroupId == question.GroupId &&
                   column.ParentId == question.ParentId &&
                   column.Id != question.Id);

            await _unitOfWork.SaveChangesAsync();

            return await GetFormVersionDetailsAsync(question.FormVersionId);
        }

        public async Task<bool> RearrangeQuestionsAsync(RearrangeQuestionsDto rearrangeQuestionsDto)
        {
            List<int> questionIds = rearrangeQuestionsDto.Questions.Select(x => x.Id).ToList();

            IEnumerable<TableColumn> questionsToUpdate = _unitOfWork.TableColumnRepository
            .Find(question => questionIds.Contains(question.Id))
            .ToList();

            questionsToUpdate = questionsToUpdate.Actualize(rearrangeQuestionsDto.Questions, model => _mapper.Map<TableColumn>(model), (model, entity) => _mapper.Map(model, entity));

            _unitOfWork.TableColumnRepository.UpdateRange(questionsToUpdate);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IReadOnlyCollection<OptionDto>> GetSectionOptionsAsync(int formVersionId)
        {
            IReadOnlyCollection<TableColumnGroup> sections = await _unitOfWork.TableColumnGroupRepository.GetListAsync(
                section => section.FormVersionId == formVersionId,
                section => section.Sequence,
                asNoTracking: true);

            return _mapper.Map<IReadOnlyCollection<OptionDto>>(sections);
        }

        public async Task<IReadOnlyCollection<MdsSectionDto>> GetMdsSectionsAsync(int formVersionId)
        {
            IReadOnlyCollection<FormSection> sections = await _unitOfWork.FormSectionRepository.GetListAsync(section => section.FormVersionId == formVersionId, asNoTracking: true);
            return _mapper.Map<IReadOnlyCollection<MdsSectionDto>>(sections);
        }

        public async Task<FormVersionDto> AddMdsSectionAsync(AddMdsSectionDto addMdsSectionDto)
        {
            FormSection formSection = new FormSection();
            formSection.Name = addMdsSectionDto.Name;
            formSection.FormVersionId = addMdsSectionDto.FormVersionId;
            formSection.Sequence = GetMdsSectionSequence(addMdsSectionDto.FormVersionId);
            await _unitOfWork.FormSectionRepository.AddAsync(formSection);
            await _unitOfWork.SaveChangesAsync();

            return await GetFormVersionDetailsAsync(addMdsSectionDto.FormVersionId);
        }

        public async Task<FormVersionDto> EditMdsGroupAsync(EditMdsGroupDto editMdsGroupDto)
        {
            var group =
                await _unitOfWork.FormGroupRepository.GetFormGroupAsync(editMdsGroupDto.Id);
            group.Name = editMdsGroupDto.Name;

            await _unitOfWork.SaveChangesAsync();

            return await GetFormVersionDetailsAsync(group.FormSection.FormVersionId);
        }

        public async Task<FormVersionDto> EditMdsSectionAsync(EditMdsSectionDto editMdsSectionDto)
        {
            var formSection =
                await _unitOfWork.FormSectionRepository.FirstOrDefaultAsync(section =>
                    section.Id == editMdsSectionDto.Id);
            formSection.Name = editMdsSectionDto.Name;

            await _unitOfWork.SaveChangesAsync();

            return await GetFormVersionDetailsAsync(formSection.FormVersionId);
        }

        public async Task<FormVersionDto> AddCriteriaQuestionAsync(AddCriteriaQuestionDto addQuestionDto)
        {
            TableColumn question = _mapper.Map<TableColumn>(addQuestionDto);
            question.Sequence = GetCriteriaQuestionSequence(addQuestionDto);

            if (question.Group != null && question.Group.Sequence == 0)
            {
                question.Group.Sequence = GetSectionSequence(addQuestionDto.FormVersionId);
            }

            await _unitOfWork.TableColumnRepository.AddAsync(question);
            await _unitOfWork.SaveChangesAsync();

            return await GetFormVersionDetailsAsync(addQuestionDto.FormVersionId);
        }

        public async Task<FormVersionDto> AddMdsGroupAsync(AddMdsGroupDto addMdsGroupDto)
        {
            FormGroup group = new FormGroup();
            group.Name = addMdsGroupDto.Name;
            group.FormSectionId = addMdsGroupDto.FormSectionId;
            group.Sequence = GetMdsGroupSequence(addMdsGroupDto.FormSectionId);

            await _unitOfWork.FormGroupRepository.AddAsync(group);
            await _unitOfWork.SaveChangesAsync();

            return await GetFormVersionDetailsAsync(addMdsGroupDto.FormVersionId);

        }

        public async Task<FormVersionDto> EditCriteriaQuestionAsync(EditCriteriaQuestionDto editQuestionDto)
        {
            TableColumn question = await _unitOfWork.TableColumnRepository.GetCriteriaQuestionAsync(editQuestionDto.Id);

            bool isGroupChanged = question.Group?.Id != editQuestionDto.Group?.Id || question.Group?.Name != editQuestionDto.Group?.Name;

            int? prevGroupId = question.Group?.Id;

            _mapper.Map(editQuestionDto, question);

            if (isGroupChanged)
            {
                question.Sequence = GetCriteriaQuestionSequence(editQuestionDto);

                if (question.Group != null && question.Group.Sequence == 0)
                {
                    question.Group.Sequence = GetSectionSequence(editQuestionDto.FormVersionId);
                }

                RearrangeQuestions(column =>
                    column.FormVersionId == question.FormVersionId &&
                    column.GroupId == prevGroupId &&
                    column.ParentId == null &&
                    column.Id != question.Id);
            }

            await _unitOfWork.SaveChangesAsync();

            return await GetFormVersionDetailsAsync(editQuestionDto.FormVersionId);
        }

        public async Task<FormVersionDto> AddFormFieldAsync(AddFormFieldDto addFormFieldDto)
        {
            FormField formField = _mapper.Map<FormField>(addFormFieldDto);
            formField.Sequence = GetFormFieldSequence(addFormFieldDto.FormVersionId);

            await _unitOfWork.FormFieldRepository.AddAsync(formField);
            await _unitOfWork.SaveChangesAsync();

            return await GetFormVersionDetailsAsync(addFormFieldDto.FormVersionId);
        }

        public async Task<FormVersionDto> EditFormFieldAsync(EditFormFieldDto editFormFieldDto)
        {
            FormField formField = await _unitOfWork.FormFieldRepository
                .FirstOrDefaultAsync(
                field => field.Id == editFormFieldDto.Id,
                field => field.Items);

            _mapper.Map(editFormFieldDto, formField);

            await _unitOfWork.SaveChangesAsync();

            return await GetFormVersionDetailsAsync(editFormFieldDto.FormVersionId);
        }

        public async Task<FormVersionDto> DeleteFormFieldAsync(int formVersionId, int fieldId)
        {
            FormVersion formVersion = await _unitOfWork.FormVersionRepository
                .FirstOrDefaultAsync(
                field => field.Id == formVersionId,
                field => field.FormFields);

            FormField formField = formVersion.FormFields.FirstOrDefault(field => field.Id == fieldId);

            if (formField != null)
            {
                formVersion.FormFields.Remove(formField);
            }
            else
            {
                throw new NotFoundException("Field is not found");
            }

            int sequence = 1;
            foreach (var field in formVersion.FormFields)
            {
                field.Sequence = sequence;

                sequence++;
            }

            await _unitOfWork.SaveChangesAsync();

            return await GetFormVersionDetailsAsync(formVersionId);
        }

        public async Task<bool> RearrangeFormFieldsAsync(int formVersionId, RearrangeItemsDto rearrangeItemsDto)
        {
            FormVersion formVersion = await _unitOfWork.FormVersionRepository
               .FirstOrDefaultAsync(
               field => field.Id == formVersionId,
               field => field.FormFields);

            formVersion.FormFields = formVersion.FormFields
                .Actualize(rearrangeItemsDto.Items, model => _mapper.Map<FormField>(model), (model, entity) => _mapper.Map(model, entity))
                .ToList();

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<FormVersionDto> EditFormSectionsAsync(int formVersionId, EditQuestionGroupsDto editQuestionGroupsDto)
        {
            FormVersion formVersion = await _unitOfWork.FormVersionRepository
               .FirstOrDefaultAsync(
               field => field.Id == formVersionId,
               field => field.Groups);

            int[] deletedGroupIds = formVersion.Groups.Select(group => group.Id).Except(editQuestionGroupsDto.Sections.Select(section => section.Id)).ToArray();

            TableColumnGroup[] deletedGroups = formVersion.Groups.Where(group => deletedGroupIds.Contains(group.Id)).ToArray();

            formVersion.Groups = formVersion.Groups
                .Actualize(editQuestionGroupsDto.Sections, model => _mapper.Map<TableColumnGroup>(model), (model, entity) => _mapper.Map(model, entity))
                .ToList();

            if (deletedGroupIds.Any())
            {
                IEnumerable<TableColumn> questions = _unitOfWork.TableColumnRepository
                    .Find(question => question.GroupId.HasValue && deletedGroupIds.Contains(question.GroupId.Value));

                if (questions.Any())
                {
                    int maxSequence = _unitOfWork.TableColumnRepository
                    .Find(question => question.FormVersionId == formVersionId && !question.GroupId.HasValue)
                    .Select(column => column.Sequence)
                    .DefaultIfEmpty(0)
                    .Max();

                    foreach (var question in questions)
                    {
                        maxSequence++;

                        question.GroupId = null;
                        question.Sequence = maxSequence;
                    }

                    _unitOfWork.TableColumnRepository.UpdateRange(questions);
                }

                _unitOfWork.TableColumnGroupRepository.RemoveRange(deletedGroups);
            }

            await _unitOfWork.SaveChangesAsync();

            return await GetFormVersionDetailsAsync(formVersionId);
        }

        public async Task<FormVersionDto> AddTrackerQuestionAsync(AddTrackerQuestionDto addQuestionDto)
        {
            TableColumn question = _mapper.Map<TableColumn>(addQuestionDto);

            int maxSequence = _unitOfWork.TableColumnRepository
                    .Find(question => question.FormVersionId == addQuestionDto.FormVersionId)
                    .Select(group => group.Sequence)
                    .DefaultIfEmpty(0)
                    .Max();

            question.Sequence = ++maxSequence;


            await _unitOfWork.TableColumnRepository.AddAsync(question);
            await _unitOfWork.SaveChangesAsync();

            return await GetFormVersionDetailsAsync(addQuestionDto.FormVersionId);
        }

        public async Task<FormVersionDto> AddMdsQuestionAsync(AddTrackerQuestionDto addQuestionDto)
        {
            FormField formField = new FormField();
            formField.FieldName = addQuestionDto.Question.Replace(" ", "");
            formField.FieldTypeId = addQuestionDto.FieldTypeId;
            formField.LabelName = addQuestionDto.Question;
            formField.FormVersionId = addQuestionDto.FormVersionId;
            formField.IsRequired = addQuestionDto.IsRequired;
            formField.FormGroupId = addQuestionDto.FormGroupId;
            formField.Sequence = GetFormFieldSequenceFromGroup(addQuestionDto.FormGroupId);

            await _unitOfWork.FormFieldRepository.AddAsync(formField);
            await _unitOfWork.SaveChangesAsync();

            foreach (FormFieldItemDto formFieldItemDto in addQuestionDto.Items)
            {
                FormFieldItem formFieldItem = new FormFieldItem();
                formFieldItem.FormFieldId = formField.Id;
                formFieldItem.Value = formFieldItemDto.Value;
                formFieldItem.Sequence = formFieldItemDto.Sequence;
                formFieldItem.Code = formFieldItemDto.Code;

                await _unitOfWork.FormFieldItemRepository.AddAsync(formFieldItem);
                await _unitOfWork.SaveChangesAsync();

            }

            return await GetFormVersionDetailsAsync(addQuestionDto.FormVersionId);
        }

        public async Task<FormVersionDto> EditMdsQuestionAsync(EditTrackerQuestionDto editQuestionDto)
        {
            var editFormField = await _unitOfWork.FormFieldRepository.GetFormFieldAsync(editQuestionDto.Id);
            editFormField.LabelName = editQuestionDto.Question;
            editFormField.FieldName = editQuestionDto.Question.Replace(" ", "");
            editFormField.IsRequired = editQuestionDto.IsRequired;
            editFormField.FieldTypeId = editQuestionDto.FieldTypeId;

            await _unitOfWork.SaveChangesAsync();

            _unitOfWork.FormFieldItemRepository.RemoveRange(editFormField.Items);

            foreach (FormFieldItemDto formFieldItemDto in editQuestionDto.Items)
            {
                FormFieldItem formFieldItem = new FormFieldItem();
                formFieldItem.FormFieldId = editFormField.Id;
                formFieldItem.Value = formFieldItemDto.Value;
                formFieldItem.Sequence = formFieldItemDto.Sequence;
                formFieldItem.Code = formFieldItemDto.Code;

                await _unitOfWork.FormFieldItemRepository.AddAsync(formFieldItem);
                await _unitOfWork.SaveChangesAsync();

            }


            return await GetFormVersionDetailsAsync(editFormField.FormVersionId);
        }

        public async Task<FormVersionDto> EditTrackerQuestionAsync(EditTrackerQuestionDto editQuestionDto)
        {
            TableColumn question = await _unitOfWork.TableColumnRepository.GetTrackerQuestionAsync(editQuestionDto.Id);

            _mapper.Map(editQuestionDto, question);

            await _unitOfWork.SaveChangesAsync();

            return await GetFormVersionDetailsAsync(editQuestionDto.FormVersionId);
        }

        public async Task<bool> IsFormActive(int formVersionId)
        {
            FormVersion formVersion = await _unitOfWork.FormVersionRepository.FirstOrDefaultAsync(
                formVersion => formVersion.Id == formVersionId,
                formVersion => formVersion.Form);

            return formVersion.Status == FormVersionStatus.Published && formVersion.Form.IsActive;
        }

        public async Task<bool> IsFormNameAlreadyExist(string formName, int organizationId, int? formId)
        {
            Expression<Func<FormVersion, bool>> predicate = i =>
            i.Form.Name.ToUpper().Trim() == formName.ToUpper().Trim() &&
            i.Form.FormOrganizations.Any(formOrganization => formOrganization.Organization.Id == organizationId) &&
            i.Status != FormVersionStatus.Archived;

            if (formId.HasValue)
            {
                predicate = predicate.And(i => i.Id != formId.Value);
            }

            return await _unitOfWork.FormVersionRepository.ExistsAsync(predicate);
        }

        private Expression<Func<FormVersion, object>> GetOrderBySelector(FormVersionColumn columnName)
        {
            var columnSelectorMap = new Dictionary<FormVersionColumn, Expression<Func<FormVersion, object>>>
            {
                {
                    FormVersionColumn.Name,
                    i => i.Form.Name
                },
                {
                    FormVersionColumn.Organizations,
                    i => SqlFunctions.OrganizationFormVersionOrderValue(i.Id)
                },
                {
                    FormVersionColumn.AuditType,
                    i => i.Form.AuditType.Name
                },
                {
                    FormVersionColumn.CreatedDate,
                    i => i.CreatedDate
                }
            };

            if (!columnSelectorMap.TryGetValue(columnName, out var selector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return selector;
        }

        public async Task<IReadOnlyCollection<FilterOption>> GetFilterColumnSourceDataAsync(FormManagementFilterColumnSource<FormVersionColumn> columnData)
        {
            if (columnData.Column == FormVersionColumn.Undefined)
            {
                throw new InvalidOperationException("Column is invalid");
            }

            var queryRule = GetColumnQueryRule(columnData.Column);

            var columnValues = await _unitOfWork.FormVersionRepository.GetDistinctColumnAsync(columnData, queryRule);

            return _mapper.Map<IReadOnlyCollection<FilterOption>>(columnValues.OrderBy(i => i.Value));
        }

        public async Task<IReadOnlyCollection<FormOptionDto>> GetFormVersionOptionsAsync(int organizationId, string auditType)
        {
            IReadOnlyCollection<FormVersion> forms = await _unitOfWork.FormVersionRepository.GetFormVersionsByOrganizationAsync(organizationId, auditType);

            return _mapper.Map<IReadOnlyCollection<FormOptionDto>>(forms);
        }

        public async Task<IReadOnlyCollection<FormOptionDto>> GetFormOptionsAsync(int organizationId, string auditType)
        {
            IReadOnlyCollection<Form> forms = await _unitOfWork.FormRepository.GetFormsByOrganizationAsync(organizationId, auditType);

            return _mapper.Map<IReadOnlyCollection<FormOptionDto>>(forms);
        }

        public async Task<IReadOnlyCollection<FormOptionDto>> GetFormOptionsAsync(FormOptionFilter filter)
        {
            ICollection<int> userOrganizationIds = await _userService.GetUserOrganizationIdsAsync();

            if (userOrganizationIds.Any() && !filter.OrganizationIds.Any())
            {
                filter.OrganizationIds = userOrganizationIds.ToList();
            }

            IReadOnlyCollection<Form> forms = await _unitOfWork.FormRepository.GetFormOptionsAsync(filter);

            return _mapper.Map<IReadOnlyCollection<FormOptionDto>>(forms);
        }

        public async Task<bool> SetFormStateAsync(int formId, bool state)
        {
            Form form = await _unitOfWork.FormRepository.FirstOrDefaultAsync(
                form => form.Id == formId);

            if (form == null)
            {
                throw new NotFoundException("Form is not found");
            }

            form.IsActive = state;

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<int> GetLastActiveFormVersionIdAsync(int formId)
        {
            var formVersion = await _unitOfWork.FormVersionRepository.GetLastActiveFormVersionAsync(formId);

            if (formVersion == null)
            {
                throw new NotFoundException("Form is not found");
            }

            return formVersion.Id;
        }

        private ColumnOptionQueryRule<FormVersion, FilterOptionQueryModel> GetColumnQueryRule(FormVersionColumn columnName)
        {
            var columnQueryRuleMap = new Dictionary<FormVersionColumn, ColumnOptionQueryRule<FormVersion, FilterOptionQueryModel>>
            {
                {
                    FormVersionColumn.Name,
                    new ColumnOptionQueryRule<FormVersion, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Id = i.FormId, Value = i.Form.Name }
                    }
                },
                {
                    FormVersionColumn.Organizations,
                    new ColumnOptionQueryRule<FormVersion, FilterOptionQueryModel>
                    {
                        ManySelector = i => i.Form.FormOrganizations.Select(a => new FilterOptionQueryModel { Id = a.OrganizationId, Value = a.Organization.Name })
                    }
                },
                {
                    FormVersionColumn.AuditType,
                    new ColumnOptionQueryRule<FormVersion, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Id = i.Form.AuditTypeId, Value = i.Form.AuditType.Name } //i.Form.AuditType.Name
                    }
                }
            };

            if (!columnQueryRuleMap.TryGetValue(columnName, out var queryRule))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return queryRule;
        }

        private FormVersionDto MapFormDetails(FormVersion formVersion)
        {
            if (formVersion == null)
            {
                return null;
            }

            switch (formVersion.Form.AuditType.Name)
            {
                case CommonConstants.TWENTY_FOUR_HOUR_KEYWORD:
                    return _mapper.Map<KeywordFormDetailsDto>(formVersion);

                case CommonConstants.CRITERIA:
                    return _mapper.Map<CriteriaFormDetailsDto>(formVersion);

                case CommonConstants.TRACKER:
                    return _mapper.Map<TrackerFormDetailsDto>(formVersion);

                case CommonConstants.MDS:
                    return _mapper.Map<MdsFormDetailsDto>(formVersion);

                default:
                    return _mapper.Map<FormVersionDto>(formVersion);
            }
        }

        private int GetCriteriaQuestionSequence(AddCriteriaQuestionDto addQuestionDto)
        {
            Expression<Func<TableColumn, bool>> predicate = null;

            if (addQuestionDto.Group != null && addQuestionDto.Group.Id == null && addQuestionDto.Group.Name != null)
            {
                return 1;
            }
            else if (addQuestionDto.ParentId.HasValue)
            {
                predicate = column => column.FormVersionId == addQuestionDto.FormVersionId && column.ParentId == addQuestionDto.ParentId;
            }
            else
            {
                int? groupId = addQuestionDto.Group?.Id;

                predicate = column => column.FormVersionId == addQuestionDto.FormVersionId && column.GroupId == groupId && column.ParentId == null;
            }

            int maxSequence = _unitOfWork.TableColumnRepository
                    .Find(predicate)
                    .Select(column => column.Sequence)
                    .DefaultIfEmpty(0)
                    .Max();

            return ++maxSequence;
        }

        private int GetFormFieldSequence(int formVersionId)
        {
            int maxSequence = _unitOfWork.FormFieldRepository
                    .Find(field => field.FormVersionId == formVersionId)
                    .Select(column => column.Sequence)
                    .DefaultIfEmpty(0)
                    .Max();

            return ++maxSequence;
        }

        private int GetFormFieldSequenceFromGroup(int? formGroupId)
        {
            int maxSequence = _unitOfWork.FormFieldRepository
                .Find(field => field.FormGroupId == formGroupId)
                .Select(column => column.Sequence)
                .DefaultIfEmpty(0)
                .Max();

            return ++maxSequence;
        }

        private int GetMdsGroupSequence(int formSectionId)
        {
            int maxSequence = _unitOfWork.FormGroupRepository
                .Find(field => field.FormSectionId == formSectionId)
                .Select(column => column.Sequence)
                .DefaultIfEmpty(0)
                .Max();

            return ++maxSequence;
        }

        private int GetMdsSectionSequence(int formVersionId)
        {
            int maxSequence = _unitOfWork.FormSectionRepository
                .Find(field => field.FormVersionId == formVersionId)
                .Select(column => column.Sequence)
                .DefaultIfEmpty(0)
                .Max();

            return ++maxSequence;
        }

        private int GetSectionSequence(int formVersionId)
        {
            int maxSequence = _unitOfWork.TableColumnGroupRepository
                    .Find(group => group.FormVersionId.HasValue && group.FormVersionId.Value == formVersionId)
                    .Select(group => group.Sequence)
                    .DefaultIfEmpty(0)
                    .Max();

            return ++maxSequence;
        }

        private void RearrangeQuestions(Expression<Func<TableColumn, bool>> predicate)
        {
            TableColumn[] questionsToUpdate = _unitOfWork.TableColumnRepository
                .Find(predicate)
                .OrderBy(column => column.Sequence)
                .ToArray();

            for (int i = 0; i < questionsToUpdate.Count(); i++)
            {
                questionsToUpdate[i].Sequence = i + 1;
            }

            _unitOfWork.TableColumnRepository.UpdateRange(questionsToUpdate);
        }

        private void ValidateKeywords(int? currentKeywordId, string currentkeyword, ICollection<TableColumn> columns)
        {
            var formKeywords = columns.Where(c => c.Id != currentKeywordId && c.Hidden == null).SelectMany(c => c.Name.Split(CommonConstants.SLASH).Select(k => k.Trim().ToLower())).ToList();

            var currentKeywords = currentkeyword.Split(CommonConstants.SLASH).Select(k => k.Trim().ToLower());

            List<string> duplicatedKeywords = new List<string>();

            foreach (var keyword in currentKeywords)
            {
                if (formKeywords.Contains(keyword))
                {
                    duplicatedKeywords.Add(keyword);
                }
                else
                {
                    formKeywords.Add(keyword);
                }
            }

            if (duplicatedKeywords.Any())
            {
                throw new Exception($"Duplicated keywords: {string.Join(", ", duplicatedKeywords)}");
            }
        }

        public async Task<TableColumn[]> GetQuestions(int[] FormVersionIds)
        {
            var questions = await _unitOfWork.TableColumnRepository.GetFormCriteriaSimpleQuestionsAsync(formVersionIds: FormVersionIds);

            return questions;
        }

        public async Task<IReadOnlyCollection<OptionDto>> GetFormsTriggeredAsync(int organizationId, string keyword)
        {
            var formsCriteria = await _unitOfWork.FormRepository.GetFormsByOrganizationAsync(organizationId: organizationId,
                 auditType: CommonConstants.CRITERIA);
            var formsTrackers = await _unitOfWork.FormRepository.GetFormsByOrganizationAsync(organizationId: organizationId,
                 auditType: CommonConstants.TRACKER);

            IList<string> keywords = new List<string>();

            if (keyword != "none")
                keywords = keyword.Split('-').ToList();


            var formSuppl = formsCriteria.Where(form => form.IsActive && form.Name.Contains(CommonConstants.SUPPLEMENTAL_AUDIT_FORM, StringComparison.InvariantCultureIgnoreCase)).ToList();
            var formTrackSupp = formsTrackers.Where(form => form.IsActive && form.Name.Contains(CommonConstants.SUPPLEMENTAL_AUDIT_FORM, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (!keywords.Any())
            {
                return _mapper.Map<IReadOnlyCollection<OptionDto>>(formSuppl);
            }
            else
            {
                var formsBYName = new List<Form>();
                foreach (var key in keywords)
                {
                    if (string.IsNullOrEmpty(key)) continue;

                    formsBYName = formsCriteria.Where(form => form.IsActive && form.Name.Contains(key, StringComparison.InvariantCultureIgnoreCase)).ToList();
                    if (formsBYName.Any())
                    {
                        formSuppl.AddRange(formsBYName);
                    }
                    formsBYName = formsTrackers.Where(form => form.IsActive && form.Name.Contains(key, StringComparison.InvariantCultureIgnoreCase)).ToList();
                    if (formsBYName.Any())
                    {
                        formTrackSupp.AddRange(formsBYName);
                    }
                }
            }
            var outputForms = new List<Form>();
            formSuppl = formSuppl.Distinct().ToList();
            formTrackSupp = formTrackSupp.Distinct().ToList();
            outputForms.AddRange(formSuppl);
            outputForms.AddRange(formTrackSupp);
            return _mapper.Map<IReadOnlyCollection<OptionDto>>(outputForms);
        }

        public async Task<IReadOnlyCollection<OptionDto>> GetAuditTypes()
        {
            var auditTypes =  _unitOfWork.AuditTypeRepository.GetAll();
            return _mapper.Map<IReadOnlyCollection<OptionDto>>(auditTypes);
        }
    }
}
