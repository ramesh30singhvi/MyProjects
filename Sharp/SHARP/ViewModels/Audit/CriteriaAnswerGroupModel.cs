using System.Collections.Generic;

namespace SHARP.ViewModels.Audit
{
    public class CriteriaAnswerGroupModel
    {
        public string Name { get; set; }

        public IEnumerable<CriteriaAnswerModel> Answers { get; set; }
    }
}
