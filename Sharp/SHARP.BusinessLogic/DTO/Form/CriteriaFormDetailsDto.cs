using SHARP.BusinessLogic.DTO.Audit;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Form
{
    public class CriteriaFormDetailsDto : FormVersionDto
    {
        public IReadOnlyCollection<CriteriaQuestionGroupDto> QuestionGroups { get; set; }

        public IReadOnlyCollection<FormFieldDto> FormFields { get; set; }
    }
}
