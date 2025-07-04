using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Form
{
    public class TrackerFormDetailsDto : FormVersionDto
    {
        public IReadOnlyCollection<TrackerQuestionDto> Questions { get; set; }
    }
}
