using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Audit
{
    public class CriteriaQuestionDto
    {
        public int Id { get; set; }

        public string Value { get; set; }

        public int? Sequence { get; set; }

        public int? GroupId { get; set; }

        public int? ParentId { get; set; }

        public CriteriaOptionDto CriteriaOption { get; set; }

        public CriteriaAnswerDto Answer { get; set; }

        public IReadOnlyCollection<CriteriaQuestionDto> SubQuestions { get; set; }
    }
}
