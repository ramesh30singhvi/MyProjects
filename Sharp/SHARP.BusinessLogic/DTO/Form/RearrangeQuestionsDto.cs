using SHARP.Common.Constants;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Form
{
    public class RearrangeQuestionsDto
    {
        public IReadOnlyCollection<RearrangeQuestionDto> Questions { get; set; }
    }

    public class RearrangeQuestionDto: IIdModel<int>
    {
        public int Id { get; set; }

        public int? GroupId { get; set; }

        public int Sequence { get; set; }
    }
}
