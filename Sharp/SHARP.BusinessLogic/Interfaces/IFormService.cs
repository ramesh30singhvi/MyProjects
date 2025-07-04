using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.BusinessLogic.DTO.Common;
using SHARP.BusinessLogic.DTO.Form;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Interfaces
{
    public interface IFormService
    {
        Task<IEnumerable<FormVersionDto>> GetFormVersionsAsync(FormVersionFilter filter);

        Task<IEnumerable<FormVersionDto>> GetFormVersionsAsyncByFormId(int formId);

        Task<IReadOnlyCollection<FilterOption>> GetFilterColumnSourceDataAsync(FormManagementFilterColumnSource<FormVersionColumn> columnData);

        Task<IReadOnlyCollection<OptionDto>> GetTypeOptionsAsync();

        Task<FormVersionDto> AddFormAsync(AddFormDto formAddDto);

        Task<FormVersion> GetFormVersionAsync(int formVersionId);

        Task<FormVersionDto> GetFormVersionDetailsAsync(int formVersionId);

        Task<FormVersionDto> GetFormVersionDetailsAsyncForManagement(int formVersionId);

        Task<bool> IsFormActive(int formVersionId);

        Task<bool> IsFormNameAlreadyExist(string formName, int organizationId, int? formId = null);

        Task<FormVersionDto> DeleteFormVersionAsync(int formVersionId);

        Task PublishFormVersionAsync(int formVersionIdid, int allowEmptyComment, int disableCompliance, bool useHighAlert,int ahTime);

        Task<FormVersionDto> EditFormVersionAsync(int formVersionId);
        
        Task<KeywordOptionDto> AddFormKeywordAsync(AddFormKeywordDto addKeywordDto);

        Task<OptionDto> AddFormCustomKeywordAsync(AddFormKeywordDto addKeywordDto);

        Task<bool> EditFormKeywordAsync(EditFormKeywordDto editKeyword);

        Task<FormVersionDto> DeleteTableColumnAsync(int keywordId);

        Task<bool> RearrangeQuestionsAsync(RearrangeQuestionsDto rearrangeQuestionsDto);

        Task<IReadOnlyCollection<OptionDto>> GetSectionOptionsAsync(int formVersionId);

        Task<IReadOnlyCollection<MdsSectionDto>> GetMdsSectionsAsync(int formVersionId);

        Task<FormVersionDto> AddCriteriaQuestionAsync(AddCriteriaQuestionDto addQuestionDto);

        Task<FormVersionDto> AddMdsGroupAsync(AddMdsGroupDto addMdsGroupDto);

        Task<FormVersionDto> AddMdsSectionAsync(AddMdsSectionDto addMdsSectionDto);
        
        Task<FormVersionDto> EditMdsGroupAsync(EditMdsGroupDto editMdsGroupDto);

        Task<FormVersionDto> EditMdsSectionAsync(EditMdsSectionDto editMdsSectionDto);

        Task<FormVersionDto> EditCriteriaQuestionAsync(EditCriteriaQuestionDto editQuestionDto);

        Task<FormVersionDto> DeleteQuestionAsync(int id);

        Task<IReadOnlyCollection<OptionDto>> GetFieldTypeOptionsAsync();

        Task<FormVersionDto> AddFormFieldAsync(AddFormFieldDto addFormFieldDto);

        Task<FormVersionDto> EditFormFieldAsync(EditFormFieldDto editFormFieldDto);

        Task<FormVersionDto> DeleteFormFieldAsync(int formVersionId, int fieldId);

        Task<bool> RearrangeFormFieldsAsync(int formVersionId, RearrangeItemsDto rearrangeItemsDto);

        Task<FormVersionDto> EditFormSectionsAsync(int formVersionId, EditQuestionGroupsDto editQuestionGroupsDto);

        Task<FormVersionDto> AddTrackerQuestionAsync(AddTrackerQuestionDto addQuestionDto);

        Task<FormVersionDto> AddMdsQuestionAsync(AddTrackerQuestionDto addQuestionDto);
        
        Task<FormVersionDto> EditMdsQuestionAsync(EditTrackerQuestionDto editQuestionDto);

        Task<FormVersionDto> EditTrackerQuestionAsync(EditTrackerQuestionDto editQuestionDto);

        Task<IReadOnlyCollection<FormOptionDto>> GetFormVersionOptionsAsync(int organizationId, string auditType);

        Task<IReadOnlyCollection<FormOptionDto>> GetFormOptionsAsync(int organizationId, string auditType);

        Task<IReadOnlyCollection<FormOptionDto>> GetFormOptionsAsync(FormOptionFilter filter);

        Task<bool> SetFormStateAsync(int formId, bool state);

        Task<int> GetLastActiveFormVersionIdAsync(int formId);
        Task EditFormNameAsync(int formId, string formName);
        Task DuplicateFormAsync(int formId, int organizationId);
        bool FormExists(int id, string name);

        Task<bool> DeleteFormAsync(int id);
        Task<TableColumn[]> GetQuestions(int[] FormVersionIds);
        Task<IReadOnlyCollection<OptionDto>> GetFormsTriggeredAsync(int organizationId,string keyword);
        Task<IReadOnlyCollection<OptionDto>> GetAuditTypes();
    }
}
