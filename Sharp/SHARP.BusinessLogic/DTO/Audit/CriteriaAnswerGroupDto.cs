using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Audit
{
    public class CriteriaAnswerGroupDto
    {
        public string Name { get; set; }

        public IReadOnlyCollection<CriteriaAnswerDto> Answers { get; set; }
    }
}
