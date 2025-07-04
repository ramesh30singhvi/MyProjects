using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
    public class RearrangeQuestionsModel
    { 
        public IReadOnlyCollection<RearrangeQuestionModel> Questions { get; set; }
    }

    public class RearrangeQuestionModel
    {
        public int Id { get; set; }

        public int? GroupId { get; set; }

        public int Sequence { get; set; }
    }
}
