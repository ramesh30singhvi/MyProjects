using System.Collections.Generic;

namespace SHARP.ViewModels.Audit
{
    public class CriteriaQuestionGroupModel
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public int Sequence { get; set; }

        public IReadOnlyCollection<CriteriaQuestionModel> Questions { get; set; }
    }
}
