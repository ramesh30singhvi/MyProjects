using SHARP.ViewModels.Audit;
using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
    public class CriteriaFormDetailsModel : FormDetailsModel
    {
        public IReadOnlyCollection<CriteriaQuestionGroupModel> QuestionGroups { get; set; }

        public IReadOnlyCollection<FormFieldModel> FormFields { get; set; }
    }
}
