
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Form
{
    public class AddFormKeywordDto
    {
        public int FormVersionId { get; set; }

        public string Name { get; set; }

        public int? Hidden { get; set; }

        public bool? Trigger { get; set; }

        public IReadOnlyCollection<OptionDto> FormsTriggeredByKeyword { get; set; }
    }
}
