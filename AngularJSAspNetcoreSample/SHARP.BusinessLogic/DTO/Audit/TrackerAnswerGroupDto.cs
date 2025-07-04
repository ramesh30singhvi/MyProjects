using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Audit
{
    public class TrackerAnswerGroupDto
    {
        public string GroupId { get; set; }

        public IReadOnlyCollection<TrackerAnswerDto> Answers { get; set; }

        public int MaxId { get; set; }
    }
}
