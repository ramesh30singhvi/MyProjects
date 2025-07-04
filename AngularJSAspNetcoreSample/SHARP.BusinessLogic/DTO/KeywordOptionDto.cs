using SHARP.BusinessLogic.DTO.Form;
using System;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO
{
    public  class KeywordOptionDto : OptionDto
    {
        public bool? Trigger { get; set; }

        public ICollection<OptionDto> FormsTriggeredByKeyword { get; set; }
    }
}
