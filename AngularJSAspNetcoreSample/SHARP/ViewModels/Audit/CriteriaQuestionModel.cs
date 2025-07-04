using SHARP.ViewModels.Form;
using System.Collections.Generic;

namespace SHARP.ViewModels.Audit
{
    public class CriteriaQuestionModel
    {
        public int Id { get; set; }

        public string Value { get; set; }

        public int? Sequence { get; set; }

        public int? GroupId { get; set; }

        public int? ParentId { get; set; }

        public CriteriaOptionModel CriteriaOption { get; set; }

        public CriteriaAnswerModel Answer { get; set; }

        public IReadOnlyCollection<CriteriaQuestionModel> SubQuestions { get; set; }
    }
}
