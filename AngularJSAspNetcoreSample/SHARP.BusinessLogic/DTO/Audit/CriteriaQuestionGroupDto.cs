using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Audit
{
    public class CriteriaQuestionGroupDto
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public int? Sequence { get; set; }

        public IReadOnlyCollection<CriteriaQuestionDto> Questions { get; set; }
    }
}
