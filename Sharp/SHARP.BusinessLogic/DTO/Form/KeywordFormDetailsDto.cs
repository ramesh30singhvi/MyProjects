using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Form
{
    public class KeywordFormDetailsDto : FormVersionDto
    {
        public IReadOnlyCollection<KeywordOptionDto> Keywords { get; set; }
    }
}
